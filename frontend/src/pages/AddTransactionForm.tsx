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
  Alert,
  useMediaQuery,
  InputAdornment,
  Stack,
} from '@mui/material';

import { TransactionFormSchema, type TransactionFormInput } from '../types/Transaction';
import { useAddTransaction } from '../apis/Transaction';
import { useCategories } from '../apis/Category';
import { useAuth } from '../contexts/AuthContext';


export default function AddTransaction() {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const today = new Date().toISOString().slice(0, 10);
  const { user } = useAuth();
  const { data: categories = [] } = useCategories();
  const { mutate: addTransaction, isPending, isError, error } = useAddTransaction();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<TransactionFormInput>({
    resolver: zodResolver(TransactionFormSchema),
    defaultValues: {
      userId: user?.userId || '',
      amount: "0",
      name: '',
      date: today,
      categoryId: '',
    },
  });

  const onSubmit = (data: TransactionFormInput) => {
    addTransaction(
      { ...data, userId: user?.userId || '' },
      {
        onSuccess: () => {
          reset({
            userId: user?.userId || '',
            amount: "0",
            name: '',
            date: today,
            categoryId: '',
          });
        },
      }
    );
  };

  return (
    <Box sx={{ width: '100%', maxWidth: '600px', mx: 'auto', p: 2 }}>
      <Typography variant="h6" gutterBottom>
        Add Transaction
      </Typography>

      {isError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error?.message || 'An error occurred while adding the transaction'}
        </Alert>
      )}

      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <Stack spacing={2}>
          <Controller
            name="name"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Name*"
                fullWidth
                error={!!errors.name}
                helperText={errors.name?.message}
              />
            )}
          />

          <Box
            sx={{
              display: 'flex',
              flexDirection: isMobile ? 'column' : 'row',
              gap: 2,
            }}
          >
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
                  InputProps={{
                    startAdornment: <InputAdornment position="start">$</InputAdornment>,
                  }}
                  inputProps={{
                    inputMode: 'decimal',
                  }}
                  // ensure the displayed value is string
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
                <TextField
                  {...field}
                  label="Date"
                  type="date"
                  fullWidth
                  InputLabelProps={{ shrink: true }}
                  error={!!errors.date}
                  helperText={errors.date?.message}
                />
              )}
            />
          </Box>

          <Controller
            name="categoryId"
            control={control}
            render={({ field }) => (
              <FormControl fullWidth error={!!errors.categoryId}>
                <InputLabel id="category-label">Category*</InputLabel>
                <Select {...field} labelId="category-label" label="Category*">
                  <MenuItem value="">
                    <em>Select a category</em>
                  </MenuItem>
                  {categories.map((category) => (
                    <MenuItem key={category.categoryId} value={category.categoryId}>
                      {category.name}
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
              <TextField
                {...field}
                label="Description"
                multiline
                rows={3}
                fullWidth
                error={!!errors.description}
                helperText={errors.description?.message}
              />
            )}
          />

          <Box sx={{ mt: 1 }}>
            <Button
              type="submit"
              variant="contained"
              color="primary"
              fullWidth={isMobile}
              disabled={isPending || isSubmitting}
            >
              {isPending ? 'Adding...' : 'Add Transaction'}
            </Button>
          </Box>
        </Stack>
      </form>
    </Box>
  );
}