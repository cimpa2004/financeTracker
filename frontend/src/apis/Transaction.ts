import { TransactionArraySchema } from "../types/Transaction";
import {httpService} from "../services/httpService";
import { useQuery } from "@tanstack/react-query";

async function getAllTransactions() {
    const response = await httpService.get('transactions', TransactionArraySchema);
    return response;
}

export function useTransactionApi() {
    return useQuery({
        queryKey: ["transactions"],
        queryFn: getAllTransactions,
    })
}

async function getLast3Transactions() {
    const response = await httpService.get('transactions/last3', TransactionArraySchema);
    return response;
}

export function useLast3Transactions() {
    return useQuery({
        queryKey: ["transactions", "last3"],
        queryFn: getLast3Transactions,
    })
}

