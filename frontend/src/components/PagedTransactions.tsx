import { useState } from 'react';
import { Box, Paper, Stack, Pagination, Select, MenuItem, Typography, CircularProgress } from '@mui/material';
import Transaction from './Transaction';
import { usePagedTransactions } from '../apis/Transaction';

export default function PagedTransactions() {
  const [page, setPage] = useState(1);
  const [size, setSize] = useState(10);

  const { data, isLoading, isError } = usePagedTransactions(page, size);

  if (isLoading) return (
    <Paper sx={{ p: 2, display: 'flex', justifyContent: 'center' }} elevation={2}>
      <CircularProgress />
    </Paper>
  );

  if (isError) return (
    <Paper sx={{ p: 2 }} elevation={2}>
      <Typography color="error">Failed to load transactions</Typography>
    </Paper>
  );

  return (
    <Box>
      <Stack spacing={1}>
        {data?.items?.map((t) => (
          <Transaction key={t.transactionId} Transaction={t} />
        ))}
      </Stack>

      <Box display="flex" justifyContent="space-between" alignItems="center" mt={2}>
        <Box>
          <Typography variant="body2">Rows per page:</Typography>
          <Select size="small" value={size} onChange={(e) => { setSize(Number(e.target.value)); setPage(1); }} sx={{ ml: 1 }}>
            <MenuItem value={5}>5</MenuItem>
            <MenuItem value={10}>10</MenuItem>
            <MenuItem value={20}>20</MenuItem>
            <MenuItem value={50}>50</MenuItem>
          </Select>
        </Box>

        <Pagination count={data ? data.totalPages : 1} page={page} onChange={(_, p) => setPage(p)} color="primary" />
      </Box>
    </Box>
  );
}
