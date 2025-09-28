import { Controller, useForm } from 'react-hook-form';
import {
  Box,
  TextField,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormControlLabel,
  Checkbox,
  Stack,
  Alert,
} from '@mui/material';
import { useAddCategory } from '../apis/Category';
import type { AddCategoryInput } from '../types/Category';
import { ColorPicker } from 'primereact/colorpicker';

export default function AddCategoryForm() {
  const { mutate: addCategory, isPending, isError, error, isSuccess } = useAddCategory();

  const {
    control,
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<AddCategoryInput>({
    defaultValues: {
      name: '',
      icon: '',
      color: '#1976d2',
      type: 'expense',
      isPublic: false,
    },
  });

  const onSubmit = (data: AddCategoryInput) => {
    const hexedData = { ...data, color: '#' + data.color };
    addCategory(hexedData, {
      onSuccess: () => {
        reset();
      },
    });
  };

  return (
    <Box sx={{ width: '100%', maxWidth: 520, mx: 'auto', p: 2 }}>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <Stack spacing={2}>
          {isError && <Alert severity="error">{error?.message ?? 'Failed to add category'}</Alert>}
          {isSuccess && <Alert severity="success">Category added</Alert>}

          <TextField
            label="Name"
            {...register('name', { required: 'Name is required', maxLength: { value: 255, message: 'Max 255 chars' } })}
            error={!!errors.name}
            helperText={errors.name?.message}
            fullWidth
          />

          <TextField
            label="Icon (name)"
            {...register('icon', { maxLength: { value: 255, message: 'Max 255 chars' } })}
            error={!!errors.icon}
            helperText={errors.icon?.message}
            fullWidth
          />

          <Controller
            name="color"
            control={control}
            render={({ field }) => (
              <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
                <Box width={40} height={40} overflow="hidden"
                >
                  <ColorPicker
                    value={field.value ?? ''}
                    format="hex"
                    onChange={(e) => field.onChange(e.value)}
                    style={{ width: '100%', height: '100%', padding: 0, borderRadius: 0 }}
                  />
                </Box>

                <TextField
                  label="Color (hex)"
                  value={field.value ?? ''}
                  onChange={(e) => field.onChange(e.target.value)}
                  error={!!errors.color}
                  helperText={errors.color?.message}
                />
              </Box>
            )}
          />

          <FormControl fullWidth error={!!errors.type}>
            <InputLabel id="category-type-label">Type</InputLabel>
            <Controller
              name="type"
              control={control}
              render={({ field }) => (
                <Select labelId="category-type-label" label="Type" {...field}>
                  <MenuItem value="expense">Expense</MenuItem>
                  <MenuItem value="income">Income</MenuItem>
                </Select>
              )}
            />
          </FormControl>

          <Controller
            name="isPublic"
            control={control}
            render={({ field }) => (
              <FormControlLabel control={<Checkbox {...field} checked={field.value} />} label="Public category" />
            )}
          />

          <Box>
            <Button type="submit" variant="contained" color="primary" disabled={isPending} fullWidth>
              {isPending ? 'Saving...' : 'Add category'}
            </Button>
          </Box>
        </Stack>
      </form>
    </Box>
  );
}