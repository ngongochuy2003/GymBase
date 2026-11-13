import { Platform } from 'react-native';
import { createAuthTransport } from '../auth/auth-transport';
import type { AuthResult, AuthUser } from '../auth/types';
import { env } from '../config/env';
import { tokenStorage } from '../storage/token-storage';
import { createApiClient } from './client';

const baseURL = `${env.apiUrl.replace(/\/+$/, '')}/api/v1/mob`;

/** Client không gắn token: dùng cho login và refresh, nơi 401 nghĩa là sai thông tin, không phải hết phiên. */
const anonymousClient = createApiClient(baseURL);

/** Token của phiên hiện tại và cầu nối cho client có xác thực. */
export const authRuntime = createAuthTransport({
  storage: tokenStorage,
  refresh: (refreshToken) => authApi.refresh(refreshToken),
});

const client = createApiClient(baseURL, authRuntime.transport);

export type LoginInput = { email: string; mat_khau: string; ten_thiet_bi?: string };

/** Các endpoint xác thực của `/api/v1/mob/auth` (docs/api/xac-thuc.md). */
export const authApi = {
  async login(input: LoginInput): Promise<AuthResult> {
    const { data } = await anonymousClient.post<AuthResult>('/auth/login', {
      ten_thiet_bi: Platform.OS,
      ...input,
    });
    return data;
  },
  async refresh(refreshToken: string): Promise<AuthResult> {
    const { data } = await anonymousClient.post<AuthResult>('/auth/refresh', {
      refresh_token: refreshToken,
    });
    return data;
  },
  async logout(): Promise<void> {
    await client.post('/auth/logout');
  },
  async me(): Promise<AuthUser> {
    const { data } = await client.get<AuthUser>('/auth/me');
    return data;
  },
};
