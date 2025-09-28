import { httpService } from '../services/httpService';
import { CategorySchema, type AddCategoryInput } from '../types/Category';
import { z } from 'zod';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';


async function getCategories() {
  return httpService.get('categories', z.array(CategorySchema));
}

export function useCategories() {
  return useQuery({
    queryKey: ['categories'],
    queryFn: getCategories,
  });
}

async function addCategory(payload: AddCategoryInput) {
  const body = {
    Name: payload.name,
    Icon: payload.icon ?? null,
    Color: payload.color ?? null,
    Type: payload.type,
    IsPublic: !!payload.isPublic,
  };

  return httpService.post('categories', CategorySchema, body);
}

export function useAddCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: addCategory,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['categories'] }),
  });
}