import {z} from 'zod';

export const UserSchema = z.object({
    userId: z.string(),
    username: z.string().min(1).max(255),
    email: z.email().max(255),
    createdAt: z.string().refine((s) => !Number.isNaN(Date.parse(s)), { message: "Invalid datetime" }),
});

export const UserNestedSchema = z.object({
    userId: z.string(),
    username: z.string().max(255),
    email: z.email(),
});

export type User = z.infer<typeof UserSchema>;


