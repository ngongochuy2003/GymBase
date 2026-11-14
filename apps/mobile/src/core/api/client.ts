import {
  create,
  isAxiosError,
  type AxiosRequestConfig,
  type InternalAxiosRequestConfig,
} from 'axios';
import { toApiError } from './api-error';

export interface AuthTransport {
  getAccessToken(): Promise<string | null>;
  // The backend integration owns rotation, storage and revocation.
  refresh(): Promise<string | null>;
  onUnauthorized(): Promise<void>;
}

type RetryConfig = InternalAxiosRequestConfig & { _retried?: boolean };

export function createApiClient(
  baseURL: string,
  auth?: AuthTransport,
  options?: AxiosRequestConfig,
) {
  const client = create({ ...options, baseURL, timeout: 15_000 });
  let refreshTask: Promise<string | null> | undefined;

  client.interceptors.request.use(async (config) => {
    const token = await auth?.getAccessToken();
    if (token) {
      if (typeof config.headers?.set === 'function') {
        config.headers.set('Authorization', `Bearer ${token}`);
      } else {
        config.headers = config.headers || {};
        config.headers['Authorization'] = `Bearer ${token}`;
      }
    }
    return config;
  });

  client.interceptors.response.use(
    (response) => response,
    async (error: unknown) => {
      if (isAxiosError(error) && error.response?.status === 401 && auth) {
        const config = error.config as RetryConfig | undefined;
        if (config && !config._retried) {
          config._retried = true;
          refreshTask ??= auth.refresh().finally(() => {
            refreshTask = undefined;
          });
          try {
            const token = await refreshTask;
            if (token) {
              config.headers.set('Authorization', `Bearer ${token}`);
              return client.request(config);
            }
          } catch {
            // Never expose tokens or backend exception details.
          }
        }
        await auth.onUnauthorized();
      }
      return Promise.reject(toApiError(error));
    },
  );
  return client;
}
