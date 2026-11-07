import { z } from 'zod';
export const demoSchema = z.object({
  name: z.string().trim().min(2, 'Nhập ít nhất 2 ký tự.').max(40, 'Tên tối đa 40 ký tự.'),
});
export type DemoForm = z.infer<typeof demoSchema>;
