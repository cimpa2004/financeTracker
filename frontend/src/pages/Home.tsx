import { Box, CircularProgress } from "@mui/material";
import SpentThisMonth from "../components/SpentThisMonth";
import TranOrSubContainer from "../components/TransactionsContainer";
import { usePagedTransactions } from "../apis/Transaction";
import SpentByCategoryCard from "../components/SpentByCategoryCard";
// import { useLast3Subscriptions } from "../apis/Subsription";

//code is commented out couse subscribtions are not part of the MVP
export default function Home() {
    const { data: pagedData, isLoading, isError } = usePagedTransactions(1, 3);
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
            <TranOrSubContainer Transactions={pagedData?.items || []} />
            {/* {last3Subscriptions?.length === 0 ? (
                <Box>No subscriptions found</Box>
            ) : (
                <TranOrSubContainer Subscriptions={last3Subscriptions || []} />
            )} */}
        <SpentByCategoryCard />
        </Box>
    )
}