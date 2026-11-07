import { z } from 'zod';

const schema = z.object({
  dataMode: z.enum(['demo', 'api']).default('demo'),
  apiUrl: z.union([z.literal(''), z.url()]).default(''),
});

export function parseEnv(input: { dataMode?: string; apiUrl?: string }) {
  const result = schema.parse(input);
  if (result.dataMode === 'api' && !result.apiUrl) {
    throw new Error('EXPO_PUBLIC_API_URL is required in API mode.');
  }
  return result;
}

export const env = parseEnv({
  dataMode: process.env.EXPO_PUBLIC_DATA_MODE,
  apiUrl: process.env.EXPO_PUBLIC_API_URL,
});
