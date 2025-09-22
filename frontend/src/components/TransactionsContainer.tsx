import { Paper } from "@mui/material";
import React from "react";
import type { Transaction as TransactionType } from "../types/Transaction";
import Transaction from "./Transaction";
import type { Subscription as SubscriptionType } from "../types/Subscription";
import Subscription from "./Subsription";

export interface TransactionsContainerProps {
    Transactions?: TransactionType[];
    Subscriptions?: SubscriptionType[];
}

export default function TranOrSubContainer({
    Transactions = [],
    Subscriptions = [],
}: TransactionsContainerProps) {
    const [firstItems, setFirstItems] = React.useState<(TransactionType | SubscriptionType)[]>([]);

    React.useEffect(() => {
        const combined = [...Transactions, ...Subscriptions];
        setFirstItems(combined.slice(0, 3));
    }, [Transactions, Subscriptions]);

    const isTransaction = (item: TransactionType | SubscriptionType): item is TransactionType =>
        (item as TransactionType).transactionId !== undefined;

    return (
        <Paper elevation={1} sx={{ padding: 1, marginBottom: 2, width: "100%" }}>
            {firstItems.map((item) =>
                isTransaction(item) ? (
                    <Transaction key={item.transactionId} Transaction={item} />
                ) : (
                    <Subscription
                        key={(item as SubscriptionType).subscriptionId}
                        Subsription={item as SubscriptionType}
                    />
                )
            )}
        </Paper>
    );
}