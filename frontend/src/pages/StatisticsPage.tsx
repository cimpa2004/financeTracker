import { Box } from "@mui/material";
import SpentByCategoryCard from "../components/SpentByCategoryCard";

export default function StatisticsPage() {
  return (
    <Box p={2} width="100vw" justifyContent="center" alignItems="center">
      <Box
        display="grid"
        gridTemplateColumns={{ xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr' }}
        gap={2}
      >
        <SpentByCategoryCard />
      </Box>

    </Box>
  );
}