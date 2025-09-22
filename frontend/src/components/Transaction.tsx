import { Box, Paper } from "@mui/material";
import type { Transaction as TransactionType } from "../types/Transaction";

export interface TransactionProps{
    Transaction: TransactionType;
}

export default function Transaction({ Transaction }: TransactionProps) {
    return (
        <Paper
            elevation={2}
            sx={{
                display: "flex",
                flexDirection: "row",
                alignItems: "center",
                justifyContent: "space-between",
                p: 2,
                width: "90%",
                marginBottom: 1,
            }}
        >
            <img
                src={typeof Transaction.category === "object" && Transaction.category?.icon ? Transaction.category.icon : ""}
                alt=""
                style={{ width: 40, height: 40, marginRight: 16 }}
            />
            <Box flexGrow={1} display="flex" flexDirection="row" justifyContent="space-between" alignItems="center">
                <Box fontWeight="bold">{Transaction.name}</Box>
                <Box color="text.secondary">${Transaction.amount.toFixed(2)}</Box>
            </Box>
        </Paper>
    );
}