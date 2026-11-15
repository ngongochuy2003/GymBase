import { z } from 'zod';

export const EMAIL_FORMAT_MESSAGE = 'Email chưa đúng định dạng, ví dụ ban@email.com.';

export const loginSchema = z.object({
  email: z
    .string()
    .trim()
    .min(1, 'Vui lòng nhập email.')
    .regex(/^[^\s@]+@[^\s@]+\.[^\s@]+$/, EMAIL_FORMAT_MESSAGE),
  password: z.string().min(1, 'Vui lòng nhập mật khẩu.'),
});
export type LoginForm = z.infer<typeof loginSchema>;
