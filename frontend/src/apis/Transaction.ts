import { TransactionArraySchema, TransactionSchema, type TransactionFormInput } from "../types/Transaction";
import {httpService} from "../services/httpService";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import z from "zod";

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

async function addTransaction(payload: TransactionFormInput ) {
  // validate/serialize with TransactionSchema if available in your types
  const response = await httpService.post('transactions', TransactionSchema, payload);
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

async function deleteTransaction(transactionId: string) {
  await httpService.delete(`transactions/${transactionId}`, z.string());
}

export function useDeleteTransaction(transactionId: string) {
  const queryClient = useQueryClient();
  return useMutation({
      mutationFn: () => deleteTransaction(transactionId),
      onSuccess: () => {
          queryClient.invalidateQueries({ queryKey: ["transactions"] });
          queryClient.invalidateQueries({ queryKey: ["transactions", "last3"] });
      },
  });
}
