import { Button, Paper, Stack, Dialog, DialogTitle, DialogContent, DialogActions } from "@mui/material";
import { useCallback, useMemo, useState } from "react";
import TransactionForm from './TransactionForm';
import type { Transaction as TransactionType } from "../types/Transaction";
import Transaction from "./Transaction";
import type { Subscription as SubscriptionType } from "../types/Subscription";
import Subscription from "./Subsription";
import TransactionDetails from "./TransactionDetails";
import PagedTransactions from "./PagedTransactions";

export interface TransactionsContainerProps {
    Transactions?: TransactionType[];
    Subscriptions?: SubscriptionType[];
}

export default function TranOrSubContainer({
    Transactions = [],
    Subscriptions = [],
}: TransactionsContainerProps) {
  const [openCreate, setOpenCreate] = useState(false);
  const [openTransactionDetail, setOpenTransactionDetail] = useState(false);
  const [selectedTransaction, setSelectedTransaction] = useState<TransactionType | null>(null);
  const [AllTransactionsOpen, setAllTransactionsOpen] = useState(false);

  const firstItems = useMemo<(TransactionType | SubscriptionType)[]>(
    () => [...Transactions, ...Subscriptions].slice(0, 3),
    [Transactions, Subscriptions]
  );

    const isTransaction = (item: TransactionType | SubscriptionType): item is TransactionType =>
    item !== null && typeof item === "object" && "transactionId" in item;

  const onClickTransaction = useCallback((transaction: TransactionType) => {
    setSelectedTransaction(transaction);
    setOpenTransactionDetail(true);
  }, []);


    return (
        <Paper elevation={1} sx={{ padding: 1, marginBottom: 2, width: "100%" }}>
            {firstItems.map((item) =>
                isTransaction(item) ? (
                    <Transaction key={(item as TransactionType).transactionId} onClick={() => onClickTransaction(item as TransactionType)} Transaction={item as TransactionType} />
                ) : (
                    <Subscription
                        key={(item as SubscriptionType).subscriptionId}
                        Subsription={item as SubscriptionType}
                    />
                )
            )}
            <Stack flexDirection={'row'} justifyContent={"space-between"} alignItems={"center"} padding={1}>
              {Transactions.length > 0 ? (
                <>
                  <Button variant="outlined" color="primary" onClick={()=>setAllTransactionsOpen(true)}>Show All</Button>
                  <Button variant="contained" color="primary" onClick={() => setOpenCreate(true)}>Add New Transaction</Button>
                </>
                            ) : (
                                <Button variant="contained" color="primary" onClick={() => setOpenCreate(true)}>Add New Transaction</Button>
                            )}
            </Stack>
            <Dialog open={openCreate} onClose={() => setOpenCreate(false)} fullWidth maxWidth="sm">
                <DialogTitle>Add Transaction</DialogTitle>
                <DialogContent>
                    <TransactionForm />
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setOpenCreate(false)}>Close</Button>
                </DialogActions>
            </Dialog>
            <Dialog open={openTransactionDetail} onClose={() => setOpenTransactionDetail(false)} fullWidth maxWidth="sm">
                <DialogTitle>Transaction Details</DialogTitle>
                <DialogContent>
                  <TransactionDetails transaction={selectedTransaction!} onClose={()=>setOpenTransactionDetail(false)} />
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setOpenTransactionDetail(false)}>Close</Button>
                </DialogActions>
        </Dialog>
        <Dialog open={AllTransactionsOpen} onClose={() => setAllTransactionsOpen(false)} fullWidth maxWidth="sm">
                <DialogTitle>All Transactions</DialogTitle>
          <DialogContent>
              <PagedTransactions maxContentHeight={'55vh'}/>
          </DialogContent>
                <DialogActions>
                    <Button onClick={() => setAllTransactionsOpen(false)}>Close</Button>
                </DialogActions>
            </Dialog>
        </Paper>
    );
}