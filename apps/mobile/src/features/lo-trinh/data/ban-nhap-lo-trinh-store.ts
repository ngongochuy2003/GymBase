import { useSyncExternalStore } from 'react';
import type { BuaAnKeHoach, LichTapTheoTuan } from '../types';
import { taoKeHoachMau } from './seed';

interface BanNhap {
  buaAn: BuaAnKeHoach[];
  lichTap: LichTapTheoTuan;
}

/** Giá trị điền sẵn của form tạo lộ trình, để xem đủ giao diện. */
// TODO(ROUTE): form bắt đầu trống hoặc nạp bản nháp của PT (adr.md, N22); bỏ giá trị điền sẵn này.
function macDinh(): BanNhap {
  return taoKeHoachMau();
}

let trangThai: BanNhap = macDinh();
const nguoiNghe = new Set<() => void>();

function capNhat(moi: BanNhap) {
  trangThai = moi;
  nguoiNghe.forEach((nghe) => nghe());
}

function dangKy(nghe: () => void) {
  nguoiNghe.add(nghe);
  return () => nguoiNghe.delete(nghe);
}

/**
 * Bản nháp kế hoạch của form tạo lộ trình (bữa ăn và lịch tập), giữ trong bộ nhớ để màn PT-02 và hai màn con PT-02a,
 * PT-02b cùng đọc và sửa. Quay lại PT-01 không làm mất; tạo lộ trình xong thì đặt lại.
 */
// TODO(ROUTE): thay bằng bản nháp lưu ở backend (adr.md, N22, K8); bỏ kho này.
export function useBanNhapLoTrinh(): BanNhap {
  return useSyncExternalStore(dangKy, () => trangThai);
}

/** Bản nháp hiện tại, đọc ngay lúc gọi (dùng trong xử lý bất đồng bộ, nơi state của màn có thể đã cũ). */
export function layBanNhap(): BanNhap {
  return trangThai;
}

export function datBuaAn(buaAn: BuaAnKeHoach[]) {
  capNhat({ ...trangThai, buaAn });
}

export function datLichTap(lichTap: LichTapTheoTuan) {
  capNhat({ ...trangThai, lichTap });
}

/** Đưa bản nháp về giá trị ban đầu. */
export function datLaiBanNhap() {
  capNhat(macDinh());
}
