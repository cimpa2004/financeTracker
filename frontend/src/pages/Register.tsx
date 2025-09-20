import { Box, Button, TextField, Typography, Checkbox, FormControlLabel } from "@mui/material";
import { useForm } from "react-hook-form";
import { Link, useNavigate } from "react-router-dom";
import { useSmallScreen } from "../hooks/useSmallScreen";
import { ROUTES } from "../constants";
import { registerAccount } from "../apis/Auth";


type RegisterForm = {
    username: string;
    email: string;
    password: string;
    confirmPassword: string;
    acceptTerms: boolean;
};

export default function Register() {
    const isSmallScreen = useSmallScreen();
    const navigate = useNavigate();

    const {
        register,
        handleSubmit,
        watch,
        formState: { errors, isSubmitting }
    } = useForm<RegisterForm>({ mode: "onTouched" });

    const onSubmit = async (data: RegisterForm) => {
        const response = await registerAccount({ username: data.username, email: data.email, password: data.password });
        if (response?.userId) {
            navigate(ROUTES.LOGIN);
        }
    };

    const password = watch("password", "");

    return (
        <Box
            display="flex"
            flexDirection="column"
            alignItems="center"
            justifyContent="center"
            height="100vh"
            gap={2}
            width={isSmallScreen ? "90%" : "30%"}
            margin="0 auto"
        >
            <Typography variant="h3" color="primary">Register</Typography>

            <TextField
                label="Username"
                variant="outlined"
                fullWidth
                {...register("username", {
                    required: "Username is required",
                    maxLength: { value: 255, message: "Username cannot exceed 255 characters" }
                })}
                error={!!errors.username}
                helperText={errors.username?.message}
            />

            <TextField
                label="Email"
                variant="outlined"
                fullWidth
                {...register("email", {
                    required: "Email is required",
                    pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: "Invalid email address" }
                })}
                error={!!errors.email}
                helperText={errors.email?.message}
            />

            <TextField
                label="Password"
                type="password"
                variant="outlined"
                fullWidth
                {...register("password", {
                    required: "Password is required",
                    minLength: { value: 8, message: "Password must be at least 8 characters" }
                })}
                error={!!errors.password}
                helperText={errors.password?.message}
            />

            <TextField
                label="Confirm password"
                type="password"
                variant="outlined"
                fullWidth
                {...register("confirmPassword", {
                    required: "Please confirm your password",
                    validate: (val) => val === password || "Passwords do not match"
                })}
                error={!!errors.confirmPassword}
                helperText={errors.confirmPassword?.message}
            />

            <FormControlLabel sx={{ alignSelf: "flex-start" }}
                control={
                    <Checkbox
                        {...register("acceptTerms", {
                            required: "You must accept the terms and conditions"
                        })}
                        color="primary"
                    />
                }
                label="I accept the terms and conditions"
            />
            {errors.acceptTerms && (
                <Typography color="error" variant="body2">{errors.acceptTerms.message}</Typography>
            )}

            <Button variant="contained" fullWidth onClick={handleSubmit(onSubmit)} disabled={isSubmitting}>
                Register
            </Button>

            <Box display="flex" flexDirection="row" alignItems="center">
                <Typography variant="body1">Already have an account?</Typography>
                <Button component={Link} to={ROUTES.LOGIN} variant="text">Login</Button>
            </Box>
        </Box>
    );
}