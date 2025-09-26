import { Button, Paper, Stack } from "@mui/material";
import React from "react";
import type { Transaction as TransactionType } from "../types/Transaction";
import Transaction from "./Transaction";
import type { Subscription as SubscriptionType } from "../types/Subscription";
import Subscription from "./Subsription";
import { useNavigate } from "react-router-dom";
import { ROUTES } from "../constants";

export interface TransactionsContainerProps {
    Transactions?: TransactionType[];
    Subscriptions?: SubscriptionType[];
}

export default function TranOrSubContainer({
    Transactions = [],
    Subscriptions = [],
}: TransactionsContainerProps) {
    const navigate = useNavigate();

    const firstItems = React.useMemo<(TransactionType | SubscriptionType)[]>(
        () => [...Transactions, ...Subscriptions].slice(0, 3),
        [Transactions, Subscriptions]
    );

    const isTransaction = (item: TransactionType | SubscriptionType): item is TransactionType =>
        item !== null && typeof item === "object" && "transactionId" in item;

    return (
        <Paper elevation={1} sx={{ padding: 1, marginBottom: 2, width: "100%" }}>
            {firstItems.map((item) =>
                isTransaction(item) ? (
                    <Transaction key={(item as TransactionType).transactionId} Transaction={item as TransactionType} />
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
                  <Button variant="outlined" color="primary">Show All</Button>
                  <Button variant="contained" color="primary" onClick={() => navigate(ROUTES.ADD_TRANSACTION)}>Add New Transaction</Button>
                </>
              ) : (<>
                  <Button variant="outlined" color="primary">Show All</Button>
                  <Button variant="contained" color="primary">Add New Subscription</Button>
              </>
              )}
            </Stack>
        </Paper>
    );
}