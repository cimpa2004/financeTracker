import { z } from 'zod';

export const TransactionSchema = z.object({
    transactionId: z.string(),
    userId: z.string(),
    categoryId: z.string().nullable(),
    amount: z.number(),
    description: z.string().max(1000, { message: "Description must be at most 1000 characters" }).nullable().optional(),
    name: z.string().max(255, { message: "Name must be at most 255 characters" }).nullable().optional(),
    date: z.string().refine((date) => !isNaN(Date.parse(date)), { message: "Invalid date format" }),
    createdAt: z.string().refine((date) => !isNaN(Date.parse(date)), { message: "Invalid date format" }),
    updatedAt: z.string().refine((date) => !isNaN(Date.parse(date)), { message: "Invalid date format" }).optional(),
    subscriptionId: z.string().nullable().optional(),
});

export const TransactionArraySchema = z.array(TransactionSchema);

export type Transaction = z.infer<typeof TransactionSchema>;
export type TransactionArray = z.infer<typeof TransactionArraySchema>;