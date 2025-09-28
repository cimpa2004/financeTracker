import { z } from 'zod';

export const CategorySchema = z.object({
    categoryId: z.string(),
    name: z.string().max(255),
    icon: z.string().nullable().optional(),
    color: z.string().nullable().optional(),
    type: z.string().max(50),
});

export type AddCategoryInput = {
  name: string;
  icon?: string | null;
  color?: string | null;
  type: string;
  isPublic?: boolean;
};