import { Platform } from 'react-native';
import { env } from '../config/env';

export const DEFAULT_LOCAL_API_URL = 'http://localhost:5020';

export function resolveMobApiBaseUrl(overrideUrl?: string): string {
  if (
    !overrideUrl &&
    Platform.OS === 'web' &&
    typeof window !== 'undefined' &&
    window.location?.hostname === 'localhost'
  ) {
    return 'http://localhost:5020/api/v1/mob';
  }
  const raw = (overrideUrl || env.apiUrl || DEFAULT_LOCAL_API_URL).trim().replace(/\/+$/, '');
  return raw.endsWith('/api/v1/mob') ? raw : `${raw}/api/v1/mob`;
}
