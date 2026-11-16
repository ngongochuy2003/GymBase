import { DEFAULT_LOCAL_API_URL } from '@/core/api/mobile-client';
import { env } from '@/core/config/env';
import { ImageVariantUrls } from './file.types';

export function resolveMediaUrl(url?: string | null): string | undefined {
  if (!url) return undefined;
  const trimmed = url.trim();
  if (!trimmed) return undefined;
  if (
    trimmed.startsWith('http://') ||
    trimmed.startsWith('https://') ||
    trimmed.startsWith('data:')
  ) {
    return trimmed;
  }
  const apiHostBase = (env.apiUrl || DEFAULT_LOCAL_API_URL)
    .replace(/\/api\/v1\/mob\/?$/, '')
    .replace(/\/+$/, '');
  return `${apiHostBase}${trimmed.startsWith('/') ? '' : '/'}${trimmed}`;
}

/**
 * Chọn biến thể ảnh phù hợp nhất với chiều rộng hiển thị (display width dp)
 * - displayWidth <= 120dp -> thumb (max 200px)
 * - displayWidth <= 480dp -> medium (max 720px)
 * - displayWidth > 480dp  -> large (max 1440px)
 */
export function selectImageVariant(urls: ImageVariantUrls, displayWidth: number = 360): string {
  if (!urls) return '';

  if (displayWidth <= 120 && urls.thumb) {
    return urls.thumb;
  }

  if (displayWidth <= 480 && urls.medium) {
    return urls.medium;
  }

  return urls.large || urls.medium || urls.thumb || '';
}
