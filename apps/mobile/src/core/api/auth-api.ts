import { Platform } from 'react-native';
import { createAuthTransport } from '../auth/auth-transport';
import type { AuthResult, AuthUser } from '../auth/types';
import { tokenStorage } from '../storage/token-storage';
import { createApiClient } from './client';
import { resolveMobApiBaseUrl } from './mobile-client';

const baseURL = resolveMobApiBaseUrl();

/** Client không gắn token: dùng cho login và refresh, nơi 401 nghĩa là sai thông tin, không phải hết phiên. */
const anonymousClient = createApiClient(baseURL);

/** Token của phiên hiện tại và cầu nối cho client có xác thực. */
export const authRuntime = createAuthTransport({
  storage: tokenStorage,
  refresh: (refreshToken) => authApi.refresh(refreshToken),
});

const client = createApiClient(baseURL, authRuntime.transport);

export type LoginInput = { email: string; mat_khau: string; ten_thiet_bi?: string };

export type RegisterInput = {
  ho_ten: string;
  email: string;
  so_dien_thoai: string;
  mat_khau: string;
  dong_y_dieu_khoan: boolean;
  // TODO(REFERRAL): backend nhận nhưng chưa xử lý mã giới thiệu.
  ma_gioi_thieu?: string;
  ten_thiet_bi?: string;
};

/** Các endpoint xác thực của `/api/v1/mob/auth` (docs/api/xac-thuc.md). */
export const authApi = {
  async login(input: LoginInput): Promise<AuthResult> {
    const { data } = await anonymousClient.post<AuthResult>('/auth/login', {
      ten_thiet_bi: Platform.OS,
      ...input,
    });
    return data;
  },
  /** Tạo tài khoản, không có phiên: người dùng đăng nhập sau đó. */
  async register(input: RegisterInput): Promise<AuthUser> {
    const { data } = await anonymousClient.post<AuthUser>('/auth/register', {
      ten_thiet_bi: Platform.OS,
      ...input,
    });
    return data;
  },
  async google(input: { id_token: string; ten_thiet_bi?: string }): Promise<AuthResult> {
    const { data } = await anonymousClient.post<AuthResult>('/auth/google', {
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
  /** Người dùng đã bấm một trong hai nút của màn onboarding; server đặt `is_dang_nhap_lan_dau = false`. */
  async hoanTatDangNhapLanDau(): Promise<AuthUser> {
    const { data } = await client.post<AuthUser>('/auth/hoan-tat-dang-nhap-lan-dau');
    return data;
  },
  async me(): Promise<AuthUser> {
    const { data } = await client.get<AuthUser>('/auth/me');
    return data;
  },
};
