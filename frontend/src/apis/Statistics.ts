import { useQuery } from '@tanstack/react-query';
import { httpService } from '../services/httpService';
import { spentByCategoryResponseSchema, type Interval } from '../types/Statistics';

async function getSpentByCategory(interval: Interval) {
  return httpService.get(`spent-by-category/${interval}`, spentByCategoryResponseSchema);
}

export function useGetSpentByCategory(interval: Interval) {
  return useQuery({
    queryKey: ['spentByCategory', interval],
    queryFn: () => getSpentByCategory(interval),
  });
}