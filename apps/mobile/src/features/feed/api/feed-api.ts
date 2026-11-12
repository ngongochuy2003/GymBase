import type { AxiosInstance } from 'axios';
import { createApiClient } from '@/core/api/client';
import { env } from '@/core/config/env';
import { tokenStorage } from '@/core/storage/token-storage';
import { feedPosts } from '../mock/feed-posts';
import type { FeedPostData } from '../types';

export const DEFAULT_LOCAL_API_URL = 'http://localhost:5020';

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

const DEFAULT_AVATAR = 'https://picsum.photos/seed/gymbase-minh-khoi/160/160';

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
      name: dto.tac_gia?.ten_hien_thi || 'Minh Khôi',
      avatarUri: dto.tac_gia?.anh_dai_dien_url || DEFAULT_AVATAR,
    },
    timeLabel: formatRelativeTimeVi(dto.ngay_dang ?? dto.ngay_tao, now),
    content: dto.noi_dung ?? '',
    imageUri: dto.hinh_anh_urls?.[0],
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

export function filterFeedPostsByTab(posts: FeedPostData[], tab: string): FeedPostData[] {
  const normalized = tab.trim().toLowerCase();
  if (normalized === 'workout') {
    return posts.filter(
      (p) =>
        p.topicId === 'workout' ||
        /push|pull|squat|tập|ngực|vai|#workout|#tapluyen/i.test(p.content),
    );
  }
  if (normalized === 'knowledge') {
    return posts.filter(
      (p) =>
        p.topicId === 'knowledge' ||
        p.topicId === 'meal-prep' ||
        p.topicId === 'tips' ||
        /meal prep|siết mỡ|protein|mẹo|#kienthuc|#mealprep/i.test(p.content),
    );
  }
  if (normalized === 'trending') {
    return [...posts].sort(
      (a, b) =>
        (b.counts?.like ?? 0) +
        (b.counts?.comment ?? 0) -
        ((a.counts?.like ?? 0) + (a.counts?.comment ?? 0)),
    );
  }
  return posts;
}

export function resolveMobApiBaseUrl(overrideUrl?: string): string {
  const raw = (overrideUrl || env.apiUrl || DEFAULT_LOCAL_API_URL).trim().replace(/\/+$/, '');
  return raw.endsWith('/api/v1/mob') ? raw : `${raw}/api/v1/mob`;
}

export function createMobHttpClient(overrideUrl?: string): AxiosInstance {
  return createApiClient(resolveMobApiBaseUrl(overrideUrl), {
    getAccessToken: async () => (await tokenStorage.read())?.accessToken ?? null,
    refresh: async () => null,
    onUnauthorized: async () => undefined,
  });
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
      if (process.env.NODE_ENV === 'test' && !env.apiUrl) {
        return filterFeedPostsByTab(feedPosts, tab);
      }
      try {
        const response = await client.get<PagingResultDto<BaiVietDto>>('/baiviet/feed', {
          params: { tab, page: 1, page_size: 20 },
        });
        const items = response.data?.data ?? [];
        if (items.length === 0 && (!tab || tab === 'for-you')) {
          return feedPosts;
        }
        return items.map((dto) => mapBaiVietDtoToFeedPost(dto));
      } catch {
        return filterFeedPostsByTab(feedPosts, tab);
      }
    },

    async toggleLike(postId: string): Promise<KetQuaThichBaiVietDto> {
      if (process.env.NODE_ENV === 'test' && !env.apiUrl) {
        return { bai_viet_id: postId, da_thich: true, so_luot_thich: 1 };
      }
      const response = await client.post<KetQuaThichBaiVietDto>(`/baiviet/${postId}/thich`);
      return response.data;
    },
  };
}

export const feedRepository = createFeedRepository();
