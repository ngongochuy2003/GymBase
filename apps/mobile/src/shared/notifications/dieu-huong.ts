import type { Href } from 'expo-router';
import type { ThongBaoDto } from './notification.types';

/**
 * Màn hình mở khi người dùng bấm vào một thông báo.
 */
// TODO(THONGBAO): tạm mở màn lộ trình cho mọi loại; khi có màn tin nhắn, kết nối PT, đánh giá thì rẽ theo
//   `loai_doi_tuong` và `doi_tuong_id`.
export function duongDanCuaThongBao(_thongBao?: Pick<ThongBaoDto, 'loai_doi_tuong' | 'doi_tuong_id'>): Href {
  return '/route';
}
