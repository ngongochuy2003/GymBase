import { Platform } from 'react-native';
import { env } from '../config/env';

export const DEFAULT_LOCAL_API_URL = 'http://localhost:5020';

export function resolveMobApiBaseUrl(): string {
  if (
    Platform.OS === 'web' &&
    typeof window !== 'undefined' &&
    window.location?.hostname === 'localhost'
  ) {
    return 'http://localhost:5020/api/v1/mob';
  }
  const raw = env.apiUrl.trim().replace(/\/+$/, '');
  return raw.endsWith('/api/v1/mob') ? raw : `${raw}/api/v1/mob`;
}
