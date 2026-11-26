import { useSyncExternalStore } from 'react';
import type { CheckInQuanLy } from '../types';

/** Một lần PT đánh giá check-in: duyệt hoặc yêu cầu bổ sung, kèm nhận xét (có thể trống khi duyệt). */
export interface DanhGiaCheckIn {
  loTrinhId: string;
  trangThai: 'da_duyet' | 'can_bo_sung';
  nhanXet: string;
}

/** Kết quả gửi đánh giá: đã ghi, hoặc bị chặn vì học viên vừa sửa (N13), hoặc check-in đã được đánh giá rồi. */
export type KetQuaGuiDanhGia = 'ok' | 'da_doi' | 'da_xu_ly';

interface TrangThaiKho {
  danhGia: Readonly<Record<string, DanhGiaCheckIn>>;
  /** Phiên bản mới của check-in sau khi học viên sửa, theo id check-in; không có thì dùng phiên bản gốc. */
  phienBanMoi: Readonly<Record<string, number>>;
  thongBao: string | null;
}

const RONG: TrangThaiKho = { danhGia: {}, phienBanMoi: {}, thongBao: null };
let trangThai = RONG;
const nguoiNghe = new Set<() => void>();

function capNhat(moi: TrangThaiKho) {
  trangThai = moi;
  nguoiNghe.forEach((nghe) => nghe());
}

function dangKy(nghe: () => void) {
  nguoiNghe.add(nghe);
  return () => nguoiNghe.delete(nghe);
}

/** Phiên bản hiện tại của một check-in (tăng mỗi lần học viên sửa). */
export function phienBanHienTai(checkIn: Pick<CheckInQuanLy, 'id' | 'phienBan'>): number {
  return trangThai.phienBanMoi[checkIn.id] ?? checkIn.phienBan;
}

/**
 * Gửi đánh giá của PT. `phienBanDaXem` là phiên bản PT đã mở xem: nếu học viên đã sửa sau đó thì không ghi, trả `da_doi` để
 * màn báo PT xem lại (N13). Check-in đã có đánh giá thì trả `da_xu_ly` (tránh gửi hai lần).
 *
 * Kho tạm trong bộ nhớ, mất khi thoát app.
 */
// TODO(ROUTE): thay bằng API đánh giá check-in (nhat_ky_hoat_dong.nhan_xet, ngay_danh_gia, kiểm phiên bản, thông báo học viên;
//   adr.md, N13, N19) dùng useMutation; bỏ kho này và invalidate query lộ trình.
export function guiDanhGia(
  checkIn: Pick<CheckInQuanLy, 'id' | 'phienBan'>,
  danhGia: DanhGiaCheckIn,
  phienBanDaXem: number,
): KetQuaGuiDanhGia {
  if (trangThai.danhGia[checkIn.id]) return 'da_xu_ly';
  if (phienBanDaXem !== phienBanHienTai(checkIn)) return 'da_doi';
  capNhat({
    ...trangThai,
    danhGia: { ...trangThai.danhGia, [checkIn.id]: danhGia },
    thongBao: 'Đã gửi đánh giá cho học viên',
  });
  return 'ok';
}

/** Giả lập học viên vừa sửa check-in (để thử phát hiện phiên bản cũ); thật ra do backend tăng phiên bản. */
// TODO(ROUTE): bỏ khi backend trả phiên bản của check-in.
export function datPhienBanCheckIn(id: string, phienBan: number) {
  capNhat({ ...trangThai, phienBanMoi: { ...trangThai.phienBanMoi, [id]: phienBan } });
}

export function xoaThongBaoDanhGia() {
  capNhat({ ...trangThai, thongBao: null });
}

export function datLaiDanhGiaCheckIn() {
  capNhat(RONG);
}

export function useDanhGiaCheckIn(): Readonly<Record<string, DanhGiaCheckIn>> {
  return useSyncExternalStore(dangKy, () => trangThai.danhGia);
}

export function useThongBaoDanhGia(): string | null {
  return useSyncExternalStore(dangKy, () => trangThai.thongBao);
}
