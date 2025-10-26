import { Box, Button, TextField, Typography } from "@mui/material";
import { useForm } from "react-hook-form";
import { useEffect } from 'react';
import { useNavigate } from "react-router-dom";
import { useSmallScreen } from "../hooks/useSmallScreen";
import { useAuth } from "../contexts/AuthContext";
import { useProfile, useUpdateProfile } from "../apis/User";
import { ROUTES } from "../constants";

type ProfileForm = {
  username: string;
  email: string;
  password?: string;
  confirmPassword?: string;
};

export default function Profile() {
  const isSmallScreen = useSmallScreen();
  const navigate = useNavigate();
  const { user } = useAuth();

  const { data: profileData } = useProfile();

  const { register, handleSubmit, watch, reset, formState: { errors } } = useForm<ProfileForm>({ mode: 'onTouched', defaultValues: { username: user?.username ?? '', email: user?.email ?? '' } });

  const mutation = useUpdateProfile();

  const password = watch('password', '');

  // reset form when fetched profile or auth user changes
  useEffect(() => {
    const src = (profileData ?? user) as { username?: string; email?: string } | undefined;
    reset({ username: src?.username ?? '', email: src?.email ?? '' });
  }, [profileData, user, reset]);

  const onSubmit = (data: ProfileForm) => {
    if (data.password && data.password !== data.confirmPassword) return;
    const payload: { username?: string; email?: string; password?: string } = { username: data.username, email: data.email };
    if (data.password) payload.password = data.password;

    mutation.mutate(payload);
  };

  return (
    <Box display="flex" flexDirection="column" alignItems="center" justifyContent="center" gap={2} width={isSmallScreen ? '92%' : '40%'} margin="0 auto" mt={4}>
      <Typography variant="h4">Edit Profile</Typography>

      <TextField label="Username" variant="outlined" fullWidth {...register('username', { required: 'Username is required', maxLength: { value: 255, message: 'Username too long' } })} error={!!errors.username} helperText={errors.username?.message} />

      <TextField label="Email" variant="outlined" fullWidth {...register('email', { required: 'Email is required', pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: 'Invalid email' } })} error={!!errors.email} helperText={errors.email?.message} />

      <TextField label="New password" type="password" variant="outlined" fullWidth {...register('password', { minLength: { value: 8, message: 'Password must be at least 8 characters' } })} error={!!errors.password} helperText={errors.password?.message} />

      <TextField label="Confirm new password" type="password" variant="outlined" fullWidth {...register('confirmPassword', { validate: (val) => val === password || 'Passwords do not match' })} error={!!errors.confirmPassword} helperText={errors.confirmPassword?.message} />

      <Box width="100%" display="flex" gap={2}>
        <Button variant="outlined" onClick={() => navigate(ROUTES.HOME)} fullWidth>Cancel</Button>
        <Button variant="contained" onClick={handleSubmit(onSubmit)} disabled={mutation.status === 'pending'} fullWidth>Save</Button>
      </Box>
    </Box>
  );
}
