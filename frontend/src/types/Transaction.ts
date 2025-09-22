import { z } from 'zod';
import { UserNestedSchema } from './User';
import { SubscriptionNestedSchema } from './Subscription';
import { CategoryNestedSchema } from './Category';

const DateString = z.string().refine((s) => !isNaN(Date.parse(s)), { message: "Invalid date format" });


export const TransactionSchema = z.object({
    transactionId: z.string(),
    // accept either nested category object or categoryId string (or null)
    category: z.union([CategoryNestedSchema, z.string()]).nullable().optional(),
    user: z.union([UserNestedSchema, z.string()]).nullable().optional(),

    amount: z.number(),
    description: z.string().max(1000, { message: "Description must be at most 1000 characters" }).nullable().optional(),
    name: z.string().max(255, { message: "Name must be at most 255 characters" }).nullable().optional(),

    date: DateString,
    createdAt: DateString,
    updatedAt: DateString.optional(),

    subscription: SubscriptionNestedSchema,
});

export const TransactionArraySchema = z.array(TransactionSchema);

export type Transaction = z.infer<typeof TransactionSchema>;
export type TransactionArray = z.infer<typeof TransactionArraySchema>;