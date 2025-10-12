import { Box, Button, Paper, Stack } from "@mui/material";
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import LoginIcon from '@mui/icons-material/Login';
import LogoutIcon from '@mui/icons-material/Logout';
import HomeIcon from '@mui/icons-material/Home';
import AddShoppingCartIcon from '@mui/icons-material/AddShoppingCart';
import CategoryIcon from '@mui/icons-material/Category';
import PieChartIcon from '@mui/icons-material/PieChart';
import NavItem from "./NavItem";
import { ROUTES } from "../../constants";
import { useAuth } from "../../contexts/AuthContext";
import { useNavigate } from "react-router-dom";

type PageDef = {
  name: string;
  route: string;
  icon?: React.ReactNode;
};

const PAGES: PageDef[] = [
  { name: 'Home', route: ROUTES.HOME, icon: <HomeIcon /> },
  { name: 'Add Transaction', route: ROUTES.ADD_TRANSACTION, icon: <AddShoppingCartIcon /> },
  { name: 'Add Category', route: ROUTES.ADD_CATEGORY, icon: <CategoryIcon /> },
  { name: 'Budget Charts', route: ROUTES.BUDGET_CHARTS, icon: <PieChartIcon /> },
  { name: 'Add Budget', route: ROUTES.ADD_BUDGET, icon: <PieChartIcon /> },
];

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
          {PAGES.map(p => (
            <NavItem key={p.route} name={p.name} route={p.route} icon={p.icon} closeDrawer={closeDrawer} />
          ))}
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
                navigate(ROUTES.INDEX);
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