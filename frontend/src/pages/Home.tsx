import { Box, CircularProgress } from "@mui/material";
import SpentThisMonth from "../components/SpentThisMonth";
import TransactionContainer from "../components/TransactionsContainer";
import { useLast3Transactions } from "../apis/Transaction";

export default function Home() {
    const { data: last3Transactions, isLoading, isError } = useLast3Transactions();
    if (isLoading) {
        return <CircularProgress />;
    }
    if (isError) {
        return <Box>Error loading transactions</Box>;
    }
    return (
        <Box 
            textAlign="center" 
            display="flex" 
            flexDirection="column"
            justifyContent="flex-start"
            alignItems="center"
            width="100%"
            height={"100%"}
        >
            <SpentThisMonth />
            {last3Transactions?.length === 0 ? (
                <Box>No transactions found</Box>
            ) : (
                <TransactionContainer Transactions={last3Transactions || []} />
            )}
        </Box>
    )
}