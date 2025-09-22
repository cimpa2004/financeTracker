import { Box, Paper, Typography } from "@mui/material";
import type { Subscription as SubscriptionType } from "../types/Subscription";

export interface SubscriptionProps {
    Subsription: SubscriptionType;
}

export default function Subscription({ Subsription }: SubscriptionProps) {
    const icon =
        typeof Subsription.category === "object" && Subsription.category?.icon
            ? Subsription.category.icon
            : "";

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
                width: "90%",
                marginBottom: 1,
            }}
        >
            <img
                src={icon}
                alt=""
                style={{ width: 40, height: 40, marginRight: 16 }}
            />
            <Box flexGrow={1} display="flex" flexDirection="row" justifyContent="space-between" alignItems="center">
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