import type { AxiosInstance } from 'axios';
import { getMobClient } from '@/core/api/mobile-client';
import { uploadMultipartFile, isVideoUrl } from '@/shared/file/file-uploader';
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
  client: AxiosInstance = getMobClient(),
): ComposerRepository {
  return {
    async createPost(payload: ComposerPayload): Promise<BaiVietCreatedResponseDto> {
      const body = toBaiVietCreateDto(payload);


      // Tự động upload các ảnh từ thiết bị (blob: hoặc file://) lên backend
      const itemsToUpload =
        payload.mediaItems && payload.mediaItems.length > 0
          ? payload.mediaItems
          : body.hinh_anh_urls.map((uri) => ({
              id: uri,
              uri,
              type: undefined as 'image' | 'video' | undefined,
              mimeType: undefined,
            }));

      const processedMediaUrls: string[] = [];
      for (const item of itemsToUpload) {
        const uri = item.uri;
        if (uri.startsWith('http://') || uri.startsWith('https://')) {
          processedMediaUrls.push(uri);
        } else {
          const isVideo = item.type === 'video' || isVideoUrl(uri);
          const uploadResult = await uploadMultipartFile(uri, 'post', {
            contentType: isVideo ? (item.mimeType || 'video/mp4') : undefined,
          });
          if (!uploadResult?.file_id) throw new Error('API không trả về file_id sau khi tải tệp lên.');
          // TODO(FILE): bài viết còn nhận URL chuỗi nên tạm dùng đường đọc cũ (/files/{id}/content); đổi sang gửi file_ids khi
          //   POST /baiviet nhận file_ids và trả URL ký (xem docs/architecture/upload-file.md, mục 8, bước 3).
          processedMediaUrls.push(`/api/v1/mob/files/${uploadResult.file_id}/content`);
        }
      }
      body.hinh_anh_urls = processedMediaUrls;

      const response = await client.post<BaiVietCreatedResponseDto>('/baiviet', body);
      return response.data;
    },
  };
}

export const composerRepository = createComposerRepository();
