import type { AxiosInstance } from 'axios';
import { createMobHttpClient, DEFAULT_LOCAL_API_URL } from '@/core/api/mobile-client';
import { env } from '@/core/config/env';
import { uploadMultipartFile } from '@/shared/file/file-uploader';
import type { ComposerPayload } from '../types';

export interface BaiVietCreateRequestDto {
  noi_dung: string | null;
  che_do_hien_thi: string;
  chu_de?: string;
  hinh_anh_urls: string[];
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
  };
}

export interface ComposerRepository {
  createPost(payload: ComposerPayload): Promise<BaiVietCreatedResponseDto>;
}

export function createComposerRepository(
  client: AxiosInstance = createMobHttpClient(),
): ComposerRepository {
  return {
    async createPost(payload: ComposerPayload): Promise<BaiVietCreatedResponseDto> {
      const body = toBaiVietCreateDto(payload);


      // Tự động upload các ảnh từ thiết bị (blob: hoặc file://) lên backend
      const apiHostBase = (env.apiUrl || DEFAULT_LOCAL_API_URL)
        .replace(/\/api\/v1\/mob\/?$/, '')
        .replace(/\/+$/, '');

      const processedMediaUrls: string[] = [];
      for (const uri of body.hinh_anh_urls) {
        if (uri.startsWith('http://') || uri.startsWith('https://')) {
          processedMediaUrls.push(uri);
        } else {
          const uploadResult = await uploadMultipartFile(uri, 'post', { apiBaseUrl: apiHostBase });
          const resultUrl = uploadResult?.content_url;
          if (!resultUrl) throw new Error('API không trả về URL sau khi tải tệp lên.');
          processedMediaUrls.push(resultUrl);
        }
      }
      body.hinh_anh_urls = processedMediaUrls;

      const response = await client.post<BaiVietCreatedResponseDto>('/baiviet', body);
      return response.data;
    },
  };
}

export const composerRepository = createComposerRepository();
