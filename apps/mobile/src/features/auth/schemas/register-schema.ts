import { z } from 'zod';
import { EMAIL_FORMAT_MESSAGE } from './login-schema';

const MOBILE_PATTERN = /^(?:0|84)[35789][0-9]{8}$/;

/** Số di động Việt Nam: nhận `0…`, `+84…`, `84…`, bỏ khoảng trắng, chấm, gạch, ngoặc. Khớp luật của backend. */
export function isVietnamMobile(phone: string) {
  const digits = phone.replace(/[ .\-()]/g, '').replace(/^\+/, '');
  return MOBILE_PATTERN.test(digits);
}

export const TERMS_REQUIRED_MESSAGE = 'Bạn cần đồng ý Điều khoản sử dụng và Chính sách bảo mật.';

export const registerSchema = z
  .object({
    name: z
      .string()
      .trim()
      .min(1, 'Vui lòng nhập họ và tên.')
      .max(100, 'Họ và tên tối đa 100 ký tự.'),
    email: z
      .string()
      .trim()
      .min(1, 'Vui lòng nhập email.')
      .regex(/^[^\s@]+@[^\s@]+\.[^\s@]+$/, EMAIL_FORMAT_MESSAGE),
    // Để trống được; có nhập thì mới kiểm tra định dạng (khớp rule của backend).
    phone: z
      .string()
      .trim()
      .refine(
        (phone) => phone === '' || isVietnamMobile(phone),
        'Nhập số di động Việt Nam, ví dụ 0912 345 678.',
      ),
    password: z
      .string()
      .min(1, 'Vui lòng nhập mật khẩu.')
      .min(8, 'Mật khẩu cần ít nhất 8 ký tự.')
      .max(128, 'Mật khẩu tối đa 128 ký tự.'),
    confirm: z.string().min(1, 'Vui lòng nhập lại mật khẩu.'),
    referral: z.string().trim().max(50, 'Mã giới thiệu tối đa 50 ký tự.'),
    agreed: z.boolean(),
  })
  .refine((form) => form.password === form.confirm, {
    path: ['confirm'],
    message: 'Mật khẩu xác nhận không khớp.',
  })
  .refine((form) => form.agreed, { path: ['agreed'], message: TERMS_REQUIRED_MESSAGE });
export type RegisterForm = z.infer<typeof registerSchema>;
