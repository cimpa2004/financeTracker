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

const PagedTransactionsSchema = z.object({
    page: z.number(),
    size: z.number(),
    total: z.number(),
    totalPages: z.number(),
    items: TransactionArraySchema,
});

async function getPagedTransactions(page = 1, size = 20) {
    const response = await httpService.get(`transactions/paged?page=${page}&size=${size}`, PagedTransactionsSchema);
    return response;
}

export function usePagedTransactions(page: number, size: number) {
    return useQuery({
        queryKey: ["transactions", "paged", page, size],
        queryFn: () => getPagedTransactions(page, size),
    });
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
            // refresh lists after a successful add - invalidate all transactions queries
            queryClient.invalidateQueries({ predicate: (q) => Array.isArray(q.queryKey) && q.queryKey[0] === 'transactions' });
        },
    });
}

async function updateTransaction(id: string, payload: TransactionFormInput) {
    const response = await httpService.put(`transactions/${id}`, TransactionSchema, payload);
    return response;
}

export function useUpdateTransaction() {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: ({ id, payload }: { id: string; payload: TransactionFormInput }) => updateTransaction(id, payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ predicate: (q) => Array.isArray(q.queryKey) && q.queryKey[0] === 'transactions' });
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
          queryClient.invalidateQueries({ predicate: (q) => Array.isArray(q.queryKey) && q.queryKey[0] === 'transactions' });
      },
  });
}
