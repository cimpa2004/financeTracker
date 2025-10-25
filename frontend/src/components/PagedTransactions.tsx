import { useCallback, useState } from 'react';
import { Box, Paper, Stack, Pagination, Select, MenuItem, Typography, CircularProgress, Dialog, DialogActions, Button, DialogContent, DialogTitle } from '@mui/material';
import Transaction from './Transaction';
import { usePagedTransactions } from '../apis/Transaction';
import type { Transaction as TransactionType } from '../types/Transaction';
import TransactionDetails from './TransactionDetails';
import ThemedScrollbar from './ThemedScrollbar';

export default function PagedTransactions() {
  const [page, setPage] = useState(1);
  const [size, setSize] = useState(5);
  const [openTransactionDetail, setOpenTransactionDetail] = useState(false);
  const [selectedTransaction, setSelectedTransaction] = useState<TransactionType | null>(null);

  const { data, isLoading, isError } = usePagedTransactions(page, size);

  const onClickTransaction = useCallback((transaction: TransactionType) => {
      setSelectedTransaction(transaction);
      setOpenTransactionDetail(true);
    }, []);

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
    <>
      <Box display="flex" justifyContent="center">
        <Box width="100%" maxWidth={800} sx={{ pb: 10 }}>
          <ThemedScrollbar maxHeight={'75vh'}>
            <Stack spacing={1}>
              {data?.items?.map((t) => (
                <Transaction key={t.transactionId} onClick={() => onClickTransaction(t)} Transaction={t} />
              ))}
            </Stack>
          </ThemedScrollbar>
        </Box>

        <Dialog open={openTransactionDetail} onClose={() => setOpenTransactionDetail(false)} fullWidth maxWidth="sm">
          <DialogTitle>Transaction Details</DialogTitle>
          <DialogContent>
            <TransactionDetails transaction={selectedTransaction!} onClose={() => setOpenTransactionDetail(false)} />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenTransactionDetail(false)}>Close</Button>
          </DialogActions>
        </Dialog>
      </Box>

      {/* Pagination controls placed in normal flow under the list */}
      <Box display="flex" justifyContent="center" sx={{ mt: 2 }}>
        <Paper sx={{ p: 1, width: '100%', maxWidth: 800 }} elevation={2}>
          <Box display="flex" justifyContent="space-between" alignItems="center">
            <Box display="flex" alignItems="center">
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
        </Paper>
      </Box>
    </>
  );
}
