import { Platform } from 'react-native';
import { env } from './env';

/** Địa chỉ API khi chạy web trên localhost (dev). */
const LOCAL_WEB_ORIGIN = 'http://localhost:5020';

/** Tiền tố của URI đã đầy đủ: giữ nguyên, không ghép với địa chỉ API. */
const ABSOLUTE_URI_PREFIXES = ['http://', 'https://', 'data:', 'file://', 'blob:'];

/**
 * Địa chỉ gốc của backend (không có `/api/v1/mob`, không có dấu `/` cuối), dùng chung cho API client, upload file và
 * dựng URL ảnh. `override` chỉ để test hoặc ngữ cảnh đặc biệt.
 */
export function resolveApiOrigin(override?: string): string {
  if (
    !override &&
    Platform.OS === 'web' &&
    typeof window !== 'undefined' &&
    window.location?.hostname === 'localhost'
  ) {
    return LOCAL_WEB_ORIGIN;
  }
  return (override || env.apiUrl).trim().replace(/\/api\/v1\/mob\/?$/, '').replace(/\/+$/, '');
}

/**
 * Biến URL tương đối của backend (`/api/v1/mob/files/...`) thành URL đầy đủ để thẻ Image hiển thị được. URL đã đầy đủ
 * (http, https, data, file, blob) giữ nguyên; rỗng thì trả `undefined`.
 */
export function resolveMediaUrl(url?: string | null): string | undefined {
  const trimmed = url?.trim();
  if (!trimmed) return undefined;
  if (ABSOLUTE_URI_PREFIXES.some((prefix) => trimmed.startsWith(prefix))) return trimmed;
  return `${resolveApiOrigin()}${trimmed.startsWith('/') ? '' : '/'}${trimmed}`;
}
