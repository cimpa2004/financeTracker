import { Box, Paper } from "@mui/material";
import type { Transaction as TransactionType } from "../types/Transaction";
import { Icon } from '../Icons/Icons.tsx';

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
        <Icon name={typeof Transaction?.category === 'object' ? Transaction?.category?.icon ?? undefined : undefined}
          colorOf={Transaction?.category && typeof Transaction?.category === 'object' ? Transaction?.category?.color ?? undefined : undefined}
          style={{ width: 40, height: 40 }}
        />
            <Box flexGrow={1} display="flex" flexDirection="row" justifyContent="space-between" alignItems="center" marginLeft={1}>
                <Box fontWeight="bold">{Transaction.name}</Box>
                <Box color="text.secondary">${Transaction.amount.toFixed(2)}</Box>
            </Box>
        </Paper>
    );
}