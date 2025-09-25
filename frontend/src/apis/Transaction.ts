import { TransactionArraySchema, TransactionSchema } from "../types/Transaction";
import {httpService} from "../services/httpService";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

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

async function addTransaction(payload: unknown) {
    // validate/serialize with TransactionSchema if available in your types
    const response = await httpService.post('transactions', payload, TransactionSchema);
    return response;
}

export function useAddTransaction() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: addTransaction,
        onSuccess: () => {
            // refresh lists after a successful add
            queryClient.invalidateQueries({ queryKey: ["transactions"] });
            queryClient.invalidateQueries({ queryKey: ["transactions", "last3"] });
        },
    });
}
