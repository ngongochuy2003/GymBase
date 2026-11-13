import { z } from 'zod';

export const loginSchema = z.object({
  email: z
    .string()
    .trim()
    .min(1, 'Nhập email.')
    .regex(/^[^\s@]+@[^\s@]+\.[^\s@]+$/, 'Email không hợp lệ.'),
  password: z.string().min(1, 'Nhập mật khẩu.'),
});
export type LoginForm = z.infer<typeof loginSchema>;
