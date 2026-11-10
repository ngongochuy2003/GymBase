import { ImageVariantUrls } from './file.types';

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
