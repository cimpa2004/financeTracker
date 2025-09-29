import { Controller, useForm } from 'react-hook-form';
import {
  Box,
  TextField,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
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
    const normalizeHex = (val?: string | null) => {
      const v = (val ?? '').trim();
      if (!v) return '';
      const noHashes = v.replace(/^#+/, '');
      return '#' + noHashes;
    };

    const hexedData = { ...data, color: normalizeHex(data.color) };
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
              <TextField
                label="Color (hex)"
                value={field.value ?? ''}
                onChange={(e) => field.onChange(e.target.value)}
                error={!!errors.color}
                helperText={errors.color?.message}
                  slotProps={{
                    input: {
                      startAdornment: (
                        <Stack direction="row">
                          <ColorPicker
                            value={field.value ?? ''}
                            format="hex"
                            onChange={(e) => field.onChange(e.value)}
                            defaultColor='1976d2'
                            inputStyle={{ border: 'none', width: 36, height: '100%', marginRight: 8 }}
                          />
                          #
                        </Stack>
                      ),
                    },
                  }}
                />
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