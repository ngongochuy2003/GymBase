/**
 * Nén video trên điện thoại bằng react-native-compressor (module native, cần dev build; Expo Go và Jest không có).
 * Không có module hoặc nén lỗi thì trả `null` để uploader gửi bản gốc, rồi để giới hạn dung lượng quyết định.
 */

/** Lớp mỏng quanh thư viện, để test thay bằng bản giả và đổi thư viện không phải sửa nơi dùng. */
export interface VideoCompressorAdapter {
  /** Nén `uri` và trả URI của tệp mp4 mới. `onProgress` nhận 0 đến 1. */
  compress(uri: string, onProgress?: (progress: number) => void): Promise<string>;
  /** Dung lượng tệp (byte). */
  getFileSize(uri: string): Promise<number>;
}

/** Cạnh dài tối đa của video sau khi nén (px): 1280 là 720p, đủ nét trên điện thoại. */
export const CANH_DAI_VIDEO_TOI_DA_PX = 1280;

/** Video nhỏ hơn mức này (byte) thì gửi nguyên bản, khỏi tốn thời gian nén. */
export const NGUONG_NEN_VIDEO_BYTE = 20 * 1024 * 1024;

let adapter: VideoCompressorAdapter | null | undefined;

function loadDefaultAdapter(): VideoCompressorAdapter | null {
  try {
    // eslint-disable-next-line @typescript-eslint/no-require-imports
    const lib = require('react-native-compressor');
    // Module native thiếu (Expo Go) thì chạm vào `Video` mới báo lỗi; kiểm ngay để biết là không dùng được.
    if (!lib?.Video?.compress || !lib?.getFileSize) return null;
    return {
      compress: (uri, onProgress) =>
        lib.Video.compress(
          uri,
          {
            compressionMethod: 'auto',
            maxSize: CANH_DAI_VIDEO_TOI_DA_PX,
            // Bỏ metadata (vị trí GPS, thời điểm quay) khỏi video gửi lên.
            preserveMetadata: false,
            minimumFileSizeForCompress: 0,
          },
          onProgress,
        ),
      getFileSize: async (uri) => Number(await lib.getFileSize(uri)),
    };
  } catch {
    return null;
  }
}

function getAdapter(): VideoCompressorAdapter | null {
  if (adapter === undefined) adapter = loadDefaultAdapter();
  return adapter;
}

/** Test truyền bản giả; `null` mô phỏng máy không có thư viện (Expo Go). */
export function setCustomVideoCompressor(custom: VideoCompressorAdapter | null) {
  adapter = custom;
}

export function isVideoCompressorAvailable(): boolean {
  return getAdapter() !== null;
}

/** Nén video; `null` khi không có thư viện hoặc nén lỗi (người gọi dùng bản gốc). */
export async function compressVideoOnDevice(
  uri: string,
  onProgress?: (progress: number) => void,
): Promise<{ uri: string; sizeBytes: number } | null> {
  const active = getAdapter();
  if (!active) return null;
  try {
    const compressedUri = await active.compress(uri, onProgress);
    const sizeBytes = await active.getFileSize(compressedUri);
    if (!Number.isFinite(sizeBytes) || sizeBytes <= 0) return null;
    return { uri: compressedUri, sizeBytes };
  } catch {
    return null;
  }
}
