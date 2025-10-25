import { AppBar, Box, Toolbar, IconButton, Drawer } from "@mui/material";
import MenuIcon from '@mui/icons-material/Menu';
import LightModeIcon from '@mui/icons-material/LightMode';
import DarkModeIcon from '@mui/icons-material/DarkMode';
import { useState } from "react";
import NavDrawerContent from "./NavDrawerContent";
import { useColorMode } from '../../contexts/colorModeContext';

export default function Navbar() {
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const closeDrawer = () => setIsDrawerOpen(false);
    return (
        <Box>
            <AppBar position="sticky">
                <Toolbar sx={{ justifyContent: "space-between" }}>
                    <Box display="flex" alignItems="center">
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
                    </Box>
                    <Box>
                      <ThemeToggle />
                    </Box>
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

function ThemeToggle() {
    const { mode, toggleMode } = useColorMode();
    return (
        <IconButton color="inherit" onClick={toggleMode} aria-label="toggle theme">
            {mode === 'light' ? <DarkModeIcon /> : <LightModeIcon />}
        </IconButton>
    );
}