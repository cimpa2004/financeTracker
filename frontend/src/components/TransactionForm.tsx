import { useEffect, useState } from 'react';
import type { ElementType } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTheme } from '@mui/material/styles';
import {
  TextField,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  FormHelperText,
  Box,
  Typography,
  Stack,
  Alert,
  useMediaQuery,
} from '@mui/material';
import { LocalizationProvider, DatePicker } from '@mui/x-date-pickers';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';

import { TransactionFormSchema, type TransactionFormInput } from '../types/Transaction';
import { useCategories } from '../apis/Category';
import { useAddTransaction, useUpdateTransaction } from '../apis/Transaction';
import { useAuth } from '../contexts/AuthContext';
import ThemedScrollbar from './ThemedScrollbar';

type Props = {
  submitLabel?: string;
  defaultValues?: Partial<TransactionFormInput> & { transactionId?: string };
  onSuccess?: () => void;
};

export default function TransactionForm({ submitLabel = 'Add Transaction', defaultValues = {}, onSuccess }: Props) {
  const today = new Date().toISOString().slice(0, 10);
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const { user } = useAuth();
  const { data: categories = [] } = useCategories();
  const addMutation = useAddTransaction();
  const updateMutation = useUpdateTransaction();

  const { mutate: addTransaction } = addMutation;
  const { mutate: updateTransaction } = updateMutation;

  const [showError, setShowError] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);

  useEffect(() => {
    let t: ReturnType<typeof setTimeout> | undefined;
    if (addMutation.error || updateMutation.error) {
      setShowError(true);
      t = setTimeout(() => setShowError(false), 2000);
    }
    return () => {
      if (t) clearTimeout(t);
    };
  }, [addMutation.error, updateMutation.error]);

  useEffect(() => {
    let t: ReturnType<typeof setTimeout> | undefined;
    if (addMutation.isSuccess || updateMutation.isSuccess) {
      setShowSuccess(true);
      t = setTimeout(() => setShowSuccess(false), 2000);
    }
    return () => {
      if (t) clearTimeout(t);
    };
  }, [addMutation.isSuccess, updateMutation.isSuccess]);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<TransactionFormInput>({
    resolver: zodResolver(TransactionFormSchema),
    defaultValues: {
      userId: defaultValues.userId ?? user?.userId ?? '',
      amount: defaultValues.amount ?? '0',
      name: defaultValues.name ?? '',
      date: defaultValues.date ?? today,
      categoryId: defaultValues.categoryId ?? '',
      description: defaultValues.description ?? '',
    },
  });

  const editingId = defaultValues?.transactionId;
  const isEditing = Boolean(editingId);

  const internalSubmit = (data: TransactionFormInput) => {
    const payload = { ...data, userId: user?.userId || '' };
    if (isEditing && editingId) {
      updateTransaction(
        { id: editingId, payload },
        {
          onSuccess: () => {
            reset({
              userId: user?.userId || '',
              amount: '0',
              name: '',
              date: defaultValues.date ?? today,
              categoryId: '',
              description: '',
            });
            onSuccess?.();
          },
        }
      );
    } else {
      addTransaction(
        payload,
        {
          onSuccess: () => {
            reset({
              userId: user?.userId || '',
              amount: '0',
              name: '',
              date: defaultValues.date ?? today,
              categoryId: '',
              description: '',
            });
            onSuccess?.();
          },
        }
      );
    }
  };

  return (
    <Box sx={{ width: '100%', maxWidth: '600px', mx: 'auto', p: 2 }}>
      <Typography variant="h6" gutterBottom>
        {submitLabel}
      </Typography>

      {showError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {(() => {
            const err = addMutation.error ?? updateMutation.error;
            if (!err) return 'An error occurred while adding/updating the transaction';
            if (err instanceof Error) return err.message;
            if (typeof err === 'object' && err !== null && 'message' in err) {
              const m = (err as { message?: unknown }).message;
              if (typeof m === 'string') return m;
            }
            try {
              return JSON.stringify(err);
            } catch {
              return String(err);
            }
          })()}
        </Alert>
      )}

      {showSuccess && (
        <Alert severity="success" sx={{ mb: 2 }}>
          {isEditing ? 'Transaction updated successfully' : 'Transaction added successfully'}
        </Alert>
      )}

      <form onSubmit={handleSubmit(internalSubmit)} noValidate>
        <Stack spacing={2}>
          <Controller
            name="name"
            control={control}
            render={({ field }) => (
              <TextField {...field} label="Name*" fullWidth error={!!errors.name} helperText={errors.name?.message} />
            )}
          />

          <Box sx={{ display: 'flex', flexDirection: isMobile ? 'column' : 'row', gap: 2 }}>
            <Controller
              name="amount"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Amount*"
                  type="text"
                  fullWidth
                  inputMode="decimal"
                  value={field.value ?? ''}
                  onChange={(e) => field.onChange(e.target.value)}
                  error={!!errors.amount}
                  helperText={errors.amount?.message}
                />
              )}
            />

            <Controller
              name="date"
              control={control}
              render={({ field }) => (
                <LocalizationProvider dateAdapter={AdapterDateFns}>
                  <DatePicker
                    label="Date"
                    value={field.value ? new Date(field.value) : null}
                    onChange={(d: Date | null) => field.onChange(d ? d.toISOString().slice(0, 10) : '')}
                    slotProps={{ textField: { fullWidth: true, error: !!errors.date, helperText: errors.date?.message } }}
                  />
                </LocalizationProvider>
              )}
            />
          </Box>

          <Controller
            name="categoryId"
            control={control}
            render={({ field }) => (
              <FormControl fullWidth error={!!errors.categoryId}>
                <InputLabel id="category-label">Category*</InputLabel>
                <Select
                  {...field}
                  labelId="category-label"
                  label="Category*"
                  MenuProps={{
                    PaperProps: { component: ThemedScrollbar as ElementType },
                  }}
                >
                    <MenuItem value="">
                      <em>Select a category</em>
                    </MenuItem>
                    {categories.map((category) => (
                      <MenuItem key={category.categoryId} value={category.categoryId}>
                        {category.type} - {category.name}
                      </MenuItem>
                    ))}
                </Select>
                {errors.categoryId && <FormHelperText>{errors.categoryId.message}</FormHelperText>}
              </FormControl>
            )}
          />

          <Controller
            name="description"
            control={control}
            render={({ field }) => (
              <TextField {...field} label="Description" multiline rows={3} fullWidth error={!!errors.description} helperText={errors.description?.message} />
            )}
          />

          <Box sx={{ mt: 1 }}>
            <Button type="submit" variant="contained" color="primary" fullWidth={isMobile} disabled={(addMutation.isPending || updateMutation.isPending) || isSubmitting}>
              {(addMutation.isPending || updateMutation.isPending) || isSubmitting ? 'Submitting...' : submitLabel}
            </Button>
          </Box>
        </Stack>
      </form>
    </Box>
  );
}
