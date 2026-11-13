import { z } from 'zod';

const schema = z.object({
  // Chỉ quyết định nguồn dữ liệu lịch tập; đăng nhập luôn gọi API thật.
  dataMode: z.enum(['demo', 'api']).default('demo'),
  apiUrl: z.url({ error: 'EXPO_PUBLIC_API_URL là bắt buộc và phải là một URL hợp lệ.' }),
});

export function parseEnv(input: { dataMode?: string; apiUrl?: string }) {
  return schema.parse(input);
}

export const env = parseEnv({
  dataMode: process.env.EXPO_PUBLIC_DATA_MODE,
  apiUrl: process.env.EXPO_PUBLIC_API_URL,
});
