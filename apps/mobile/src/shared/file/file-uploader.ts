import axios, { isAxiosError } from 'axios';
import { Platform } from 'react-native';
import { toApiError } from '@/core/api/api-error';
import { getMobClient } from '@/core/api/mobile-client';
import { FilePurpose, FileUploadResult, UploadOptions } from './file.types';

/** Thời gian chờ tối đa cho một lần gửi tệp lên storage (ms); video trên mạng yếu cần lâu hơn các lệnh API thường. */
const UPLOAD_TIMEOUT_MS = 5 * 60_000;
const DEFAULT_MAX_RETRIES = 2;
const DEFAULT_RETRY_DELAY_MS = 800;

/** Upload thất bại; `status` để giao diện nói đúng nguyên nhân (401 hết phiên, 400 tệp không hợp lệ, 413 quá lớn...). */
export class UploadError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly code?: string,
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

/** Form đã ký do backend trả khi xin upload (xem docs/architecture/upload-file.md). */
interface UploadTarget {
  url: string;
  method: 'POST';
  fields: Record<string, string>;
  file_field: string;
  het_han: string;
}

interface XinUploadResponse {
  file_id: string;
  upload: UploadTarget;
}

interface XacNhanUploadResponse {
  file_id: string;
  loai_mime: string;
  kich_thuoc_byte: number;
  muc_dich: string;
  trang_thai: string;
}

/** Tệp đã chuẩn bị để gửi: đã nén (ảnh) hoặc giữ nguyên (video, PDF). */
interface PreparedFile {
  uri: string;
  type: string;
  name: string;
  /** Ước lượng dung lượng (byte); backend chỉ dùng để kiểm sớm, dung lượng thật lấy từ storage lúc xác nhận. */
  sizeBytes: number;
  /** Có khi chạy trên web (`blob:` URL): gửi thẳng Blob thay vì { uri }. */
  blob?: Blob;
}

const sleep = (ms: number) => new Promise<void>((resolve) => setTimeout(resolve, ms));

const throwIfAborted = (signal?: AbortSignal) => {
  if (signal?.aborted) throw new Error('Upload cancelled by user');
};

/** Đổi lỗi của API (xin upload, xác nhận) sang `UploadError`, giữ mã lỗi để giao diện rẽ nhánh. */
function toUploadError(error: unknown): UploadError {
  if (error instanceof UploadError) return error;
  const api = toApiError(error);
  return new UploadError(api.message, api.status ?? 0, api.code);
}

/** Chuẩn bị tệp: phân loại (ảnh, video, PDF), nén ảnh, đọc dung lượng. */
async function prepareFile(
  localUri: string,
  purpose: FilePurpose,
  options?: UploadOptions,
): Promise<PreparedFile> {
  // Chứng chỉ PT là tài liệu PDF: gửi nguyên bản, không nén ảnh và không coi là video.
  const isDocument = purpose === 'chung_chi_pt';
  let isVideo =
    !isDocument && (isVideoUrl(localUri) || Boolean(options?.contentType?.startsWith('video/')));
  let type =
    options?.contentType || (isDocument ? 'application/pdf' : isVideo ? 'video/mp4' : 'image/jpeg');
  let blob: Blob | undefined;
  let sizeBytes = options?.sizeBytes ?? 0;

  // Web (blob: URL): đọc MIME và dung lượng trực tiếp từ Blob.
  if (typeof fetch !== 'undefined' && localUri.startsWith('blob:')) {
    try {
      const response = await fetch(localUri);
      blob = await response.blob();
      if (blob.type) {
        type = blob.type;
        if (blob.type.startsWith('video/')) isVideo = true;
      }
      sizeBytes = blob.size;
    } catch {
      // Dùng giá trị mặc định ở trên.
    }
  }

  let uri = localUri;
  let name =
    options?.fileName ||
    localUri.split('/').pop() ||
    (isDocument
      ? `tai-lieu_${Date.now()}.pdf`
      : isVideo
        ? `video_${Date.now()}.mp4`
        : `upload_${Date.now()}.jpg`);

  if (isDocument) {
    // Giữ nguyên tệp gốc.
  } else if (!isVideo) {
    try {
      // Nén ảnh giữ độ nét cao bằng expo-image-manipulator (cạnh dài 2048px, JPEG 0.9, bỏ EXIF).
      const compressed = await compressImageOnDevice(localUri);
      uri = compressed.uri;
      type = compressed.contentType;
      name = compressed.uri.split('/').pop() || `upload_${Date.now()}.jpg`;
      sizeBytes = compressed.sizeBytes;
      blob = undefined;
    } catch {
      // Không nén được (ảnh đặc biệt): gửi nguyên bản.
      uri = localUri;
    }
  } else {
    // Video: giữ nguyên dữ liệu gốc và chỉ định đúng MIME.
    const lower = uri.toLowerCase();
    if (lower.endsWith('.mov')) {
      type = 'video/quicktime';
    } else if (!type.startsWith('video/')) {
      type = 'video/mp4';
    }
  }

  return { uri, type, name, sizeBytes: Math.max(1, Math.round(sizeBytes)), blob };
}

