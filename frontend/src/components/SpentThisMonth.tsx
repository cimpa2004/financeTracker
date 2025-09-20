import { Box, Divider, Typography } from "@mui/material";

export default function SpentThisMonth() {
  return (
    <Box display={"flex"} flexDirection="column" alignItems="center" p={2}>
        <Typography variant="h4" gutterBottom>
            Spent This Month
        </Typography>
        <Divider sx={{ width: "100%" }} />
        <Typography variant="h5">
           Dummy text
        </Typography>
    </Box>
  );
}
