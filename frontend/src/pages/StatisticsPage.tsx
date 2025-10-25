import { Box } from "@mui/material";
import SpentByCategoryCard from "../components/SpentByCategoryCard";
import SpentByIntervalCard from "../components/SpentByIntervalCard";
import Masonry from '@mui/lab/Masonry';

export default function StatisticsPage() {
  return (
    <Box p={2} width="100%" sx={{ boxSizing: 'border-box' }}>
      <Masonry columns={{ xs: 1, sm: 2, md: 2 }} spacing={2}>
        <Box>
          <SpentByCategoryCard />
        </Box>
        <Box>
          <SpentByIntervalCard />
        </Box>
      </Masonry>
    </Box>
  );
}