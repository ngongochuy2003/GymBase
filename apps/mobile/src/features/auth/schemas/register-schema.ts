import { z } from 'zod';

const MOBILE_PATTERN = /^(?:0|84)[35789][0-9]{8}$/;

/** Số di động Việt Nam: nhận `0…`, `+84…`, `84…`, bỏ khoảng trắng, chấm, gạch, ngoặc. Khớp luật của backend. */
export function isVietnamMobile(phone: string) {
  const digits = phone.replace(/[ .\-()]/g, '').replace(/^\+/, '');
  return MOBILE_PATTERN.test(digits);
}

export const TERMS_REQUIRED_MESSAGE = 'Bạn cần đồng ý Điều khoản sử dụng và Chính sách bảo mật.';

export const registerSchema = z
  .object({
    name: z.string().trim().min(1, 'Nhập họ và tên.').max(100, 'Họ và tên tối đa 100 ký tự.'),
    email: z
      .string()
      .trim()
      .min(1, 'Nhập email.')
      .regex(/^[^\s@]+@[^\s@]+\.[^\s@]+$/, 'Email không hợp lệ.'),
    phone: z
      .string()
      .trim()
      .min(1, 'Nhập số điện thoại.')
      .refine(isVietnamMobile, 'Số điện thoại phải là số di động Việt Nam.'),
    password: z
      .string()
      .min(1, 'Nhập mật khẩu.')
      .min(8, 'Mật khẩu cần ít nhất 8 ký tự.')
      .max(128, 'Mật khẩu tối đa 128 ký tự.'),
    confirm: z.string().min(1, 'Nhập lại mật khẩu để xác nhận.'),
    referral: z.string().trim().max(50, 'Mã giới thiệu tối đa 50 ký tự.'),
    agreed: z.boolean(),
  })
  .refine((form) => form.password === form.confirm, {
    path: ['confirm'],
    message: 'Mật khẩu xác nhận không khớp.',
  })
  .refine((form) => form.agreed, { path: ['agreed'], message: TERMS_REQUIRED_MESSAGE });
export type RegisterForm = z.infer<typeof registerSchema>;
