import { Stack, Typography, Button, Box, Paper } from "@mui/material";
import { useAuth } from "../contexts/AuthContext";
import MainLogo from "../assets/Logo";
import { useNavigate } from "react-router-dom";
import { ROUTES } from "../constants";

export default function WelcomePage() {
  const { user } = useAuth();
  const navigate = useNavigate();

  return (
    <Box
      component={Paper}
      elevation={0}
      sx={{
        minHeight: "60vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        p: 4,
        bgcolor: "transparent",
      }}
    >
      <Stack direction="column" gap={2} alignItems="center" textAlign="center">
        <MainLogo />
        <Typography variant="h4" fontWeight={700}>
          Trackaroo
        </Typography>
        {!user ? (
          <Button
            variant="contained"
            color="primary"
            onClick={() => navigate(ROUTES.LOGIN)}
            sx={{ mt: 1, minWidth: 140 }}
          >
            Login
          </Button>
        ) : (
          <Button
            variant="outlined"
            color="primary"
            onClick={() => navigate(ROUTES.HOME)}
            sx={{ mt: 1, minWidth: 140 }}
          >
            Go to Dashboard
          </Button>
        )}
      </Stack>
    </Box>
  );
}