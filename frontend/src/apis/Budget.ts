import { httpService } from '../services/httpService';
import { BudgetSchema, BudgetArraySchema, BudgetStatusSchema, BudgetsStatusArraySchema } from '../types/Budget';
import type { BudgetFormInput } from '../types/Budget';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';
async function getBudgets() {
  return httpService.get('budgets', BudgetArraySchema);
}

export function useBudgets() {
  return useQuery({ queryKey: ['budgets'], queryFn: getBudgets });
}

async function getBudgetById(id: string) {
  return httpService.get(`budgets/${id}`, BudgetSchema);
}

export function useBudget(id: string | null) {
  return useQuery({ queryKey: ['budgets', id], queryFn: () => getBudgetById(id || ''), enabled: !!id });
}


async function addBudget(payload: BudgetFormInput) {
  const body = {
    CategoryId: payload.categoryId ?? null,
  Amount: payload.amount,
    Name: payload.name ?? null,
    StartDate: payload.startDate ?? null,
    EndDate: payload.endDate ?? null,
  };

  return httpService.post('budgets', BudgetSchema, body);
}

export function useAddBudget() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: addBudget,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['budgets'] });
      queryClient.invalidateQueries({ queryKey: ['budgets', 'status'] });
    },
  });
}

async function updateBudget(id: string, payload: BudgetFormInput) {
  const body = {
    CategoryId: payload.categoryId ?? null,
    Amount: payload.amount,
    Name: payload.name ?? null,
    StartDate: payload.startDate ?? null,
    EndDate: payload.endDate ?? null,
  };
  return httpService.put(`budgets/${id}`, BudgetSchema, body);
}

export function useUpdateBudget(id: string | null) {
  const queryClient = useQueryClient();
  return useMutation({ mutationFn: (payload: BudgetFormInput) => updateBudget(id || '', payload), onSuccess: () => queryClient.invalidateQueries({ queryKey: ['budgets'] }) });
}

async function deleteBudget(id: string) {
  return httpService.delete(`budgets/${id}`, z.string());
}

export function useDeleteBudget() {
  const queryClient = useQueryClient();
  return useMutation({ mutationFn: deleteBudget, onSuccess: () => queryClient.invalidateQueries({ queryKey: ['budgets'] }) });
}

async function getBudgetStatus(id: string) {
  return httpService.get(`budgets/${id}/status`, BudgetStatusSchema);
}

export function useBudgetStatus(id: string | null) {
  return useQuery({ queryKey: ['budgets', id, 'status'], queryFn: () => getBudgetStatus(id || ''), enabled: !!id });
}

async function getAllBudgetsStatus() {
  return httpService.get('budgets/status', BudgetsStatusArraySchema);
}

export function useAllBudgetsStatus() {
  return useQuery({ queryKey: ['budgets', 'status'], queryFn: getAllBudgetsStatus });
}
