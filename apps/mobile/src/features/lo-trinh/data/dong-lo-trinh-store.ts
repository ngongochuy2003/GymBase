import { useSyncExternalStore } from 'react';
import type { TrangThaiLoTrinh } from '../types';

/** Một lần PT đóng lộ trình trước hạn (N28). */
export interface DongLoTrinh {
  trangThai: Extract<TrangThaiLoTrinh, 'hoan_thanh' | 'that_bai'>;
  lyDo: string;
}

const RONG: Readonly<Record<string, DongLoTrinh>> = {};
let daDong = RONG;
const nguoiNghe = new Set<() => void>();

function dangKy(nghe: () => void) {
  nguoiNghe.add(nghe);
  return () => nguoiNghe.delete(nghe);
}

/**
 * Kho tạm trong bộ nhớ cho các lộ trình PT vừa đóng, để màn Lộ trình và chi tiết cập nhật ngay. Mất khi thoát app.
 */
// TODO(ROUTE): thay bằng API đóng lộ trình (lưu lý do, người đóng, thời điểm đóng, thông báo học viên; adr.md, N28, K9);
//   bỏ kho này và invalidate query lộ trình.
export function dongLoTrinh(id: string, dong: DongLoTrinh) {
  daDong = { ...daDong, [id]: dong };
  nguoiNghe.forEach((nghe) => nghe());
}

export function useLoTrinhDaDong(): Readonly<Record<string, DongLoTrinh>> {
  return useSyncExternalStore(dangKy, () => daDong);
}

export function datLaiDongLoTrinh() {
  daDong = RONG;
  nguoiNghe.forEach((nghe) => nghe());
}
