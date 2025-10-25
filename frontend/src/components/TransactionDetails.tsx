import {
  Box,
  Paper,
  Stack,
  Typography,
  IconButton,
  Divider,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Button,
} from "@mui/material";
import type { Transaction as TransactionType } from "../types/Transaction";
import { useDeleteTransaction } from "../apis/Transaction";
import { Icon } from "../Icons/Icons.tsx";
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import CloseIcon from '@mui/icons-material/Close';
import { useState } from "react";

interface TransactionDetailsProps {
  transaction: TransactionType;
  onEdit?: (t: TransactionType) => void;
  onClose?: () => void;
}

export default function TransactionDetails({ transaction, onEdit, onClose }: TransactionDetailsProps) {
  const {mutate, isSuccess, isPending} = useDeleteTransaction(transaction.transactionId);
  const [confirmOpen, setConfirmOpen] = useState(false);

  const handleDelete = () => {
    mutate();
  };

  if(isSuccess && onClose) {
    onClose();
  }

  return (
    <Paper elevation={2} sx={{ p: 2, borderRadius: 2 }}>
      <Stack direction="row" spacing={2} alignItems="center">
        <Icon name={typeof transaction?.category === 'object' ? transaction?.category?.icon ?? undefined : undefined}
          colorOf={transaction?.category && typeof transaction?.category === 'object' ? transaction?.category?.color ?? undefined : undefined}
          size="large"
        />

        <Box sx={{ flex: 1 }}>
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Box>
              <Typography variant="subtitle1" fontWeight={700}>{transaction.name}</Typography>
              <Typography variant="body2" color="text.secondary">{transaction.description}</Typography>
            </Box>

            <Box textAlign="right">
              <Typography variant="subtitle1" fontWeight={700}>
                {new Intl.NumberFormat('hu-HU', { style: 'currency', currency: 'HUF' }).format(transaction.amount)}
              </Typography>
              <Typography variant="caption" color="text.secondary">{transaction.date ? transaction.date.slice(0,10).replaceAll('-', '.') : ''}</Typography>
            </Box>
          </Stack>

          <Divider sx={{ my: 1 }} />

          <Stack direction="row" spacing={1} alignItems="center">
            {transaction.category && typeof transaction.category === 'object' ? (
              <Chip label={`${transaction.category.type} - ${transaction.category.name}`} size="small" sx={{ bgcolor: transaction.category.color ?? undefined, color: transaction.category.color ? undefined : 'inherit' }} />
            ) : null}
            <Box sx={{ flex: 1 }} />
            <Stack direction="row" spacing={1}>
              <IconButton aria-label="edit" size="small" onClick={() => onEdit?.(transaction)}>
                <EditIcon fontSize="small" />
              </IconButton>
              <IconButton aria-label="delete" size="small" color="error" onClick={() => setConfirmOpen(true)}>
                <DeleteIcon fontSize="small" />
              </IconButton>
            </Stack>
          </Stack>
        </Box>
      </Stack>

      <Dialog open={confirmOpen} onClose={() => setConfirmOpen(false)}>
        <DialogTitle>Delete transaction</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete "{transaction.name}"? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmOpen(false)} startIcon={<CloseIcon />}>Cancel</Button>
          <Button color="error" variant="contained" onClick={handleDelete} disabled={isPending} startIcon={<DeleteIcon />}>Delete</Button>
        </DialogActions>
      </Dialog>
    </Paper>
  );
}