import { z } from 'zod';
import { UserNestedSchema } from './User';
import { CategoryNestedSchema } from './Category';

const DateString = z.string().refine((s) => !isNaN(Date.parse(s)), { message: 'Invalid date format' });

export const SubscriptionSchema = z.object({
  subscriptionId: z.string(),
  user: UserNestedSchema.nullable().optional(),
  category: CategoryNestedSchema.nullable().optional(),
  amount: z.number().nullable().optional(),
  name: z.string().max(255).nullable().optional(),
  interval: z.string().max(50).nullable().optional(),
  paymentDate: DateString.nullable().optional(),
  isActive: z.boolean(),
  createdAt: DateString,
});

export const SubscriptionNestedSchema = z.object({
    subscriptionId: z.string().optional(),
    name: z.string().max(255).optional(),
    amount: z.number().optional(),
    interval: z.string().max(50).optional(),
}).nullable().optional();

export const SubscriptionArraySchema = z.array(SubscriptionSchema);

export type Subscription = z.infer<typeof SubscriptionSchema>;
export type SubscriptionArray = z.infer<typeof SubscriptionArraySchema>;

