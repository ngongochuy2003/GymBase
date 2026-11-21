import type { AxiosInstance } from 'axios';
import { authRuntime } from './auth-api';
import { createApiClient } from './client';
import { resolveMobApiBaseUrl } from './mob-base-url';

export { DEFAULT_LOCAL_API_URL, resolveMobApiBaseUrl } from './mob-base-url';

/**
 * Client có xác thực cho các module (profile, danh mục, feed, composer).
 * Dùng chung `authRuntime.transport` với `authApi`, nên mọi client thấy cùng một token,
 * refresh chỉ chạy một nơi và phiên được báo khi hết hạn.
 */
export function createMobHttpClient(overrideUrl?: string): AxiosInstance {
  return createApiClient(resolveMobApiBaseUrl(overrideUrl), authRuntime.transport);
}
