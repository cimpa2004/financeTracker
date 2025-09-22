import { z } from 'zod';

const DateString = z.string().refine((s) => !isNaN(Date.parse(s)), { message: "Invalid date format" });

export const CategoryNestedSchema = z.object({
    categoryId: z.string(),
    name: z.string().max(255),
    icon: z.string().nullable().optional(),
    color: z.string().nullable().optional(),
    type: z.string().max(50),
});

export const UserNestedSchema = z.object({
    userId: z.string(),
    username: z.string().max(255),
    email: z.email(),
});

export const SubscriptionNestedSchema = z.object({
    subscriptionId: z.string().optional(),
    name: z.string().max(255).optional(),
    amount: z.number().optional(),
    interval: z.string().max(50).optional(),
}).nullable().optional();

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