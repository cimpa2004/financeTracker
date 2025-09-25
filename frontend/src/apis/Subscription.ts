import { useQuery } from '@tanstack/react-query';
import { httpService } from '../services/httpService';
import { z } from 'zod';

const SubscriptionSchema = z.object({
  subscriptionId: z.string(),
  name: z.string(),
  // other fields as needed
});

const SubscriptionArraySchema = z.array(SubscriptionSchema);

async function getSubscriptions() {
  return httpService.get('subscriptions', SubscriptionArraySchema);
}

export function useSubscriptions() {
  return useQuery({
    queryKey: ['subscriptions'],
    queryFn: getSubscriptions,
  });
}