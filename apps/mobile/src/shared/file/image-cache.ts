import { Image } from 'expo-image';

/** Thông tin đi kèm một ảnh do API trả (xem `hinh_anh_meta` của bài viết). */
export interface ImageMeta {
  /** Id tệp: app cache ảnh theo id vì URL ký đổi mỗi lần. */
  fileId?: string;
  /** Chuỗi blurhash để vẽ bản mờ trong lúc ảnh tải. */
  blurhash?: string;
}

/** Tối đa số URL nhớ cùng lúc; vượt thì bỏ mục cũ nhất. Đủ cho vài chục trang bảng tin. */
const MAX_ENTRIES = 1000;

const registry = new Map<string, ImageMeta>();

/**
 * Nhớ thông tin của một URL ký để `RemoteImage` tự lấy được mà màn hình không phải truyền xuống từng ảnh.
 * Khóa là URL đã ghép host (đúng chuỗi mà `RemoteImage` sẽ tải).
 */
export function ghiNhoThongTinAnh(url: string, meta: ImageMeta): void {
  registry.delete(url);
  registry.set(url, meta);
  if (registry.size > MAX_ENTRIES) {
    const oldest = registry.keys().next().value;
    if (oldest !== undefined) registry.delete(oldest);
  }
}

export function layThongTinAnh(url: string): ImageMeta | undefined {
  return registry.get(url);
}

/**
 * Xóa cache ảnh trong bộ nhớ và trên đĩa, cùng thông tin đã nhớ. Gọi khi đăng xuất để máy dùng chung không còn ảnh riêng tư
 * của người trước. Lỗi (ví dụ môi trường không có module native) được bỏ qua: đăng xuất không được vì thế mà hỏng.
 */
export async function xoaCacheAnh(): Promise<void> {
  registry.clear();
  try {
    await Promise.all([Image.clearMemoryCache(), Image.clearDiskCache()]);
  } catch {
    // Bỏ qua.
  }
}
