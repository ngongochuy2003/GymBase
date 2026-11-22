import type { MocNhiemVuNhom, NhiemVuNhom, ThanhVienLoTrinh } from '../types';

/** Trạng thái của một nhiệm vụ chung: đã xong cả nhóm, hoặc mốc giờ của nó. */
export type TrangThaiNhiemVuNhom = 'da_hoan_thanh' | MocNhiemVuNhom;

/** Thành viên đang tham gia: chỉ những người này được tính khi xét một nhiệm vụ đã xong chưa (đã chấp nhận lời mời, chưa rời). */
export function thanhVienDangThamGia(thanhVien: readonly ThanhVienLoTrinh[]): ThanhVienLoTrinh[] {
  return thanhVien.filter((t) => t.thamGia === 'tham_gia');
}

/**
 * Trạng thái của nhiệm vụ chung: "đã hoàn thành" khi mọi thành viên đang tham gia đã check-in (lộ trình đơn là một người);
 * chưa thì theo mốc giờ của nhiệm vụ. Thành viên đã rời hoặc chưa chấp nhận lời mời không chặn việc hoàn thành.
 */
export function trangThaiNhiemVuNhom(
  nhiemVu: NhiemVuNhom,
  thanhVien: readonly ThanhVienLoTrinh[],
): TrangThaiNhiemVuNhom {
  const dang = thanhVienDangThamGia(thanhVien);
  const xongHet = dang.length > 0 && dang.every((t) => nhiemVu.daCheckIn.includes(t.id));
  return xongHet ? 'da_hoan_thanh' : nhiemVu.moc;
}

/** Dòng phụ dưới tên nhiệm vụ: chưa ai check-in thì là mô tả, nhóm đã có người thì "n/m đã check-in". */
export function moTaTienDoNhiemVu(
  nhiemVu: NhiemVuNhom,
  thanhVien: readonly ThanhVienLoTrinh[],
  laNhom: boolean,
): string {
  const dang = thanhVienDangThamGia(thanhVien);
  const xong = dang.filter((t) => nhiemVu.daCheckIn.includes(t.id)).length;
  if (dang.length > 0 && xong === dang.length)
    return laNhom ? 'Cả nhóm đã check-in' : 'Đã check-in';
  if (laNhom && xong > 0) return `${xong}/${dang.length} đã check-in`;
  return nhiemVu.moTa;
}
