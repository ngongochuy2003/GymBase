import axios, { isAxiosError } from 'axios';
import { Platform } from 'react-native';
import { resolveApiOrigin } from '@/core/config/api-origin';
import { authRuntime } from '@/core/api/auth-api';
import { tokenStorage } from '@/core/storage/token-storage';
import { FilePurpose, FileUploadResult, UploadOptions } from './file.types';

/** Thời gian chờ tối đa cho một lần tải tệp lên (ms); tệp lớn trên mạng yếu cần lâu hơn các lệnh API thường. */
const UPLOAD_TIMEOUT_MS = 120_000;

/** Upload thất bại do server trả mã lỗi; `status` để giao diện nói đúng nguyên nhân (401 hết phiên, 400 tệp không hợp lệ...). */
export class UploadError extends Error {
  constructor(
    message: string,
    public readonly status: number,
  ) {
    super(message);
    this.name = 'UploadError';
  }
}

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
 * Kiểm tra một URI hoặc URL có phải là định dạng video hay không.
 */
export function isVideoUrl(urlOrPath?: string | null): boolean {
  if (!urlOrPath) return false;
  const cleanPart = urlOrPath.split('?')[0] ?? '';
  const clean = (cleanPart.split('#')[0] ?? '').toLowerCase();
  return (
    clean.endsWith('.mp4') ||
    clean.endsWith('.mov') ||
    clean.endsWith('.webm') ||
    clean.endsWith('.m4v') ||
    clean.endsWith('.avi') ||
    clean.endsWith('.mkv') ||
    clean.includes('.mp4') ||
    clean.includes('.mov') ||
    clean.includes('.webm') ||
    clean.includes('video/') ||
    urlOrPath.includes('video/mp4') ||
    urlOrPath.includes('type=video')
  );
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
  const baseUrl = resolveApiOrigin(options?.apiBaseUrl);
  const authHeaders: Record<string, string> = {};

  // Access token chỉ sống 15 phút: lấy qua phiên hiện tại để có thể làm mới khi server trả 401 (xem bên dưới).
  let token =
    options?.authToken ||
    (await authRuntime.transport.getAccessToken()) ||
    (await tokenStorage.read())?.accessToken;
  if (!token && typeof window !== 'undefined' && window.localStorage) {
    token = window.localStorage.getItem('access_token') || undefined;
  }

  if (token) {
    authHeaders.Authorization = `Bearer ${token}`;
  }

  // Chứng chỉ PT là tài liệu PDF: gửi nguyên bản, không nén ảnh và không coi là video.
  const isDocument = purpose === 'chung_chi_pt';
  let isVideo =
    !isDocument && (isVideoUrl(localUri) || Boolean(options?.contentType?.startsWith('video/')));
  let fileType =
    options?.contentType || (isDocument ? 'application/pdf' : isVideo ? 'video/mp4' : 'image/jpeg');
  let blobData: Blob | null = null;

  // Nếu trên Web (blob: URL) hoặc có fetch, đọc MIME type trực tiếp từ Blob
  if (typeof fetch !== 'undefined' && localUri.startsWith('blob:')) {
    try {
      const blobRes = await fetch(localUri);
      blobData = await blobRes.blob();
      if (blobData.type) {
        fileType = blobData.type;
        if (blobData.type.startsWith('video/')) {
          isVideo = true;
        }
      }
    } catch {
      // Fallback
    }
  }

  let fileUri = localUri;
  let fileName =
    options?.fileName ||
    localUri.split('/').pop() ||
    (isDocument
      ? `tai-lieu_${Date.now()}.pdf`
      : isVideo
        ? `video_${Date.now()}.mp4`
        : `upload_${Date.now()}.jpg`);
  if (
    isVideo &&
    !fileName.endsWith('.mp4') &&
    !fileName.endsWith('.mov') &&
    !fileName.endsWith('.webm')
  ) {
    fileName = `${fileName}.mp4`;
  }

  if (isDocument) {
    // Giữ nguyên tệp gốc.
  } else if (!isVideo) {
    try {
      // Bước 1: Nén ảnh giữ độ nét cao bằng expo-image-manipulator
      const compressed = await compressImageOnDevice(localUri);
      fileUri = compressed.uri;
      fileType = compressed.contentType;
      fileName = compressed.uri.split('/').pop() || `upload_${Date.now()}.jpg`;
    } catch {
      // Fallback nếu không nén được (ví dụ video bị lọt vào nhánh ảnh hoặc file đặc biệt)
      fileUri = localUri;
    }
  } else {
    // Với video, giữ nguyên dữ liệu gốc và chỉ định đúng MIME type
    const lower = fileUri.toLowerCase();
    if (lower.endsWith('.mov')) {
      fileType = 'video/quicktime';
    } else if (lower.endsWith('.webm')) {
      fileType = 'video/webm';
    } else if (!fileType.startsWith('video/')) {
      fileType = 'video/mp4';
    }
  }

  if (options?.signal?.aborted) {
    throw new Error('Upload cancelled by user');
  }

  // Bước 2: Chuẩn bị FormData
  const formData = new FormData();
  formData.append('purpose', purpose);

  if (blobData) {
    formData.append('file', blobData, fileName);
  } else if (
    Platform.OS === 'web' &&
    typeof Blob !== 'undefined' &&
    !fileUri.startsWith('file://')
  ) {
    // Chỉ web đọc được tệp thành Blob để gửi. Trên điện thoại, FormData của React Native không nhận Blob (báo
    // 'Unsupported FormDataPart implementation'): phải gửi dạng { uri, type, name } cho phần native tự đọc, kể cả uri content://.
    try {
      const blobRes = await fetch(fileUri);
      const blob = await blobRes.blob();
      formData.append('file', blob, fileName);
    } catch {
      // @ts-ignore
      formData.append('file', {
        uri: fileUri,
        type: fileType,
        name: fileName,
      });
    }
  } else {
    // @ts-ignore
    formData.append('file', {
      uri: fileUri,
      type: fileType,
      name: fileName,
    });
  }

  // Bước 3: Gửi lên Backend (Kênh mobile /api/v1/mob/files)
  const endpoint = `${baseUrl}/api/v1/mob/files`;
  // Gửi bằng axios (XMLHttpRequest của React Native), không dùng `fetch` toàn cục: runtime Expo thay `fetch` bằng bản
  // `expo/fetch`, bản đó không nhận phần tệp dạng { uri, type, name } ('Unsupported FormDataPart implementation'),
  // còn XMLHttpRequest thì đọc tệp theo uri (file:// lẫn content://) ở phía native.
  const send = async (headers: Record<string, string>) => {
    try {
      const response = await axios.post<FileUploadResult>(endpoint, formData, {
        // Trên điện thoại cần khai báo multipart để phần native tự thêm boundary; web để trình duyệt tự đặt.
        headers:
          Platform.OS === 'web' ? headers : { ...headers, 'Content-Type': 'multipart/form-data' },
        signal: options?.signal,
        timeout: UPLOAD_TIMEOUT_MS,
      });
      return { status: response.status, data: response.data as unknown };
    } catch (error) {
      // Server trả mã lỗi thì giữ lại để xử lý (làm mới token khi 401); không có phản hồi (mất mạng...) thì ném tiếp.
      if (isAxiosError(error) && error.response) {
        return { status: error.response.status, data: error.response.data as unknown };
      }
      throw error;
    }
  };

  let result = await send(authHeaders);

  // Access token hết hạn: làm mới một lần rồi gửi lại. Không làm khi người gọi tự truyền `authToken`.
  if (result.status === 401 && !options?.authToken) {
    const refreshed = await authRuntime.transport.refresh().catch(() => null);
    if (refreshed) {
      result = await send({ ...authHeaders, Authorization: `Bearer ${refreshed}` });
    } else {
      await authRuntime.transport.onUnauthorized();
    }
  }

  if (result.status < 200 || result.status >= 300) {
    const errorText = typeof result.data === 'string' ? result.data : JSON.stringify(result.data);
    throw new UploadError(
      `Upload file multipart thất bại (${result.status}): ${errorText}`,
      result.status,
    );
  }

  return result.data as FileUploadResult;
}
