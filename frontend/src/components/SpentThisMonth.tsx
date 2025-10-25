import { Box, Divider, Typography, CircularProgress  } from "@mui/material";
import { useGetSpentLastMonth } from "../apis/SpentLastMonth";
import { useMemo } from "react";


export default function SpentThisMonth() {
  const { data, isLoading, error } = useGetSpentLastMonth();
  const formattedAmount = useMemo(() => {
    const amountNumber = Number(data?.spent ?? 0);
    return new Intl.NumberFormat('hu-HU', { style: 'currency', currency: 'HUF', maximumFractionDigits: 0 }).format(amountNumber);
  }, [data]);

  if (isLoading) {
    return <CircularProgress />;
  }
  if (error) {
    return <Box>Error loading spent this month</Box>;
  }
  return (
    <Box display={"flex"} flexDirection="column" alignItems="center" p={2}>
        <Typography variant="h4" gutterBottom>
            Spent This Month
        </Typography>
        <Divider sx={{ width: "100%" }} />
        <Typography variant="h5">
           {formattedAmount}
        </Typography>
    </Box>
  );
}
