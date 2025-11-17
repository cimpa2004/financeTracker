import { Box, Typography, CircularProgress, Fab, Dialog, DialogTitle, DialogContent, Snackbar, Button } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { useTheme } from '@mui/material/styles';
import { useAllBudgetsStatus, useDeleteBudget } from '../apis/Budget';
import BudgetDetails from '../components/BudgetDetails';
import type { BudgetStatus } from '../types/Budget';
import getIntervalFromDates from '../utils/dateInterval';
import AddBudgetForm from '../components/AddBudgetForm';
import { useState } from 'react';
import BudgetCard from '../components/BudgetCard';

export default function BudgetChartsPage() {
  const [open, setOpen] = useState(false);
  const handleOpen = () => { setEditingBudget(null); setOpen(true); };
  const handleClose = () => setOpen(false);
  const [editingBudget, setEditingBudget] = useState<BudgetStatus | null>(null);
  const openEdit = (budget: BudgetStatus) => { setEditingBudget(budget); setOpen(true); };
  const [snackOpen, setSnackOpen] = useState(false);
  const [snackMessage, setSnackMessage] = useState<string>('');
  const showSnack = (msg: string) => { setSnackMessage(msg); setSnackOpen(true); };
  const hideSnack = () => setSnackOpen(false);
  const theme = useTheme();
  const { data, isLoading, isError } = useAllBudgetsStatus();
  const deleteMutation = useDeleteBudget();
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [toDeleteId, setToDeleteId] = useState<string | null>(null);
  const openConfirm = (id: string) => { setToDeleteId(id); setConfirmOpen(true); };
  const closeConfirm = () => { setToDeleteId(null); setConfirmOpen(false); };
  const [detailsOpen, setDetailsOpen] = useState(false);
  const [detailsBudgetId, setDetailsBudgetId] = useState<string | null>(null);
  const openDetails = (id: string) => { setDetailsBudgetId(id); setDetailsOpen(true); };

  if (isLoading) return <CircularProgress />;
  if (isError) return <Box>Error loading budgets</Box>;

  const budgets = data ?? [];

  return (
    <Box p={2} justifyContent="center" alignItems="center" width="100vw">
      <Typography variant="h4" gutterBottom>
        Budgets Overview
      </Typography>
      <Box
        display="grid"
        gridTemplateColumns={{ xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr 1fr' }}
        gap={2}
      >
        {budgets.map((b) => (
          <BudgetCard
            key={b.budgetId}
            budget={b}
            onEdit={openEdit}
            onDelete={openConfirm}
            onDetails={openDetails}
          />
        ))}
      </Box>
      <Fab color="primary" aria-label="add" onClick={handleOpen} sx={{ position: 'fixed', right: 24, bottom: 24 }}>
        <AddIcon />
      </Fab>

      <Dialog open={open} onClose={() => { setEditingBudget(null); handleClose(); }} fullWidth maxWidth="sm">
        <DialogTitle>{editingBudget ? 'Edit Budget' : 'Add Budget'}</DialogTitle>
        <DialogContent>
          <AddBudgetForm
            budgetId={editingBudget?.budgetId ?? null}
            initialValues={editingBudget ? {
              categoryId: editingBudget.category?.categoryId ?? null,
              amount: editingBudget.amount,
              name: editingBudget.name ?? '',
              interval: getIntervalFromDates(editingBudget.startDate ?? null, editingBudget.endDate ?? null),
              startDate: editingBudget.startDate ?? '',
              endDate: editingBudget.endDate ?? '',
            } : undefined}
            onSuccess={() => { setEditingBudget(null); handleClose(); showSnack(editingBudget ? 'Budget updated' : 'Budget added'); }}
          />
        </DialogContent>
      </Dialog>
  <BudgetDetails open={detailsOpen} onClose={() => { setDetailsOpen(false); setDetailsBudgetId(null); }} budgetId={detailsBudgetId} />
      <Dialog open={confirmOpen} onClose={closeConfirm}>
        <DialogTitle>Delete budget?</DialogTitle>
        <DialogContent>
          <Typography>Are you sure you want to delete this budget? This action cannot be undone.</Typography>
          <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end', mt: 2 }}>
            <Button onClick={closeConfirm}>Cancel</Button>
            <Button color="error" onClick={async () => {
              if (!toDeleteId) return;
              try {
                await deleteMutation.mutateAsync(toDeleteId);
                closeConfirm();
                showSnack('Budget deleted');
              } catch (err) {
                console.error('delete failed', err);
              }
            }}>Delete</Button>
          </Box>
        </DialogContent>
      </Dialog>
      <Snackbar
        open={snackOpen}
        onClose={hideSnack}
        autoHideDuration={5000}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}
      >
        <Box sx={{ p: 0.5 }}>
          <Box sx={{ bgcolor: theme.palette.success.main, color: theme.palette.getContrastText(theme.palette.success.main), px: 2, py: 1, borderRadius: 1, boxShadow: 3 }}>
            {snackMessage}
          </Box>
        </Box>
      </Snackbar>
    </Box>
  );
}
