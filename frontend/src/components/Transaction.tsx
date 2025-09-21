import { Box } from "@mui/material";
import type { Transaction as TransactionType } from "../types/Transaction";

export interface TransactionProps{
    Transaction: TransactionType;
}

export default function Transaction({ Transaction }: TransactionProps) {
    return(
        <Box display={"flex"} flexDirection="row" alignItems="center" p={2}>
            <img src={Transaction.categoryId || ""} style={{ width: 40, height: 40, marginRight: 16 }} />
            <Box flexGrow={1}>
                <Box fontWeight="bold">{Transaction.name}</Box>
                <Box color="text.secondary">${Transaction.amount.toFixed(2)}</Box>
            </Box>
        </Box>
    )
}