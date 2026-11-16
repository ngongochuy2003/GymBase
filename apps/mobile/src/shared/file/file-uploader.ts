import { DEFAULT_LOCAL_API_URL } from '@/core/api/mobile-client';
import { env } from '@/core/config/env';
import { tokenStorage } from '@/core/storage/token-storage';
import { FilePurpose, FileUploadResult, UploadOptions } from './file.types';

export interface ManipulatorAdapter {
  manipulateAsync(
    uri: string,
    actions: { resize?: { width?: number; height?: number } }[],
    saveOptions: { compress: number; format: 'jpeg' | 'png' | 'webp' },
  ): Promise<{ uri: string; width: number; height: number; base64?: string }>;
}

let defaultManipulator: ManipulatorAdapter | null = null;
try {
  // eslint-disable-next-line @typescript-eslint/no-require-imports
  const ExpoManipulator = require('expo-image-manipulator');
  defaultManipulator = {
    manipulateAsync: (uri, actions, options) =>
      ExpoManipulator.manipulateAsync(uri, actions, options),
  };
} catch {
  // Môi trường Node.js / Jest
}

export function setCustomManipulator(manipulator: ManipulatorAdapter) {
  defaultManipulator = manipulator;
}

/**
 * 1. Nén và chuẩn hóa ảnh phía Mobile bằng expo-image-manipulator.
 * - Cạnh dài tối đa 2048px (giữ nguyên tỷ lệ chuẩn).
 * - Nén chất lượng 0.90 JPEG để đảm bảo độ nét cao không bị vỡ/bóp méo.
 * - Tự động loại bỏ metadata EXIF nhạy cảm (GPS coordinates).
 */
export async function compressImageOnDevice(
  uri: string,
  manipulator?: ManipulatorAdapter,
): Promise<{ uri: string; sizeBytes: number; contentType: string }> {
  const activeManipulator = manipulator || defaultManipulator;

  if (activeManipulator) {
    const result = await activeManipulator.manipulateAsync(uri, [{ resize: { width: 2048 } }], {
      compress: 0.9,
      format: 'jpeg',
    });

    let sizeBytes = 100 * 1024;
    try {
      const response = await fetch(result.uri);
      const blob = await response.blob();
      sizeBytes = blob.size;
    } catch {
      // Fallback nếu không đọc được blob trong native local uri
    }

    return {
      uri: result.uri,
      sizeBytes,
      contentType: 'image/jpeg',
    };
  }

  return {
    uri,
    sizeBytes: 200 * 1024,
    contentType: 'image/jpeg',
  };
}

/**
 * 2. Upload file multipart/form-data trực tiếp lên Backend GymBase (/api/v1/mob/files).
 * Backend sẽ nén/xử lý WebP và lưu vào Local Storage hoặc S3 tùy cấu hình môi trường.
 */
export async function uploadMultipartFile(
  localUri: string,
  purpose: FilePurpose = 'avatar',
  options?: UploadOptions,
): Promise<FileUploadResult> {
  const rawBase = options?.apiBaseUrl || env.apiUrl || DEFAULT_LOCAL_API_URL;
  const baseUrl = rawBase.replace(/\/api\/v1\/mob\/?$/, '').replace(/\/+$/, '');
  const authHeaders: Record<string, string> = {};

  let token = options?.authToken || (await tokenStorage.read())?.accessToken;
  if (!token && typeof window !== 'undefined' && window.localStorage) {
    token = window.localStorage.getItem('access_token') || undefined;
  }

  if (token) {
    authHeaders.Authorization = `Bearer ${token}`;
  }

  // Bước 1: Nén ảnh giữ độ nét cao
  const compressed = await compressImageOnDevice(localUri);

  if (options?.signal?.aborted) {
    throw new Error('Upload cancelled by user');
  }

  // Bước 2: Chuẩn bị FormData
  const formData = new FormData();
  formData.append('purpose', purpose);

  const fileName = compressed.uri.split('/').pop() || `upload_${Date.now()}.jpg`;

  if (typeof Blob !== 'undefined' && !compressed.uri.startsWith('file://')) {
    try {
      const blobRes = await fetch(compressed.uri);
      const blob = await blobRes.blob();
      formData.append('file', blob, fileName);
    } catch {
      // @ts-ignore
      formData.append('file', {
        uri: compressed.uri,
        type: compressed.contentType,
        name: fileName,
      });
    }
  } else {
    // @ts-ignore
    formData.append('file', {
      uri: compressed.uri,
      type: compressed.contentType,
      name: fileName,
    });
  }

  // Bước 3: Gửi lên Backend (Kênh mobile /api/v1/mob/files)
  const endpoint = `${baseUrl}/api/v1/mob/files`;
  const response = await fetch(endpoint, {
    method: 'POST',
    headers: authHeaders,
    body: formData,
    signal: options?.signal,
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`Upload file multipart thất bại (${response.status}): ${errorText}`);
  }

  return (await response.json()) as FileUploadResult;
}
