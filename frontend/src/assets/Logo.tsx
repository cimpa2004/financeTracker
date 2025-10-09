import { Box } from "@mui/material";
import Logo from '/logo.png';

interface MainLogoProps {
  width?: number;
  height?: number;
}

export default function MainLogo({ width, height }: MainLogoProps) {
  return (
    <Box width={width || 200} height={height || 200}>
      <img src={Logo} alt="App Logo" style={{ width: '100%', height: '100%' }} />
    </Box>
  );
}