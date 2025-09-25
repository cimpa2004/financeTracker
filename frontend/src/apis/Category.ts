import { useQuery } from '@tanstack/react-query';
import { httpService } from '../services/httpService';
import { z } from 'zod';

const CategorySchema = z.object({
  categoryId: z.string(),
  name: z.string(),
  // other fields as needed
});

const CategoryArraySchema = z.array(CategorySchema);

async function getCategories() {
  return httpService.get('categories', CategoryArraySchema);
}

export function useCategories() {
  return useQuery({
    queryKey: ['categories'],
    queryFn: getCategories,
  });
}