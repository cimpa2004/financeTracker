import { Box } from "@mui/material";
import PagedTransactions from "../components/PagedTransactions";

export default function AllTransactions() {
  return (
    <Box justifyContent={'center'} alignItems={'center'} display={'flex'} width={'100%'} p={2}>
      <PagedTransactions />
    </Box>
  )
 }