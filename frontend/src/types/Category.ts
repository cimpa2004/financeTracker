import { z } from 'zod';

export const CategoryNestedSchema = z.object({
    categoryId: z.string(),
    name: z.string().max(255),
    icon: z.string().nullable().optional(),
    color: z.string().nullable().optional(),
    type: z.string().max(50),
});