/** Gửi form đã ký lên storage; trả lỗi có `status` khi storage từ chối. Không gắn Authorization: chữ ký đã nằm trong form. */
async function postForm(target: UploadTarget, file: PreparedFile, options?: UploadOptions) {
  const form = new FormData();
  for (const [name, value] of Object.entries(target.fields)) form.append(name, value);

  // Trường file phải đặt cuối cùng. Trên điện thoại FormData của React Native nhận { uri, type, name } để phần native tự đọc tệp.
  if (file.blob) {
    form.append(target.file_field, file.blob, file.name);
  } else {
    // @ts-expect-error React Native cho phép phần tệp dạng { uri, type, name }.
    form.append(target.file_field, { uri: file.uri, type: file.type, name: file.name });
  }

  try {
    await axios.post(target.url, form, {
      headers: Platform.OS === 'web' ? undefined : { 'Content-Type': 'multipart/form-data' },
      signal: options?.signal,
      timeout: UPLOAD_TIMEOUT_MS,
      onUploadProgress: (event) => {
        if (event.total) options?.onProgress?.(Math.min(99, Math.round((event.loaded / event.total) * 100)));
      },
    });
  } catch (error) {
    if (isAxiosError(error) && error.response) {
      throw new UploadError(
        `Kho lưu trữ từ chối tệp (${error.response.status}).`,
        error.response.status,
        'STORAGE_REJECTED',
      );
    }
    throw error;
  }
}

/**
 * Upload hai bước lên MinIO/S3 (xem docs/architecture/upload-file.md):
 * 1. xin form đã ký (`POST /files/upload-url`), 2. gửi tệp thẳng lên storage, 3. báo xong (`POST /files/{id}/confirm`).
 * Lỗi mạng và 5xx của storage được thử lại có backoff; form hết hạn thì xin form mới. Trả `file_id` để gắn vào bài viết, hồ sơ...
 */
export async function uploadMultipartFile(
  localUri: string,
  purpose: FilePurpose = 'avatar',
  options?: UploadOptions,
): Promise<FileUploadResult> {
  const api = getMobClient();
  throwIfAborted(options?.signal);
  const file = await prepareFile(localUri, purpose, options);
  const maxRetries = options?.maxRetries ?? DEFAULT_MAX_RETRIES;
  const retryDelay = options?.initialRetryDelayMs ?? DEFAULT_RETRY_DELAY_MS;

  const xinUpload = async (): Promise<XinUploadResponse> => {
    try {
      const { data } = await api.post<XinUploadResponse>('/files/upload-url', {
        muc_dich: purpose,
        loai_mime: file.type,
        kich_thuoc_byte: file.sizeBytes,
        ten_tep: file.name,
      });
      return data;
    } catch (error) {
      throw toUploadError(error);
    }
  };

  let upload = await xinUpload();
  for (let attempt = 0; ; attempt++) {
    throwIfAborted(options?.signal);
    // Form quá hạn (ví dụ app nằm nền lâu rồi mới thử lại) thì xin form mới; dòng cũ để job dọn file xử lý.
    if (Date.parse(upload.upload.het_han) <= Date.now()) upload = await xinUpload();
    try {
      await postForm(upload.upload, file, options);
      break;
    } catch (error) {
      if (options?.signal?.aborted) throw error;
      const status = error instanceof UploadError ? error.status : 0;
      const retryable = status === 0 || status >= 500;
      if (!retryable || attempt >= maxRetries) {
        if (error instanceof UploadError) throw error;
        throw new UploadError('Không kết nối được tới máy chủ. Vui lòng kiểm tra mạng rồi thử lại.', 0);
      }
      await sleep(retryDelay * 2 ** attempt);
    }
  }

  throwIfAborted(options?.signal);
  try {
    const { data } = await api.post<XacNhanUploadResponse>(`/files/${upload.file_id}/confirm`);
    options?.onProgress?.(100);
    return {
      file_id: data.file_id,
      original_name: file.name,
      content_type: data.loai_mime,
      size: data.kich_thuoc_byte,
      purpose: data.muc_dich,
    };
  } catch (error) {
    throw toUploadError(error);
  }
}
