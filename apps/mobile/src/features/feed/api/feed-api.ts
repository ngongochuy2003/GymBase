import type { AxiosInstance } from 'axios';
import { createMobHttpClient, DEFAULT_LOCAL_API_URL } from '@/core/api/mobile-client';
import { env } from '@/core/config/env';
import type { FeedPostData } from '../types';

export interface TacGiaBaiVietDto {
  id: string;
  ten_hien_thi: string;
  anh_dai_dien_url: string | null;
}

export interface BaiVietDto {
  id: string;
  tac_gia_id: string;
  tac_gia: TacGiaBaiVietDto;
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
  ngay_chinh_sua: string | null;
}

export interface PagingResultDto<T> {
  data: T[];
  meta: {
    page: number;
    page_size: number;
    total: number;
    total_pages: number;
  };
}

export interface KetQuaThichBaiVietDto {
  bai_viet_id: string;
  da_thich: boolean;
  so_luot_thich: number;
}

export function resolveMediaUrl(url?: string | null): string | undefined {
  if (!url) return undefined;
  const trimmed = url.trim();
  if (!trimmed) return undefined;
  if (
    trimmed.startsWith('http://') ||
    trimmed.startsWith('https://') ||
    trimmed.startsWith('data:')
  ) {
    return trimmed;
  }
  const apiHostBase = (env.apiUrl || DEFAULT_LOCAL_API_URL)
    .replace(/\/api\/v1\/mob\/?$/, '')
    .replace(/\/+$/, '');
  return `${apiHostBase}${trimmed.startsWith('/') ? '' : '/'}${trimmed}`;
}

export function formatRelativeTimeVi(isoDate: string | null | undefined, now = new Date()): string {
  if (!isoDate) return 'Vừa xong';
  const parsed = new Date(isoDate);
  if (Number.isNaN(parsed.getTime())) return 'Vừa xong';

  const diffSeconds = Math.max(0, Math.floor((now.getTime() - parsed.getTime()) / 1000));
  if (diffSeconds < 60) return 'Vừa xong';

  const diffMinutes = Math.floor(diffSeconds / 60);
  if (diffMinutes < 60) return `${diffMinutes} phút trước`;

  const diffHours = Math.floor(diffMinutes / 60);
  if (diffHours < 24) return `${diffHours} giờ trước`;

  const diffDays = Math.floor(diffHours / 24);
  if (diffDays < 30) return `${diffDays} ngày trước`;

  return parsed.toLocaleDateString('vi-VN');
}

export function mapBaiVietDtoToFeedPost(dto: BaiVietDto, now?: Date): FeedPostData {
  return {
    id: dto.id,
    author: {
      name: dto.tac_gia?.ten_hien_thi || 'Tài khoản GymBase',
      avatarUri: resolveMediaUrl(dto.tac_gia?.anh_dai_dien_url),
    },
    timeLabel: formatRelativeTimeVi(dto.ngay_dang ?? dto.ngay_tao, now),
    content: dto.noi_dung ?? '',
    imageUri: resolveMediaUrl(dto.hinh_anh_urls?.[0]),
    topicId: dto.chu_de ?? undefined,
    counts: {
      like: dto.so_luot_thich ?? 0,
      comment: dto.so_binh_luan ?? 0,
    },
    initialActive: {
      like: Boolean(dto.da_thich),
    },
  };
}

export interface FeedRepository {
  getFeedPosts(tab: string): Promise<FeedPostData[]>;
  toggleLike(postId: string): Promise<KetQuaThichBaiVietDto>;
}

export function createFeedRepository(
  client: AxiosInstance = createMobHttpClient(),
): FeedRepository {
  return {
    async getFeedPosts(tab: string): Promise<FeedPostData[]> {
      const response = await client.get<PagingResultDto<BaiVietDto>>('/baiviet/feed', {
          params: { tab, page: 1, page_size: 20 },
        });
        const items = response.data?.data ?? [];
      return items.map((dto) => mapBaiVietDtoToFeedPost(dto));
    },

    async toggleLike(postId: string): Promise<KetQuaThichBaiVietDto> {
      const response = await client.post<KetQuaThichBaiVietDto>(`/baiviet/${postId}/thich`);
      return response.data;
    },
  };
}

export const feedRepository = createFeedRepository();


