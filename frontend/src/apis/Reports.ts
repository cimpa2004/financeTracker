import { useMutation } from "@tanstack/react-query";
import { httpService } from "../services/httpService";

export function useDownloadBudgetReport(from: string, to: string) {
  return useMutation({
    mutationFn: async (): Promise<Blob> => {
      return await httpService.download(`reports/budgets?from=${from}&to=${to}`);
    },
    onSuccess: (blob: Blob) => {
      const url = globalThis.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `FinanceReport_${from}_${to}.pdf`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      globalThis.URL.revokeObjectURL(url);
    },
    onError: (err: unknown) => {
      console.error('Download failed', err);
      alert('Download failed: ' + String(err));
    }
  });
}