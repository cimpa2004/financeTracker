import { Box, Typography } from '@mui/material';
import TransactionForm from '../components/TransactionForm';
import { useAuth } from '../contexts/AuthContext';

export default function AddTransaction() {
  const today = new Date().toISOString().slice(0, 10);
  const { user } = useAuth();

  const defaultValues = { userId: user?.userId || '', date: today };

  return (
    <Box sx={{ width: '100%', maxWidth: '600px', mx: 'auto', p: 2 }}>
      <Typography variant="h6" gutterBottom>
        Add Transaction
      </Typography>

      <TransactionForm submitLabel="Add Transaction" defaultValues={defaultValues} />
    </Box>
  );
}