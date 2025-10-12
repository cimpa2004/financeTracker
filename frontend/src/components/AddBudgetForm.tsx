import { Controller, useForm } from 'react-hook-form';
import { useEffect, useCallback } from 'react';
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
import { useAddBudget } from '../apis/Budget';
import { useCategories } from '../apis/Category';

type AddBudgetFormProps = {
	onSuccess?: () => void;
};

export default function AddBudgetForm({ onSuccess }: AddBudgetFormProps) {
	const { data: categories = [] } = useCategories();
	const { mutate: addBudget, isPending, isError, error, isSuccess } = useAddBudget();

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

	const onSubmit = (data: BudgetFormInput) => {
		// if the user selected "All categories" the select sets an empty string; send null to the API
		const payload: BudgetFormInput = { ...data, categoryId: data.categoryId === '' ? null : data.categoryId };

		addBudget(payload, {
			onSuccess: () => {
				reset({ categoryId: null, amount: 0, name: '', interval: 'monthly', startDate: '', endDate: '' });
				if (onSuccess) onSuccess();
			},
		});
	};

	const isSmall = useSmallScreen();

	// helper to format Date -> YYYY-MM-DD
	const fmt = (d: Date) => d.toISOString().slice(0, 10);

	const getRangeForInterval = useCallback((interval: string) => {
		const now = new Date();
		if (interval === 'weekly') {
			// week starting Monday
			const day = now.getDay();
			const diff = (day + 6) % 7; // days since Monday
			const start = new Date(now);
			start.setDate(now.getDate() - diff);
			start.setHours(0, 0, 0, 0);
			const end = new Date(start);
			end.setDate(start.getDate() + 6);
			end.setHours(23, 59, 59, 999);
			return { start: fmt(start), end: fmt(end) };
		}
		if (interval === 'yearly') {
			const start = new Date(now.getFullYear(), 0, 1);
			const end = new Date(now.getFullYear(), 11, 31);
			return { start: fmt(start), end: fmt(end) };
		}
		// default monthly
		const start = new Date(now.getFullYear(), now.getMonth(), 1);
		const end = new Date(now.getFullYear(), now.getMonth() + 1, 0);
		return { start: fmt(start), end: fmt(end) };
	}, []);

	// initialize and respond to interval changes
	const watchedInterval = watch('interval');
	useEffect(() => {
		const { start, end } = getRangeForInterval(watchedInterval || 'monthly');
		setValue('startDate', start, { shouldDirty: true });
		setValue('endDate', end, { shouldDirty: true });
	}, [getRangeForInterval, watchedInterval, setValue]);

	return (
		<Box sx={{ width: '100%', display: 'flex', justifyContent: 'center', p: 2 }}>
			<Box sx={{ width: '100%', maxWidth: 520, mx: 'auto' }}>
			<Typography variant="h6" gutterBottom>
				Add Budget
			</Typography>

			{isError && (
				<Alert severity="error" sx={{ mb: 2 }}>
					{error?.message || 'Failed to add budget'}
				</Alert>
			)}

			{isSuccess && (
				<Alert severity="success" sx={{ mb: 2 }}>
					Budget added
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
								slotProps={{ input: { startAdornment: <InputAdornment position="start">$</InputAdornment> } }}
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
											{categories.map((c) => (
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
						<Button type="submit" variant="contained" color="primary" disabled={isPending || isSubmitting} fullWidth>
							{isPending ? 'Saving...' : 'Add budget'}
						</Button>
					</Box>
				</Stack>
			</form>
			</Box>
		</Box>
	);
}