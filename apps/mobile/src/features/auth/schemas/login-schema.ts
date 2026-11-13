import { z } from 'zod';
export const loginSchema = z.object({
  userName: z.string().trim().min(1, 'Nhập tên đăng nhập.'),
  password: z.string().min(1, 'Nhập mật khẩu.'),
});
export type LoginForm = z.infer<typeof loginSchema>;
