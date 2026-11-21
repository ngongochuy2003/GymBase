import type { AxiosInstance } from 'axios';
import { getMobClient } from '@/core/api/mobile-client';
import { resolveMediaUrl } from '@/shared/file';
import type { FeedPostData } from '../types';

export { resolveMediaUrl };

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

export interface BinhLuanDto {
  id: string;
  bai_viet_id: string;
  tai_khoan_id: string;
  ten_nguoi_dung: string;
  anh_dai_dien_url: string | null;
  noi_dung: string;
  tuong_tac_cha_id: string | null;
  ngay_tao: string;
}

export interface BinhLuanCreateDto {
  noi_dung: string;
  tuong_tac_cha_id?: string | null;
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
  const images = (dto.hinh_anh_urls ?? [])
    .map((url) => resolveMediaUrl(url))
    .filter((url): url is string => Boolean(url));

  return {
    id: dto.id,
    author: {
      name: dto.tac_gia?.ten_hien_thi || 'Tài khoản GymBase',
      avatarUri: resolveMediaUrl(dto.tac_gia?.anh_dai_dien_url),
    },
    timeLabel: formatRelativeTimeVi(dto.ngay_dang ?? dto.ngay_tao, now),
    content: dto.noi_dung ?? '',
    imageUri: images[0],
    imageUris: images,
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

export interface FeedPageResult {
  posts: FeedPostData[];
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
  hasNextPage: boolean;
}

export interface FeedRepository {
  getFeedPosts(tab: string, page?: number, pageSize?: number): Promise<FeedPostData[]>;
  getPaginatedFeed(tab: string, page?: number, pageSize?: number): Promise<FeedPageResult>;
  getPostById(postId: string): Promise<FeedPostData | null>;
  toggleLike(postId: string): Promise<KetQuaThichBaiVietDto>;
  getComments(postId: string): Promise<BinhLuanDto[]>;
  addComment(postId: string, dto: BinhLuanCreateDto): Promise<BinhLuanDto>;
}

export function createFeedRepository(
  client: AxiosInstance = getMobClient(),
): FeedRepository {
  async function getPaginatedFeed(
    tab: string,
    page = 1,
    pageSize = 10,
  ): Promise<FeedPageResult> {
    const response = await client.get<PagingResultDto<BaiVietDto>>('/baiviet/feed', {
      params: { tab, page, page_size: pageSize },
    });
    const items = response.data?.data ?? [];
    const meta = response.data?.meta;
    const currentPage = meta?.page ?? page;
    const currentPageSize = meta?.page_size ?? pageSize;
    const total = meta?.total ?? items.length;
    const totalPages =
      meta?.total_pages && meta.total_pages > 0
        ? meta.total_pages
        : Math.max(1, Math.ceil(total / (currentPageSize || 1)));
    const hasNextPage = currentPage < totalPages;

    return {
      posts: items.map((dto) => mapBaiVietDtoToFeedPost(dto)),
      page: currentPage,
      pageSize: currentPageSize,
      total,
      totalPages,
      hasNextPage,
    };
  }

  return {
    getPaginatedFeed,
    async getFeedPosts(tab: string, page = 1, pageSize = 20): Promise<FeedPostData[]> {
      const res = await getPaginatedFeed(tab, page, pageSize);
      return res.posts;
    },

    async getPostById(postId: string): Promise<FeedPostData | null> {
      try {
        const response = await client.get<BaiVietDto>(`/baiviet/${postId}`);
        if (response.data) {
          return mapBaiVietDtoToFeedPost(response.data);
        }
        return null;
      } catch {
        return null;
      }
    },

    async toggleLike(postId: string): Promise<KetQuaThichBaiVietDto> {
      const response = await client.post<KetQuaThichBaiVietDto>(`/baiviet/${postId}/thich`);
      return response.data;
    },

    async getComments(postId: string): Promise<BinhLuanDto[]> {
      try {
        const response = await client.get<BinhLuanDto[]>(`/baiviet/${postId}/binh-luan`);
        return response.data ?? [];
      } catch {
        return [];
      }
    },

    async addComment(postId: string, dto: BinhLuanCreateDto): Promise<BinhLuanDto> {
      const response = await client.post<BinhLuanDto>(`/baiviet/${postId}/binh-luan`, dto);
      return response.data;
    },
  };
}

export const feedRepository = createFeedRepository();


