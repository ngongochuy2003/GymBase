import type { AxiosInstance } from 'axios';
import { getMobClient } from '@/core/api/mobile-client';
import { deleteUploadedFile, isVideoUrl, uploadMultipartFile } from '@/shared/file/file-uploader';
import type { ComposerPayload } from '../types';

export interface BaiVietCreateRequestDto {
  noi_dung: string | null;
  che_do_hien_thi: string;
  chu_de?: string;
  /** Chỉ URL http(s) của nội dung có sẵn; tệp tải lên đi qua `file_ids`. */
  hinh_anh_urls: string[];
  /** Id các tệp đã upload (`muc_dich = post`), theo thứ tự hiển thị. */
  file_ids?: string[];
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
  client: AxiosInstance = getMobClient(),
): ComposerRepository {
  return {
    async createPost(payload: ComposerPayload): Promise<BaiVietCreatedResponseDto> {
      const body = toBaiVietCreateDto(payload);

      // Ảnh/video từ thiết bị (blob: hoặc file://) được upload trước, bài viết chỉ mang file_id; URL http(s) giữ nguyên.
      const itemsToUpload =
        payload.mediaItems && payload.mediaItems.length > 0
          ? payload.mediaItems
          : body.hinh_anh_urls.map((uri) => ({
              id: uri,
              uri,
              type: undefined as 'image' | 'video' | undefined,
              mimeType: undefined,
            }));

      const externalUrls: string[] = [];
      const fileIds: string[] = [];
      try {
        for (const item of itemsToUpload) {
          const uri = item.uri;
          if (uri.startsWith('http://') || uri.startsWith('https://')) {
            externalUrls.push(uri);
            continue;
          }
          const isVideo = item.type === 'video' || isVideoUrl(uri);
          const uploaded = await uploadMultipartFile(uri, 'post', {
            contentType: isVideo ? (item.mimeType || 'video/mp4') : undefined,
          });
          if (!uploaded?.file_id) throw new Error('API không trả về file_id sau khi tải tệp lên.');
          fileIds.push(uploaded.file_id);
        }
        body.hinh_anh_urls = externalUrls;
        body.file_ids = fileIds;

        const response = await client.post<BaiVietCreatedResponseDto>('/baiviet', body);
        return response.data;
      } catch (error) {
        // Đăng bài hỏng thì bỏ các tệp đã tải lên, khỏi để chúng nằm lại ở trạng thái chờ gắn.
        await Promise.allSettled(fileIds.map((id) => deleteUploadedFile(id)));
        throw error;
      }
    },
  };
}

export const composerRepository = createComposerRepository();
