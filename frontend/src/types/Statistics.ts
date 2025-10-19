import { z } from 'zod';
import { CategorySchema } from './Category';

const spentByCategoryResponseSchemabody = z.object({
    category: CategorySchema.nullable().optional(),
    spent: z.number(),
});

const spentByCategoryResponseSchemabodyArray = z.array(spentByCategoryResponseSchemabody);

export const spentByCategoryResponseSchema = z.object({
  totalSpent: z.number(),
  byCategory: spentByCategoryResponseSchemabodyArray,
});

export type Interval = 'Daily' | 'Weekly' | 'Monthly' | 'Yearly' | 'AllTime';
