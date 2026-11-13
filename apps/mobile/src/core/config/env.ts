import { z } from 'zod';

// TODO(LEGAL): trang giữ chỗ cho tới khi có Điều khoản sử dụng và Chính sách bảo mật thật; đặt URL thật bằng
//   EXPO_PUBLIC_TERMS_URL và EXPO_PUBLIC_PRIVACY_URL.
const PLACEHOLDER_TERMS_URL = 'https://gymbase.example/dieu-khoan-su-dung';
const PLACEHOLDER_PRIVACY_URL = 'https://gymbase.example/chinh-sach-bao-mat';

const schema = z.object({
  // Chỉ quyết định nguồn dữ liệu lịch tập; đăng nhập luôn gọi API thật.
  dataMode: z.enum(['demo', 'api']).default('demo'),
  apiUrl: z.url({ error: 'EXPO_PUBLIC_API_URL là bắt buộc và phải là một URL hợp lệ.' }),
  // Web client ID của Google Cloud (cũng là `aud` mà backend kiểm tra); thiếu thì nút Google báo chưa cấu hình.
  googleWebClientId: z.string().min(1).optional(),
  termsUrl: z.url().default(PLACEHOLDER_TERMS_URL),
  privacyUrl: z.url().default(PLACEHOLDER_PRIVACY_URL),
});

export function parseEnv(input: {
  dataMode?: string;
  apiUrl?: string;
  googleWebClientId?: string;
  termsUrl?: string;
  privacyUrl?: string;
}) {
  return schema.parse(input);
}

// Biến để trống coi như chưa đặt.
const read = (value: string | undefined) => value || undefined;

export const env = parseEnv({
  dataMode: read(process.env.EXPO_PUBLIC_DATA_MODE),
  apiUrl: read(process.env.EXPO_PUBLIC_API_URL),
  googleWebClientId: read(process.env.EXPO_PUBLIC_GOOGLE_WEB_CLIENT_ID),
  termsUrl: read(process.env.EXPO_PUBLIC_TERMS_URL),
  privacyUrl: read(process.env.EXPO_PUBLIC_PRIVACY_URL),
});
