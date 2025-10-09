import { Box, Paper, Typography } from "@mui/material";
import type { Subscription as SubscriptionType } from "../types/Subscription";
import { Icon } from '../Icons/Icons.tsx';

export interface SubscriptionProps {
    Subsription: SubscriptionType;
}

export default function Subscription({ Subsription }: SubscriptionProps) {
    const paymentDate = Subsription.paymentDate
        ? new Date(Subsription.paymentDate).toLocaleDateString()
        : "";

    return (
        <Paper
            elevation={2}
            sx={{
                display: "flex",
                flexDirection: "row",
                alignItems: "center",
                justifyContent: "space-between",
                p: 2,
                width: "100%",
                marginBottom: 1,
            }}
        >
        <Icon name={typeof Subsription?.category === 'object' ? Subsription?.category?.icon ?? undefined : undefined}
          colorOf={Subsription?.category && typeof Subsription?.category === 'object' ? Subsription?.category?.color ?? undefined : undefined}
          style={{ width: 40, height: 40, marginRight: 16 }} />
            <Box flexGrow={1} display="flex" flexDirection="row" justifyContent="space-between" alignItems="center" marginLeft={1}>
                <Box display="flex" flexDirection="column" flexGrow={1} sx={{ overflow: "hidden" }}>
                    <Typography fontWeight="bold" noWrap>
                        {Subsription.name}
                    </Typography>
                    <Typography variant="caption" color="text.secondary" noWrap>
                        {paymentDate}
                    </Typography>
                </Box>
                <Box color="text.secondary" sx={{ marginLeft: 2 }}>
                    ${Subsription?.amount ? Subsription.amount.toFixed(2) : "0.00"}
                </Box>
            </Box>
        </Paper>
    );
}