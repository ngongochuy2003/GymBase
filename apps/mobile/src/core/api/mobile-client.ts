import axios, { type AxiosInstance } from 'axios';
import { createApiClient } from './client';
import { env } from '../config/env';
import { tokenStorage } from '../storage/token-storage';

export const DEFAULT_LOCAL_API_URL = 'http://localhost:5020';

export function resolveMobApiBaseUrl(overrideUrl?: string): string {
  const raw = (overrideUrl || env.apiUrl || DEFAULT_LOCAL_API_URL).trim().replace(/\/+$/, '');
  return raw.endsWith('/api/v1/mob') ? raw : `${raw}/api/v1/mob`;
}

export function createMobHttpClient(overrideUrl?: string): AxiosInstance {
  const baseURL = resolveMobApiBaseUrl(overrideUrl);
  return createApiClient(baseURL, {
    getAccessToken: async () => (await tokenStorage.read())?.accessToken ?? null,
    refresh: async () => {
      const current = await tokenStorage.read();
      if (!current) return null;
      const response = await axios.post<{ access_token: string; refresh_token: string }>(
        `${baseURL}/auth/refresh`,
        { refresh_token: current.refreshToken },
        { timeout: 15_000 },
      );
      const next = { accessToken: response.data.access_token, refreshToken: response.data.refresh_token };
      await tokenStorage.write(next);
      return next.accessToken;
    },
    onUnauthorized: async () => { await tokenStorage.clear(); },
  });
}
