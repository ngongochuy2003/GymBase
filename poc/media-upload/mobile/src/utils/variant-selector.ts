import { ImageVariantUrls } from '../types/media.types';

/**
 * Helper lựa chọn URL biến thể ảnh tối ưu nhất dựa trên kích thước khung hiển thị thực tế trên Mobile.
 * Giúp tiết kiệm băng thông mạng 4G/5G và tăng tốc độ tải ảnh.
 *
 * @param urls Đối tượng chứa các URL { thumb, medium, large }
 * @param displayWidth Chiều rộng hiển thị (logical pixels / dp)
 */
export function selectImageVariant(urls: ImageVariantUrls, displayWidth: number = 360): string {
  if (!urls) return '';

  // Avatar, thumbnail icon nhỏ <= 200px
  if (displayWidth <= 200) {
    return urls.thumb || urls.medium || urls.large;
  }

  // Card bài tập, feed post, ảnh tiến độ trong list thông thường <= 720px
  if (displayWidth <= 720) {
    return urls.medium || urls.large || urls.thumb;
  }

  // Màn hình chi tiết phóng to, zoom ảnh >= 720px
  return urls.large || urls.medium || urls.thumb;
}
