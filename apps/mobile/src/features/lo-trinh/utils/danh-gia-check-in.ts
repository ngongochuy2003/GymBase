import type { DanhGiaCheckIn } from '../data/danh-gia-check-in-store';
import type { ChiTietQuanLy } from '../types';

/** Số check-in của một lộ trình PT đã đánh giá trong phiên (mỗi bản đều từng là "chờ duyệt"). */
export function demDaDanhGia(
  danhGia: Readonly<Record<string, DanhGiaCheckIn>>,
  loTrinhId: string,
): number {
  return Object.values(danhGia).filter((d) => d.loTrinhId === loTrinhId).length;
}

/**
 * Áp các đánh giá vừa gửi lên chi tiết lộ trình: check-in đổi trạng thái (kèm nhận xét nếu có) và "Hoạt động gần đây" thêm
 * một dòng cho mỗi lần đánh giá, mới nhất ở trên.
 */
export function apDanhGia(
  chiTiet: ChiTietQuanLy,
  danhGia: Readonly<Record<string, DanhGiaCheckIn>>,
): ChiTietQuanLy {
  const daDanhGia = chiTiet.checkIn.filter((c) => danhGia[c.id]);
  if (daDanhGia.length === 0) return chiTiet;
  return {
    ...chiTiet,
    checkIn: chiTiet.checkIn.map((c) => {
      const d = danhGia[c.id];
      if (!d) return c;
      return {
        ...c,
        trangThai: d.trangThai,
        nhanXet: d.nhanXet
          ? { pt: chiTiet.pt, noiDung: d.nhanXet, thoiDiem: 'Vừa xong' }
          : undefined,
      };
    }),
    hoatDong: [
      ...daDanhGia.map((c) => ({
        id: `hd-danh-gia-${c.id}`,
        nguoi: chiTiet.pt,
        noiDung: `PT vừa ${
          danhGia[c.id]?.trangThai === 'da_duyet' ? 'duyệt' : 'yêu cầu bổ sung'
        } ${c.tieuDe.toLowerCase()} của ${c.hocVien.ten}`,
        thoiDiem: 'Vừa xong',
      })),
      ...chiTiet.hoatDong,
    ],
  };
}
