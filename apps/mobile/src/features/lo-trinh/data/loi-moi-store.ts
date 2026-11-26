import { useSyncExternalStore } from 'react';

/** Câu trả lời của học viên cho một lời mời (N23). */
export type TraLoiLoiMoi = 'tham_gia' | 'tu_choi';

const RONG: Readonly<Record<string, TraLoiLoiMoi>> = {};
let daTraLoi = RONG;
const nguoiNghe = new Set<() => void>();

function dangKy(nghe: () => void) {
  nguoiNghe.add(nghe);
  return () => nguoiNghe.delete(nghe);
}

/**
 * Kho tạm trong bộ nhớ cho các lời mời học viên vừa trả lời, để màn Lộ trình cập nhật ngay. Mất khi thoát app.
 */
// TODO(ROUTE): thay bằng API tham gia, từ chối lời mời (lưu trạng thái, thời điểm; thông báo PT; adr.md, N23, K9);
//   bỏ kho này và invalidate query lộ trình, lời mời.
export function traLoiLoiMoi(id: string, traLoi: TraLoiLoiMoi) {
  daTraLoi = { ...daTraLoi, [id]: traLoi };
  nguoiNghe.forEach((nghe) => nghe());
}

export function useLoiMoiDaTraLoi(): Readonly<Record<string, TraLoiLoiMoi>> {
  return useSyncExternalStore(dangKy, () => daTraLoi);
}

export function datLaiLoiMoi() {
  daTraLoi = RONG;
  nguoiNghe.forEach((nghe) => nghe());
}
