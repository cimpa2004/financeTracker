import { Box, CircularProgress } from "@mui/material";
import SpentThisMonth from "../components/SpentThisMonth";
import TranOrSubContainer from "../components/TransactionsContainer";
import { useLast3Transactions } from "../apis/Transaction";
import SpentByCategoryCard from "../components/SpentByCategoryCard";
// import { useLast3Subscriptions } from "../apis/Subsription";

//code is commented out couse subscribtions are not part of the MVP
export default function Home() {
    const { data: last3Transactions, isLoading, isError } = useLast3Transactions();
    // const { data: last3Subscriptions, isLoading: isLoadingSubscriptions, isError: isErrorSubscriptions } = useLast3Subscriptions();
    if (isLoading /*|| isLoadingSubscriptions*/) {
        return <CircularProgress />;
    }
    if (isError /*|| isErrorSubscriptions*/) {
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
            maxWidth={600}
            mx="auto"
        >
            <SpentThisMonth />
            {last3Transactions?.length === 0 ? (
                <Box>No transactions found</Box>
            ) : (
                <TranOrSubContainer Transactions={last3Transactions || []} />
            )}
            {/* {last3Subscriptions?.length === 0 ? (
                <Box>No subscriptions found</Box>
            ) : (
                <TranOrSubContainer Subscriptions={last3Subscriptions || []} />
            )} */}
        <SpentByCategoryCard width={'100%'}/>
        </Box>
    )
}