// React import not required in new JSX transforms
import { Dialog, DialogTitle, DialogContent, DialogActions, Button, Box, Typography, Divider, CircularProgress, Alert, Stack } from '@mui/material';
import { useBudgetStatus, useBudgetTransactions } from '../apis/Budget';
import Transaction from './Transaction';
import type { Transaction as TransactionType } from '../types/Transaction';

type BudgetDetailsProps = {
  open: boolean;
  onClose: () => void;
  budgetId: string | null;
};

export default function BudgetDetails({ open, onClose, budgetId }: BudgetDetailsProps) {
  // fetch budget status (spent/remaining/start/end/name/amount)
  const { data: status, isLoading: statusLoading, error: statusError } = useBudgetStatus(budgetId);
  const { data: transactions, isLoading: txLoading, error: txError } = useBudgetTransactions(budgetId);

  const loading = statusLoading || txLoading;
  const error = statusError || txError;

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="md">
      <DialogTitle>Budget details</DialogTitle>
      <DialogContent dividers>
        {loading && (
          <Box display="flex" justifyContent="center" p={2}>
            <CircularProgress />
          </Box>
        )}

        {error && (
          <Alert severity="error">{(error as Error)?.message ? (error as Error).message : (String(error) || 'Failed to load budget details')}</Alert>
        )}

        {!loading && !error && status && (
          <Box>
            <Stack direction="row" justifyContent="space-between" alignItems="center" mb={2}>
              <Box>
                <Typography variant="h6">{status.name}</Typography>
                <Typography variant="body2" color="text.secondary">
                  Amount: ${Number(status.amount).toFixed(2)}
                </Typography>
              </Box>
              <Box textAlign="right">
                <Typography variant="body1" fontWeight="bold">Spent: ${Number(status.spent ?? 0).toFixed(2)}</Typography>
                <Typography variant="body2" color="text.secondary">Remaining: ${Number(status.remaining ?? 0).toFixed(2)}</Typography>
              </Box>
            </Stack>

            <Divider sx={{ my: 2 }} />

            <Typography variant="subtitle1" gutterBottom>Transactions</Typography>
            {transactions && transactions.length === 0 && (
              <Typography color="text.secondary">No transactions found for this budget.</Typography>
            )}

            <Stack spacing={1} mt={1}>
              {(transactions as TransactionType[] | undefined)?.map((t) => (
                <Transaction key={t.transactionId} Transaction={t} />
              ))}
            </Stack>
          </Box>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
}
