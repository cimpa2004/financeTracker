import { Box } from "@mui/material";
import Navbar from "../components/Navbar";
import { Outlet } from "react-router-dom";

export const AppLayout = () => {
  return (
    <Box display="flex" height={'100vh'} flexDirection="column">
      <Navbar />
      <Box component="main" flexGrow={1} display="flex">
        <Outlet />
      </Box>
    </Box>
  );
};