import { z } from 'zod';
import { CategorySchema } from './Category';
import { UserNestedSchema } from './User';

const DateString = z.string().refine((s) => !isNaN(Date.parse(s)), { message: 'Invalid date format' });

export const BudgetSchema = z.object({
	budgetId: z.string(),
	name: z.string().max(255).nullable().optional(),
	amount: z.number(),
	startDate: DateString.nullable().optional(),
	endDate: DateString.nullable().optional(),
	createdAt: DateString.nullable().optional(),
	category: CategorySchema.nullable().optional(),
	user: UserNestedSchema.nullable().optional(),
});

export const BudgetArraySchema = z.array(BudgetSchema);

export const BudgetNestedSchema = z.object({
	budgetId: z.string().optional(),
	name: z.string().max(255).optional(),
	amount: z.number().optional(),
	startDate: DateString.optional(),
	endDate: DateString.optional(),
}).nullable().optional();

export const BudgetFormSchema = z.object({
	categoryId: z.string().nullable().optional(),
	amount: z.number().min(0, { message: 'Amount must be greater than or equal to 0' }),
	name: z.string().max(255).nullable().optional(),
	startDate: z.string().nullable().optional(),
	endDate: z.string().nullable().optional(),
	interval: z.enum(['weekly', 'monthly', 'yearly']).nullable().optional(),
});

export type Budget = z.infer<typeof BudgetSchema>;
export type BudgetArray = z.infer<typeof BudgetArraySchema>;
export type BudgetFormInput = z.infer<typeof BudgetFormSchema>;

export const BudgetStatusSchema = z.object({
	budgetId: z.string(),
	name: z.string().max(255).nullable().optional(),
	amount: z.number(),
	spent: z.number(),
	remaining: z.number(),
	startDate: DateString.nullable().optional(),
	endDate: DateString.nullable().optional(),
	createdAt: DateString.nullable().optional(),
	category: CategorySchema.nullable().optional(),
	user: UserNestedSchema.nullable().optional(),
});

export const BudgetsStatusArraySchema = z.array(BudgetStatusSchema);

export type BudgetStatus = z.infer<typeof BudgetStatusSchema>;
export type BudgetsStatusArray = z.infer<typeof BudgetsStatusArraySchema>;

