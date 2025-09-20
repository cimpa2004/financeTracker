import { Paper } from "@mui/material";
import type { TransactionProps } from "./Transaction";
import Transaction from "./Transaction";
import React from "react";

export interface TransactionsContainerProps{
    Transactions : TransactionProps []
}

export default function TransactionContainer({ Transactions }: TransactionsContainerProps) {
    const [first3Transactions, setFirst3Transactions] = React.useState<TransactionProps[]>([]);
    React.useEffect(() => {
        setFirst3Transactions(Transactions.slice(0, 3));
    }, [Transactions]);
    return (
         <Paper elevation={3} sx={{ padding: 2, marginBottom: 2 }}>
            {first3Transactions.map((transaction) => (
                <Transaction key={transaction.id} {...transaction} />
            ))}
        </Paper>
    );
}