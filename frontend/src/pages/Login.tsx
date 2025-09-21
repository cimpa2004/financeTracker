import { Box, Button, TextField, Typography } from "@mui/material";
import { useForm } from "react-hook-form";
import { Link, useNavigate } from "react-router-dom";
import { useSmallScreen } from "../hooks/useSmallScreen";
import { ROUTES } from "../constants";
import { login } from "../apis/Auth";
import { useAuth } from "../contexts/AuthContext";

type LoginForm = {
    email: string;
    password: string;
};

export default function Login() {
    const isSmallScreen = useSmallScreen();
    const navigate = useNavigate();
    const { setAuthData } = useAuth();
    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting }
    } = useForm<LoginForm>({ mode: "onTouched" });

    const onSubmit = async (data: LoginForm) => {
        const response = await login({ email: data.email, password: data.password }, setAuthData);
        if (response?.user) {
            navigate(ROUTES.HOME);
        }
    };

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
            <Typography variant="h3" color="primary">Login</Typography>

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

            <Button
                variant="contained"
                fullWidth
                onClick={handleSubmit(onSubmit)}
                disabled={isSubmitting}
            >
                Login
            </Button>

            <Box display="flex" flexDirection="row" alignItems="center" gap={1}>
                <Typography variant="body1">New here?</Typography>
                <Button component={Link} to={ROUTES.REGISTER} variant="text">Register</Button>
            </Box>
        </Box>
    );
}