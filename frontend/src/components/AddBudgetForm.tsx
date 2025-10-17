import { Controller, useForm } from 'react-hook-form';
import { useEffect, useState } from 'react';
import { useSmallScreen } from '../hooks/useSmallScreen';
import { zodResolver } from '@hookform/resolvers/zod';
import {
	Box,
	TextField,
	Button,
	FormControl,
	InputLabel,
	Select,
	MenuItem,
	FormHelperText,
	Stack,
	Typography,
	Alert,
	InputAdornment,
} from '@mui/material';

import { BudgetFormSchema, type BudgetFormInput } from '../types/Budget';
import { useAddBudget, useUpdateBudget } from '../apis/Budget';
import { useCategories } from '../apis/Category';
import getRangeForInterval from '../utils/intervalRange';

type AddBudgetFormProps = {
	onSuccess?: () => void;
	budgetId?: string | null;
	initialValues?: Partial<BudgetFormInput>;
};

export default function AddBudgetForm({ onSuccess, budgetId = null, initialValues }: AddBudgetFormProps) {
	const { data: categories = [] } = useCategories();
	const { mutateAsync: addBudgetAsync } = useAddBudget();
	const { mutateAsync: updateBudgetAsync } = useUpdateBudget(budgetId ?? null);
	const [mutationError, setMutationError] = useState<string | null>(null);

	const {
		control,
		handleSubmit,
		reset,
		register,
		setValue,
		watch,
		formState: { errors, isSubmitting },
	} = useForm<BudgetFormInput>({
		resolver: zodResolver(BudgetFormSchema),
		defaultValues: {
			categoryId: null,
			amount: 0,
			name: '',
			interval: 'monthly',
			startDate: '',
			endDate: '',
		},
	});

	// if initial values are provided (edit mode), populate the form
	useEffect(() => {
		if (initialValues) {
			if (initialValues.categoryId !== undefined) setValue('categoryId', initialValues.categoryId ?? null);
			if (initialValues.amount !== undefined) setValue('amount', initialValues.amount as number);
			if (initialValues.name !== undefined) setValue('name', initialValues.name ?? '');
			if (initialValues.interval !== undefined) setValue('interval', initialValues.interval as 'weekly' | 'monthly' | 'yearly' | 'All time');
			if (initialValues.startDate !== undefined) setValue('startDate', initialValues.startDate ?? '');
			if (initialValues.endDate !== undefined) setValue('endDate', initialValues.endDate ?? '');
		}
	}, [initialValues, setValue]);

	const onSubmit = async (data: BudgetFormInput) => {
		setMutationError(null);
		// normalize categoryId and dates
		const normalizedCategoryId = data.categoryId === '' ? null : data.categoryId;
		let payloadStart = data.startDate ?? null;
		let payloadEnd = data.endDate ?? null;
		if (data.interval === 'All time') {
			payloadStart = null;
			payloadEnd = null;
		}
		const payload: BudgetFormInput = { ...data, categoryId: normalizedCategoryId, startDate: payloadStart, endDate: payloadEnd };

		try {
			if (budgetId) {
				await updateBudgetAsync(payload);
				if (onSuccess) onSuccess();
			} else {
				await addBudgetAsync(payload);
				reset({ categoryId: null, amount: 0, name: '', interval: 'monthly', startDate: '', endDate: '' });
				if (onSuccess) onSuccess();
			}
		} catch (e: unknown) {
			const msg = e instanceof Error ? e.message : String(e);
			setMutationError(msg || 'Failed to save budget');
		}
	};

	const isSmall = useSmallScreen();

	// use shared utility getRangeForInterval(interval) -> { start, end }

	// initialize and respond to interval changes
	const watchedInterval = watch('interval');
	useEffect(() => {
		const { start, end } = getRangeForInterval(watchedInterval || 'monthly');
		// set nullable values; react-hook-form accepts null for nullable fields
		setValue('startDate', start ?? '', { shouldDirty: true });
		setValue('endDate', end ?? '', { shouldDirty: true });
	}, [watchedInterval, setValue]);

	let submitLabel = 'Add budget';
	if (isSubmitting) submitLabel = 'Saving...';
	else if (budgetId) submitLabel = 'Save changes';

	return (
		<Box sx={{ width: '100%', display: 'flex', justifyContent: 'center', p: 2 }}>
			<Box sx={{ width: '100%', maxWidth: 520, mx: 'auto' }}>
			<Typography variant="h6" gutterBottom>
				Add Budget
			</Typography>

			{mutationError && (
				<Alert severity="error" sx={{ mb: 2 }}>
					{mutationError}
				</Alert>
			)}

			<form onSubmit={handleSubmit(onSubmit)} noValidate>
				<Stack spacing={2}>
					<Box sx={{ display: 'flex', gap: 2, flexDirection: isSmall ? 'column' : 'row' }}>
						<Box sx={{ flex: 2 }}>
							<TextField
								label="Name"
								{...register('name')}
								error={!!errors.name}
								helperText={errors.name?.message}
								fullWidth
							/>
						</Box>

						<Box sx={{ flex: 1 }}>
							<TextField
								label="Amount"
								{...register('amount', { valueAsNumber: true })}
								error={!!errors.amount}
								helperText={errors.amount?.message}
								fullWidth
								type="number"
								inputMode="decimal"
								slotProps={{ input: { endAdornment: <InputAdornment position="end">Ft</InputAdornment> } }}
							/>
						</Box>
					</Box>

					<Box sx={{ display: 'flex', gap: 2, flexDirection: isSmall ? 'column' : 'row' }}>
						<Box sx={{ flex: 1 }}>
							<Controller
								name="interval"
								control={control}
								render={({ field }) => (
									<FormControl fullWidth error={!!errors.interval}>
										<InputLabel id="budget-interval-label">Interval</InputLabel>
										<Select {...field} labelId="budget-interval-label" label="Interval">
											<MenuItem value="weekly">Weekly</MenuItem>
											<MenuItem value="monthly">Monthly</MenuItem>
											<MenuItem value="yearly">Yearly</MenuItem>
													<MenuItem value="All time">All time</MenuItem>
										</Select>
										{errors.interval && <FormHelperText>{errors.interval?.message}</FormHelperText>}
									</FormControl>
								)}
							/>
						</Box>

						<Box sx={{ flex: 2 }}>
							<Controller
								name="categoryId"
								control={control}
								render={({ field }) => (
									<FormControl fullWidth error={!!errors.categoryId}>
										<InputLabel id="budget-category-label">Category</InputLabel>
										<Select
											{...field}
											value={field.value ?? ''}
											onChange={(e) => {
												const v = (e.target as HTMLInputElement).value;
												field.onChange(v === '' ? null : v);
											}}
											labelId="budget-category-label"
											label="Category"
										>
											<MenuItem value="">
												<em>All categories</em>
											</MenuItem>
											{categories.filter(c => c.type === 'expense').map((c) => (
												<MenuItem key={c.categoryId} value={c.categoryId}>
													{c.name}
												</MenuItem>
											))}
										</Select>
										{errors.categoryId && <FormHelperText>{errors.categoryId.message}</FormHelperText>}
									</FormControl>
								)}
							/>
						</Box>
					</Box>
					<Box>
						<Button type="submit" variant="contained" color="primary" disabled={isSubmitting} fullWidth>
							{submitLabel}
						</Button>
					</Box>
				</Stack>
			</form>
			</Box>
		</Box>
	);
}