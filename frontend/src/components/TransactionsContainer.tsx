import { Paper } from "@mui/material";
import React from "react";
import type { Transaction as TransactionType} from "../types/Transaction";
import Transaction from "./Transaction";

export interface TransactionsContainerProps{
    Transactions : TransactionType []
}

export default function TransactionContainer({ Transactions }: TransactionsContainerProps) {
    const [first3Transactions, setFirst3Transactions] = React.useState<TransactionType[]>([]);
    React.useEffect(() => {
        setFirst3Transactions(Transactions.slice(0, 3));
    }, [Transactions]);
    return (
         <Paper elevation={3} sx={{ padding: 2, marginBottom: 2 }}>
            {first3Transactions.map((transaction) => (
                <Transaction key={transaction.transactionId} Transaction={transaction} />
            ))}
        </Paper>
    );
}