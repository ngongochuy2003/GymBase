import { z } from 'zod';

const schema = z.object({ apiUrl: z.url() });

export function parseEnv(input: { apiUrl?: string }) {
  return schema.parse(input);
}

export const env = parseEnv({ apiUrl: process.env.EXPO_PUBLIC_API_URL });
