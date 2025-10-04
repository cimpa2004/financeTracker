import { AppBar, Box, Toolbar, IconButton, Drawer} from "@mui/material";
import MenuIcon from '@mui/icons-material/Menu';
import { useState } from "react";
import NavDrawerContent from "./NavDrawerContent";

export default function Navbar() {
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const closeDrawer = () => setIsDrawerOpen(false);
    return (
        <Box>
            <AppBar position="static">
                <Toolbar sx={{ justifyContent: "space-between" }}>
                    <IconButton
                        size="large"
                        edge="start"
                        color="inherit"
                        aria-label="menu"
                        sx={{ mr: 2 }}
                        onClick={() => setIsDrawerOpen(true)}
                    >
                        <MenuIcon />
                    </IconButton>
                </Toolbar>
            </AppBar>
            <Drawer
                anchor="left"
                open={isDrawerOpen}
                onClose={() => setIsDrawerOpen(false)}
            >
                <NavDrawerContent closeDrawer={closeDrawer} />
            </Drawer>
        </Box>
    )
}