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
import TransactionForm from './TransactionForm';
import { useSmallScreen } from "../hooks/useSmallScreen.ts";

interface TransactionDetailsProps {
  transaction: TransactionType;
  onClose?: () => void;
}

function getResolvedUserId(transaction: TransactionType): string | undefined {
  const u = transaction.user as unknown;
  if (!u) return undefined;
  if (typeof u === 'string') return u;
  if (typeof u === 'object' && u !== null) {
    const obj = u as Record<string, unknown>;
    if ('userId' in obj && typeof obj['userId'] === 'string') return obj['userId'] as string;
  }
  return undefined;
}

function getResolvedCategoryId(transaction: TransactionType): string | undefined {
  const c = transaction.category as unknown;
  if (!c) return undefined;
  if (typeof c === 'string') return c;
  if (typeof c === 'object' && c !== null) {
    const obj = c as Record<string, unknown>;
    if ('categoryId' in obj && typeof obj['categoryId'] === 'string') return obj['categoryId'] as string;
  }
  return undefined;
}

function TransactionDetailsView(props: {
  transaction: TransactionType;
  isSmallScreen: boolean;
  confirmOpen: boolean;
  setConfirmOpen: (v: boolean) => void;
  editOpen: boolean;
  setEditOpen: (v: boolean) => void;
  handleDelete: () => void;
  isPending: boolean;
  resolvedUserId?: string;
  resolvedCategoryId?: string;
  onClose?: () => void;
}) {
  const { transaction, isSmallScreen, confirmOpen, setConfirmOpen, editOpen, setEditOpen, handleDelete, isPending, resolvedUserId, resolvedCategoryId, onClose } = props;
  return (
    <Paper elevation={2} sx={{ p: 2, borderRadius: 2 }}>
      <Stack direction={isSmallScreen ? "column" : "row"} spacing={2} alignItems={isSmallScreen ? "stretch" : "center"}>
        <Box sx={{ display: 'flex', justifyContent: isSmallScreen ? 'center' : 'flex-start' }}>
          <Icon name={typeof transaction?.category === 'object' ? transaction?.category?.icon ?? undefined : undefined}
            colorOf={transaction?.category && typeof transaction?.category === 'object' ? transaction?.category?.color ?? undefined : undefined}
            size="large"
          />
        </Box>

        <Box sx={{ flex: 1 }}>
          <Stack direction={isSmallScreen ? "column" : "row"} justifyContent="space-between" alignItems={isSmallScreen ? "flex-start" : "center"}>
            <Box>
              <Typography variant="subtitle1" fontWeight={700}>{transaction.name}</Typography>
              <Typography variant="body2" color="text.secondary">{transaction.description}</Typography>
            </Box>

            <Box textAlign={isSmallScreen ? 'left' : 'right'} sx={{ mt: isSmallScreen ? 1 : 0 }}>
              <Typography variant="subtitle1" fontWeight={700}>
                {new Intl.NumberFormat('hu-HU', { style: 'currency', currency: 'HUF' }).format(transaction.amount)}
              </Typography>
              <Typography variant="caption" color="text.secondary">{transaction.date ? transaction.date.slice(0,10).replaceAll('-', '.') : ''}</Typography>
            </Box>
          </Stack>

          <Divider sx={{ my: 1 }} />

          <Stack direction={isSmallScreen ? 'column' : 'row'} spacing={1} alignItems={isSmallScreen ? 'stretch' : 'center'}>
            {transaction.category && typeof transaction.category === 'object' ? (
              <Chip label={`${transaction.category.type} - ${transaction.category.name}`} size="small" sx={{ bgcolor: transaction.category.color ?? undefined, color: 'black' }} />
            ) : null}

            {/* spacer only for wide layouts to push actions to the right */}
            {!isSmallScreen && <Box sx={{ flex: 1 }} />}

            <Stack direction="row" spacing={1} sx={{ justifyContent: isSmallScreen ? 'flex-start' : 'flex-end' }}>
              <IconButton aria-label="edit" size="small" onClick={() => setEditOpen(true)}>
                <EditIcon fontSize="small" />
              </IconButton>
              <IconButton aria-label="delete" size="small" color="error" onClick={() => setConfirmOpen(true)}>
                <DeleteIcon fontSize="small" />
              </IconButton>
            </Stack>
          </Stack>
        </Box>
      </Stack>

  <DeleteConfirmDialog open={confirmOpen} onClose={() => setConfirmOpen(false)} onDelete={handleDelete} isPending={isPending} name={transaction.name ?? undefined} />

      <EditTransactionDialog open={editOpen} onClose={() => setEditOpen(false)} transaction={transaction} resolvedUserId={resolvedUserId} resolvedCategoryId={resolvedCategoryId} onSuccess={() => { setEditOpen(false); onClose?.(); }} />
    </Paper>
  );
}

function DeleteConfirmDialog({ open, onClose, onDelete, isPending, name }: { open: boolean; onClose: () => void; onDelete: () => void; isPending: boolean; name?: string; }) {
  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>Delete transaction</DialogTitle>
      <DialogContent>
        <DialogContentText>
          Are you sure you want to delete "{name}"? This action cannot be undone.
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} startIcon={<CloseIcon />}>Cancel</Button>
        <Button color="error" variant="contained" onClick={onDelete} disabled={isPending} startIcon={<DeleteIcon />}>Delete</Button>
      </DialogActions>
    </Dialog>
  );
}

function EditTransactionDialog({ open, onClose, transaction, resolvedUserId, resolvedCategoryId, onSuccess }: { open: boolean; onClose: () => void; transaction: TransactionType; resolvedUserId?: string; resolvedCategoryId?: string; onSuccess: () => void; }) {
  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>Edit Transaction</DialogTitle>
      <DialogContent>
        <TransactionForm
          submitLabel="Update Transaction"
          defaultValues={{
            transactionId: transaction.transactionId,
            userId: resolvedUserId,
            amount: String(transaction.amount),
            name: transaction.name ?? '',
            date: transaction.date ?? undefined,
            categoryId: resolvedCategoryId,
            description: transaction.description ?? undefined,
          }}
          onSuccess={onSuccess}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
}

export default function TransactionDetails({ transaction, onClose }: TransactionDetailsProps) {
  const {mutate, isSuccess, isPending} = useDeleteTransaction(transaction.transactionId);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const isSmallScreen = useSmallScreen();

  const resolvedUserId = getResolvedUserId(transaction);
  const resolvedCategoryId = getResolvedCategoryId(transaction);

  const handleDelete = () => {
    mutate();
  };

  if(isSuccess && onClose) {
    onClose();
  }

  return (
    <TransactionDetailsView
      transaction={transaction}
      isSmallScreen={isSmallScreen}
      confirmOpen={confirmOpen}
      setConfirmOpen={setConfirmOpen}
      editOpen={editOpen}
      setEditOpen={setEditOpen}
      handleDelete={handleDelete}
      isPending={isPending}
      resolvedUserId={resolvedUserId}
      resolvedCategoryId={resolvedCategoryId}
      onClose={onClose}
    />
  );
}