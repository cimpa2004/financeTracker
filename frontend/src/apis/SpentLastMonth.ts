import z from "zod";
import { httpService } from "../services/httpService";
import { useQuery } from "@tanstack/react-query";

async function getSpentLastMonth() {
  return httpService.get('spent-last-month', z.object({ spent: z.number() }));
}

export function useGetSpentLastMonth() {
  return useQuery({
    queryKey: ['spent-last-month', 'transactions'],
    queryFn: getSpentLastMonth,
  });
}