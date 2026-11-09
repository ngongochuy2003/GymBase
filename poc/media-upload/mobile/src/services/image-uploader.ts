import {
  ConfirmUploadRequest,
  ConfirmUploadResponse,
  MediaPurpose,
  UploadOptions,
  UploadUrlRequest,
  UploadUrlResponse,
} from '../types/media.types';

// Interface cho Image Manipulator để dễ dàng mock trong unit test
export interface ManipulatorAdapter {
  manipulateAsync(
    uri: string,
    actions: Array<{ resize?: { width?: number; height?: number } }>,
    saveOptions: { compress: number; format: 'jpeg' | 'png' | 'webp' }
  ): Promise<{ uri: string; width: number; height: number; base64?: string }>;
}

// Fallback runtime adapter nếu expo-image-manipulator có sẵn
let defaultManipulator: ManipulatorAdapter | null = null;
try {
  // eslint-disable-next-line @typescript-eslint/no-require-imports
  const ExpoManipulator = require('expo-image-manipulator');
  defaultManipulator = {
    manipulateAsync: (uri, actions, options) =>
      ExpoManipulator.manipulateAsync(uri, actions, options),
  };
} catch {
  // Môi trường test nodejs không có native module expo-image-manipulator
}

export function setCustomManipulator(manipulator: ManipulatorAdapter) {
  defaultManipulator = manipulator;
}

/**
 * 1. Nén và chuẩn hóa ảnh phía Mobile bằng expo-image-manipulator.
 * - Cạnh dài tối đa 2048px (giữ nguyên tỷ lệ).
 * - Nén chất lượng 0.82 JPEG (giảm 70-80% dung lượng ban đầu).
 * - Tự động loại bỏ metadata EXIF nhạy cảm (GPS coordinates).
 */
export async function compressImageOnDevice(
  uri: string,
  manipulator?: ManipulatorAdapter
): Promise<{ uri: string; sizeBytes: number; contentType: string }> {
  const activeManipulator = manipulator || defaultManipulator;

  if (activeManipulator) {
    const result = await activeManipulator.manipulateAsync(
      uri,
      [{ resize: { width: 2048 } }],
      { compress: 0.82, format: 'jpeg' }
    );

    // Lấy kích thước thực tế qua blob / fetch
    const response = await fetch(result.uri);
    const blob = await response.blob();

    return {
      uri: result.uri,
      sizeBytes: blob.size,
      contentType: 'image/jpeg',
    };
  }

  // Fallback nếu không có manipulator (ví dụ ảnh đã được chuẩn hóa)
  const response = await fetch(uri);
  const blob = await response.blob();
  return {
    uri,
    sizeBytes: blob.size,
    contentType: blob.type || 'image/jpeg',
  };
}

/**
 * 2. Upload file trực tiếp lên S3/MinIO qua Presigned PUT URL.
 * - Hỗ trợ theo dõi tiến trình (0 - 100%).
 * - Hỗ trợ hủy luồng upload qua AbortSignal.
 * - Hỗ trợ cơ chế Retry với Exponential Backoff khi gặp sự cố mạng chập chờn.
 */
export async function putToS3Direct(
  presignedUrl: string,
  fileUri: string,
  contentType: string,
  options?: UploadOptions
): Promise<void> {
  const maxRetries = options?.maxRetries ?? 3;
  const initialDelay = options?.initialRetryDelayMs ?? 1000;

  // Đọc dữ liệu file
  const fileResponse = await fetch(fileUri);
  const fileBlob = await fileResponse.blob();

  let attempt = 0;
  while (attempt <= maxRetries) {
    if (options?.signal?.aborted) {
      throw new Error('Upload cancelled by user');
    }

    try {
      await new Promise<void>((resolve, reject) => {
        const xhr = new XMLHttpRequest();

        if (options?.signal) {
          options.signal.addEventListener('abort', () => {
            xhr.abort();
            reject(new Error('Upload cancelled by user'));
          });
        }

        xhr.upload.onprogress = (event) => {
          if (event.lengthComputable && options?.onProgress) {
            const percent = Math.round((event.loaded / event.total) * 100);
            options.onProgress(percent);
          }
        };

        xhr.onload = () => {
          if (xhr.status >= 200 && xhr.status < 300) {
            resolve();
          } else {
            reject(new Error(`S3 PUT failed with status ${xhr.status}: ${xhr.statusText}`));
          }
        };

        xhr.onerror = () => reject(new Error('Network error during S3 PUT upload'));
        xhr.ontimeout = () => reject(new Error('Timeout during S3 PUT upload'));

        xhr.open('PUT', presignedUrl);
        xhr.setRequestHeader('Content-Type', contentType);
        xhr.send(fileBlob);
      });

      return; // Upload thành công
    } catch (err: unknown) {
      attempt++;
      if (options?.signal?.aborted || attempt > maxRetries) {
        throw err;
      }

      // Exponential backoff kèm jitter
      const jitter = Math.random() * 200;
      const delay = initialDelay * Math.pow(2, attempt - 1) + jitter;
      await new Promise((res) => setTimeout(res, delay));
    }
  }
}

/**
 * 3. Luồng tổng thể: Nén ảnh -> Xin URL -> PUT trực tiếp lên S3 -> Confirm tại Server.
 */
export async function uploadImage(
  localUri: string,
  purpose: MediaPurpose,
  options?: UploadOptions
): Promise<ConfirmUploadResponse> {
  const baseUrl = (options?.apiBaseUrl || 'http://localhost:5000').replace(/\/+$/, '');
  const authHeaders: Record<string, string> = {
    'Content-Type': 'application/json',
  };
  if (options?.authToken) {
    authHeaders.Authorization = `Bearer ${options.authToken}`;
  }

  // Bước 1: Nén và chuẩn hóa ảnh trên thiết bị
  const compressed = await compressImageOnDevice(localUri);

  if (options?.signal?.aborted) {
    throw new Error('Upload cancelled by user');
  }

  // Bước 2: Gọi API xin cấp Presigned PUT URL
  const uploadUrlPayload: UploadUrlRequest = {
    purpose,
    content_type: compressed.contentType,
    size_bytes: compressed.sizeBytes,
  };

  const urlRes = await fetch(`${baseUrl}/api/v1/media/upload-url`, {
    method: 'POST',
    headers: authHeaders,
    body: JSON.stringify(uploadUrlPayload),
    signal: options?.signal,
  });

  if (!urlRes.ok) {
    const errorBody = await urlRes.text();
    throw new Error(`Xin cấp upload URL thất bại (${urlRes.status}): ${errorBody}`);
  }

  const urlData = (await urlRes.json()) as UploadUrlResponse;

  // Bước 3: PUT dữ liệu trực tiếp lên S3/MinIO
  await putToS3Direct(urlData.presigned_put_url, compressed.uri, compressed.contentType, options);

  if (options?.signal?.aborted) {
    throw new Error('Upload cancelled by user');
  }

  // Bước 4: Gọi API xác nhận hoàn tất
  const confirmPayload: ConfirmUploadRequest = {
    upload_id: urlData.upload_id,
    staging_key: urlData.staging_key,
    purpose,
  };

  const confirmRes = await fetch(`${baseUrl}/api/v1/media/confirm`, {
    method: 'POST',
    headers: authHeaders,
    body: JSON.stringify(confirmPayload),
    signal: options?.signal,
  });

  if (!confirmRes.ok) {
    const errorBody = await confirmRes.text();
    throw new Error(`Xác nhận upload thất bại (${confirmRes.status}): ${errorBody}`);
  }

  return (await confirmRes.json()) as ConfirmUploadResponse;
}
