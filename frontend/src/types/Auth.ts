import {z} from 'zod';
import { UserSchema } from './User';

export const LoginSchema = z.object({
    email: z.email().max(255),
    password: z.string().min(8).max(128),
});

export const RegisterSchema = z.object({
    username: z.string().min(1).max(255),
    email: z.email().max(255),
    password: z.string().min(8).max(255),
});

export const LoginResponse = z.object({
    user: UserSchema,
    accessToken: z.string().min(32).max(512),
    accessTokenExpires: z.string().refine((s) => !Number.isNaN(Date.parse(s)), { message: "Invalid datetime" }),
    refreshToken: z.string().min(32).max(512),
    refreshTokenExpires: z.string().refine((s) => !Number.isNaN(Date.parse(s)), { message: "Invalid datetime" }),
});

export type LoginData = z.infer<typeof LoginSchema>;
export type RegisterData = z.infer<typeof RegisterSchema>;
export type LoginResponseData = z.infer<typeof LoginResponse>;