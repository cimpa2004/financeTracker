import { MenuItem, Stack, Typography } from "@mui/material";
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';
import { useNavigate } from "react-router-dom";

interface NavItemProps {
  name: string;
  route: string;
  closeDrawer: () => void;
  icon?: React.ReactNode;
}

export default function NavItem({ name, route, icon = <AccountBalanceWalletIcon />, closeDrawer }: NavItemProps) {
  const navigate = useNavigate();
  return (
    <MenuItem onClick={() => {
      navigate(route);
      closeDrawer();
    }}>
      <Stack direction="row" spacing={2} alignItems="space-between">
        <Typography>{icon}</Typography>
        <Typography>{name}</Typography>
      </Stack>
    </MenuItem>
  );
}