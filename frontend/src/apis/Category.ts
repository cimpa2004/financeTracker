import { useQuery } from '@tanstack/react-query';
import { httpService } from '../services/httpService';
import { z } from 'zod';
import { CategoryNestedSchema as CategorySchema } from '../types/Category';


async function getCategories() {
  return httpService.get('categories', z.array(CategorySchema));
}

export function useCategories() {
  return useQuery({
    queryKey: ['categories'],
    queryFn: getCategories,
  });
}