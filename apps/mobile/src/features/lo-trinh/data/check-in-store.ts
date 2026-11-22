import { useSyncExternalStore } from 'react';
import type { CheckInBuaAn, CheckInBuoiTap, GhiAnhCoThe, GhiCanNang } from '../types';

interface TrangThaiKho {
  /** Check-in bữa ăn đã gửi trong phiên này, theo id nhiệm vụ. Ghi đè lên dữ liệu mẫu. */
  daGui: Readonly<Record<string, CheckInBuaAn>>;
  /** Check-in buổi tập đã gửi trong phiên này, theo id nhiệm vụ. */
  daGuiBuoiTap: Readonly<Record<string, CheckInBuoiTap>>;
  /** Cân nặng đã ghi trong phiên này, theo id nhiệm vụ (ghi trực tiếp, không qua PT duyệt, N16). */
  canNang: Readonly<Record<string, GhiCanNang>>;
  /** Ảnh cơ thể đã ghi trong phiên này, theo id nhiệm vụ. */
  anhCoThe: Readonly<Record<string, GhiAnhCoThe>>;
  /** Thông báo ngắn chờ màn đang hiển thị đọc (toast), `null` khi không có. */
  thongBao: string | null;
}

const RONG: TrangThaiKho = {
  daGui: {},
  daGuiBuoiTap: {},
  canNang: {},
  anhCoThe: {},
  thongBao: null,
};
let trangThai: TrangThaiKho = RONG;
const nguoiNghe = new Set<() => void>();

function capNhat(moi: TrangThaiKho) {
  trangThai = moi;
  nguoiNghe.forEach((nghe) => nghe());
}

function dangKy(nghe: () => void) {
  nguoiNghe.add(nghe);
  return () => nguoiNghe.delete(nghe);
}

/** Thêm hoặc thay một bản ghi vào một trong các nhóm của kho, rồi đặt thông báo cho toast. */
function ghi<K extends 'daGui' | 'daGuiBuoiTap' | 'canNang' | 'anhCoThe'>(
  nhom: K,
  nhiemVuId: string,
  gia: TrangThaiKho[K][string],
  thongBao: string,
) {
  capNhat({ ...trangThai, [nhom]: { ...trangThai[nhom], [nhiemVuId]: gia }, thongBao });
}

/**
 * Kho tạm trong bộ nhớ cho check-in và số đo vừa ghi, để danh sách nhiệm vụ và màn chi tiết cập nhật ngay. Mất khi thoát
 * app.
 */
// TODO(ROUTE): thay bằng API gửi check-in bữa ăn, buổi tập (kèm upload ảnh) và ghi cân nặng, ảnh cơ thể (chi_so_co_the, K7)
//   dùng useMutation; bỏ kho này và invalidate query lộ trình.
export function luuCheckIn(nhiemVuId: string, checkIn: CheckInBuaAn, thongBao: string) {
  ghi('daGui', nhiemVuId, checkIn, thongBao);
}

export function luuCheckInBuoiTap(nhiemVuId: string, checkIn: CheckInBuoiTap, thongBao: string) {
  ghi('daGuiBuoiTap', nhiemVuId, checkIn, thongBao);
}

export function luuCanNang(nhiemVuId: string, canNang: GhiCanNang, thongBao: string) {
  ghi('canNang', nhiemVuId, canNang, thongBao);
}

export function luuAnhCoThe(nhiemVuId: string, anh: GhiAnhCoThe, thongBao: string) {
  ghi('anhCoThe', nhiemVuId, anh, thongBao);
}

export function xoaThongBao() {
  if (trangThai.thongBao !== null) capNhat({ ...trangThai, thongBao: null });
}

/** Đưa kho về rỗng, dùng giữa các test. */
export function datLaiKhoCheckIn() {
  capNhat(RONG);
}

export function useCheckInDaGui() {
  return useSyncExternalStore(
    dangKy,
    () => trangThai.daGui,
    () => trangThai.daGui,
  );
}

export function useCheckInBuoiTapDaGui() {
  return useSyncExternalStore(
    dangKy,
    () => trangThai.daGuiBuoiTap,
    () => trangThai.daGuiBuoiTap,
  );
}

export function useCanNangDaGhi() {
  return useSyncExternalStore(
    dangKy,
    () => trangThai.canNang,
    () => trangThai.canNang,
  );
}

export function useAnhCoTheDaGhi() {
  return useSyncExternalStore(
    dangKy,
    () => trangThai.anhCoThe,
    () => trangThai.anhCoThe,
  );
}

export function useThongBaoCheckIn() {
  return useSyncExternalStore(
    dangKy,
    () => trangThai.thongBao,
    () => trangThai.thongBao,
  );
}

/** Check-in bữa ăn đã gửi của một nhiệm vụ trong phiên này (không phải hook, dùng ngoài giao diện hoặc trong test). */
export function layCheckIn(nhiemVuId: string): CheckInBuaAn | undefined {
  return trangThai.daGui[nhiemVuId];
}

/** Check-in buổi tập đã gửi của một nhiệm vụ trong phiên này. */
export function layCheckInBuoiTap(nhiemVuId: string): CheckInBuoiTap | undefined {
  return trangThai.daGuiBuoiTap[nhiemVuId];
}

/** Cân nặng đã ghi của một nhiệm vụ trong phiên này. */
export function layCanNang(nhiemVuId: string): GhiCanNang | undefined {
  return trangThai.canNang[nhiemVuId];
}

/** Ảnh cơ thể đã ghi của một nhiệm vụ trong phiên này. */
export function layAnhCoThe(nhiemVuId: string): GhiAnhCoThe | undefined {
  return trangThai.anhCoThe[nhiemVuId];
}
