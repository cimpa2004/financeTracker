import { useQuery } from '@tanstack/react-query';
import { httpService } from '../services/httpService';
import { spentByCategoryResponseSchema, spentByIntervalResponseSchema, type Interval } from '../types/Statistics';

async function getSpentByCategory(interval: Interval) {
  return httpService.get(`spent-by-category/${interval}`, spentByCategoryResponseSchema);
}

export function useGetSpentByCategory(interval: Interval) {
  return useQuery({
    queryKey: ['spentByCategory', interval],
    queryFn: () => getSpentByCategory(interval),
  });
}

async function getSpentByInterval(interval: Interval, start?: string, end?: string) {
  const qs = new URLSearchParams();
  if (start) qs.set('start', start);
  if (end) qs.set('end', end);
  const qsStr = qs.toString();
  const path = qsStr ? `spent-by-interval/${interval}?${qsStr}` : `spent-by-interval/${interval}`;
  return httpService.get(path, spentByIntervalResponseSchema);
}

export function useGetSpentByInterval(interval: Interval, start?: string, end?: string) {
  return useQuery({
    queryKey: ['spentByInterval', interval, start ?? null, end ?? null],
    queryFn: () => getSpentByInterval(interval, start, end),
  });
}