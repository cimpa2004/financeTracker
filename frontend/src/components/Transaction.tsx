import { Box } from "@mui/material";

export interface TransactionProps{
    id: string;
    name: string;
    amount: number;
    iconUrl: string;
}

export default function Transaction({ id, name, amount, iconUrl }: TransactionProps) {
    return(
        <Box display={"flex"} flexDirection="row" alignItems="center" p={2}>
            <img src={iconUrl} alt={name} style={{ width: 40, height: 40, marginRight: 16 }} />
            <Box flexGrow={1}>
                <Box fontWeight="bold">{name}</Box>
                <Box color="text.secondary">${amount.toFixed(2)}</Box>
            </Box>
        </Box>
    )
}