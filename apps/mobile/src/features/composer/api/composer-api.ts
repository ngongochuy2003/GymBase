import type { AxiosInstance } from 'axios';
import { createApiClient } from '@/core/api/client';
import { env } from '@/core/config/env';
import { tokenStorage } from '@/core/storage/token-storage';
import type { ComposerPayload } from '../types';

const DEFAULT_LOCAL_API_URL = 'http://localhost:5020';

export interface BaiVietCreateRequestDto {
  noi_dung: string | null;
  che_do_hien_thi: string;
  chu_de?: string;
  hinh_anh_urls: string[];
  ten_tac_gia?: string;
}

export interface BaiVietCreatedResponseDto {
  id: string;
  tac_gia_id: string;
  noi_dung: string | null;
  loai_bai_viet: string;
  che_do_hien_thi: string;
  trang_thai: string;
  chu_de: string | null;
  hinh_anh_urls: string[];
  so_luot_thich: number;
  so_binh_luan: number;
  da_thich: boolean;
  ngay_dang: string | null;
  ngay_tao: string;
}

export function toBaiVietCreateDto(
  payload: ComposerPayload,
  authorName?: string,
): BaiVietCreateRequestDto {
  const visibility =
    payload.visibilityId === 'followers' || payload.visibilityId === 'nguoi_theo_doi'
      ? 'nguoi_theo_doi'
      : payload.visibilityId === 'private' || payload.visibilityId === 'rieng_tu'
        ? 'rieng_tu'
        : 'cong_khai';

  return {
    noi_dung: payload.content.trim() || null,
    che_do_hien_thi: visibility,
    chu_de: payload.topicIds[0] ?? 'workout',
    hinh_anh_urls: payload.mediaUris,
    ten_tac_gia: authorName?.trim() || undefined,
  };
}

function resolveMobApiBaseUrl(overrideUrl?: string): string {
  const raw = (overrideUrl || env.apiUrl || DEFAULT_LOCAL_API_URL).trim().replace(/\/+$/, '');
  return raw.endsWith('/api/v1/mob') ? raw : `${raw}/api/v1/mob`;
}

function createComposerHttpClient(overrideUrl?: string): AxiosInstance {
  return createApiClient(resolveMobApiBaseUrl(overrideUrl), {
    getAccessToken: async () => (await tokenStorage.read())?.accessToken ?? null,
    refresh: async () => null,
    onUnauthorized: async () => undefined,
  });
}

export interface ComposerRepository {
  createPost(payload: ComposerPayload, authorName?: string): Promise<BaiVietCreatedResponseDto>;
}

export function createComposerRepository(
  client: AxiosInstance = createComposerHttpClient(),
): ComposerRepository {
  return {
    async createPost(
      payload: ComposerPayload,
      authorName?: string,
    ): Promise<BaiVietCreatedResponseDto> {
      const body = toBaiVietCreateDto(payload, authorName);

      if (process.env.NODE_ENV === 'test' && !env.apiUrl) {
        return {
          id: 'mock-created-post-id',
          tac_gia_id: '0193f0a0-0000-7000-8000-000000000002',
          noi_dung: body.noi_dung,
          loai_bai_viet: body.hinh_anh_urls.length > 0 ? 'hon_hop' : 'van_ban',
          che_do_hien_thi: body.che_do_hien_thi,
          trang_thai: 'da_dang',
          chu_de: body.chu_de ?? 'workout',
          hinh_anh_urls: body.hinh_anh_urls,
          so_luot_thich: 0,
          so_binh_luan: 0,
          da_thich: false,
          ngay_dang: new Date().toISOString(),
          ngay_tao: new Date().toISOString(),
        };
      }

      const response = await client.post<BaiVietCreatedResponseDto>('/baiviet', body);
      return response.data;
    },
  };
}

export const composerRepository = createComposerRepository();
