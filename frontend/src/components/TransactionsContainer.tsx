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
         <Paper elevation={1} sx={{ padding: 1, marginBottom: 2, width: "100%" }} >
            {first3Transactions.map((transaction) => (
                <Transaction key={transaction.transactionId} Transaction={transaction} />
            ))}
        </Paper>
    );
}