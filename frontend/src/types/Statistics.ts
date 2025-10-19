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

const spentByIntervalItem = z.object({
  periodStart: z.string(),
  spent: z.number(),
});

export const spentByIntervalResponseSchema = z.object({
  byPeriod: z.array(spentByIntervalItem),
});

export type Interval = 'Daily' | 'Weekly' | 'Monthly' | 'Yearly' | 'AllTime';
