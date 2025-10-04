import { Box, Button, Paper, Stack } from "@mui/material";
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import LoginIcon from '@mui/icons-material/Login';
import LogoutIcon from '@mui/icons-material/Logout';
import NavItem from "./NavItem";
import { ROUTES } from "../../constants";
import { useAuth } from "../../contexts/AuthContext";
import { useNavigate } from "react-router-dom";

interface NavDrawerContentProps {
  closeDrawer: () => void;
}

export default function NavDrawerContent({ closeDrawer }: NavDrawerContentProps) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  return (
    <Paper elevation={3} sx={{ width: 250, height: "100%" }}>
      <Stack direction="column" justifyContent="space-between" height="100%">
        <Stack
        spacing={2}
        direction="column"
        justifyContent="flex-start"
        alignItems="stretch"
        sx={{ padding: 2 }}
      >
        <Box display="flex" justifyContent="flex-start" p={1}>
          <Button variant="contained" color="primary" onClick={closeDrawer} startIcon={<ArrowBackIcon fontSize="small" />}>
            Back
          </Button>
        </Box>
        <Box>
          <NavItem name="Home" route={ROUTES.HOME} closeDrawer={closeDrawer} />
          <NavItem name="Add Transaction" route={ROUTES.ADD_TRANSACTION} closeDrawer={closeDrawer} />
          <NavItem name="Add Category" route={ROUTES.ADD_CATEGORY} closeDrawer={closeDrawer} />
        </Box>
        </Stack>
        <Box sx={{ p: 2 }}>
          <Button
            fullWidth
            variant={user ? "outlined" : "contained"}
            color="primary"
            startIcon={user ? <LogoutIcon /> : <LoginIcon />}
            onClick={async () => {
              if (user) {
                if (typeof logout === "function") await logout();
                closeDrawer();
              } else {
                navigate(ROUTES.LOGIN);
                closeDrawer();
              }
            }}
          >
            {user ? "Logout" : "Login"}
          </Button>
        </Box>
      </Stack>

    </Paper>
  );
}