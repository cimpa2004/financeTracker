import { Box } from "@mui/material";
import Logo from '/logo.png';

interface MainLogoProps {
  width?: number;
  height?: number;
}

export default function MainLogo({ width, height }: MainLogoProps) {
  return (
    <Box width={width || 300} height={height || 300}>
      <img src={Logo} alt="App Logo" style={{ width: '100%', height: '100%' }} />
    </Box>
  );
}