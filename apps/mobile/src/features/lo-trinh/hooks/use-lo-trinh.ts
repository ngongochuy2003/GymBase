import { useMemo } from 'react';
import {
  useAnhCoTheDaGhi,
  useCanNangDaGhi,
  useCheckInBuoiTapDaGui,
  useCheckInDaGui,
} from '../data/check-in-store';
import { taoDuLieuMau, taoTienDoMau } from '../data/seed';
import type {
  CheckInBuaAn,
  CheckInBuoiTap,
  GhiAnhCoThe,
  GhiCanNang,
  LoTrinhCuaToi,
  NhiemVuHomNay,
  TrangThaiNhiemVu,
} from '../types';

/** Áp check-in bữa ăn vừa gửi lên nhiệm vụ: Đã ăn thành "hoàn thành", Bỏ bữa thành "không hoàn thành". */
function apCheckInBuaAn(nhiemVu: NhiemVuHomNay, checkIn: CheckInBuaAn | undefined): NhiemVuHomNay {
  if (!checkIn || !nhiemVu.buaAn) return nhiemVu;
  return {
    ...nhiemVu,
    trangThai: checkIn.trangThaiBua === 'da_an' ? 'da_check' : 'khong_hoan_thanh',
    buaAn: { ...nhiemVu.buaAn, checkIn },
  };
}

/**
 * Áp check-in buổi tập vừa gửi lên nhiệm vụ: Nghỉ tập là "không hoàn thành"; Đã tập và Tập một phần đều đã gửi check-in
 * nên tính là "đã check" (tập một phần không phải thất bại).
 */
function apCheckInBuoiTap(
  nhiemVu: NhiemVuHomNay,
  checkIn: CheckInBuoiTap | undefined,
): NhiemVuHomNay {
  if (!checkIn || !nhiemVu.buoiTap) return nhiemVu;
  const trangThai: TrangThaiNhiemVu =
    checkIn.trangThaiBuoi === 'nghi_tap' ? 'khong_hoan_thanh' : 'da_check';
  return { ...nhiemVu, trangThai, buoiTap: { ...nhiemVu.buoiTap, checkIn } };
}

/** Áp cân nặng vừa ghi lên nhiệm vụ cân nặng: ghi xong là "đã check" (không qua PT duyệt, N16). */
function apCanNang(nhiemVu: NhiemVuHomNay, ghi: GhiCanNang | undefined): NhiemVuHomNay {
  if (!ghi || nhiemVu.loai !== 'can_nang') return nhiemVu;
  return { ...nhiemVu, trangThai: 'da_check', canNangGhi: ghi };
}

/** Áp ảnh cơ thể vừa ghi lên nhiệm vụ ảnh cơ thể: ghi xong là "đã check" (không qua PT duyệt, N16). */
function apAnhCoThe(nhiemVu: NhiemVuHomNay, ghi: GhiAnhCoThe | undefined): NhiemVuHomNay {
  if (!ghi || nhiemVu.loai !== 'anh_co_the') return nhiemVu;
  return { ...nhiemVu, trangThai: 'da_check', anhCoTheGhi: ghi };
}

/**
 * Nguồn dữ liệu của màn Lộ trình. Hiện trả dữ liệu mẫu đồng bộ (kèm check-in vừa gửi trong phiên); giữ hình dạng của một
 * query để sau đổi sang API mà màn hình không phải sửa.
 */
// TODO(ROUTE): chuyển sang useQuery gọi API lộ trình; thêm isPending, isError, phân trang theo bộ lọc (trạng thái, tháng).
export function useLoTrinh() {
  const daGuiBuaAn = useCheckInDaGui();
  const daGuiBuoiTap = useCheckInBuoiTapDaGui();
  const canNangDaGhi = useCanNangDaGhi();
  const anhDaGhi = useAnhCoTheDaGhi();
  const goc = useMemo(() => {
    const homNay = new Date();
    return { homNay, ...taoDuLieuMau(homNay) };
  }, []);
  return useMemo(
    () => ({
      ...goc,
      cuaToi: goc.cuaToi.map((lo): LoTrinhCuaToi => ({
        ...lo,
        nhiemVuHomNay: lo.nhiemVuHomNay.map((nv) =>
          apAnhCoThe(
            apCanNang(
              apCheckInBuoiTap(apCheckInBuaAn(nv, daGuiBuaAn[nv.id]), daGuiBuoiTap[nv.id]),
              canNangDaGhi[nv.id],
            ),
            anhDaGhi[nv.id],
          ),
        ),
      })),
    }),
    [goc, daGuiBuaAn, daGuiBuoiTap, canNangDaGhi, anhDaGhi],
  );
}

/** Chi tiết một lộ trình của học viên kèm số liệu tiến độ; trả `null` khi không có lộ trình mang id đó. */
// TODO(ROUTE): chuyển sang useQuery gọi API chi tiết lộ trình theo id; thêm isPending, isError.
export function useChiTietLoTrinh(id: string | undefined) {
  const { homNay, cuaToi } = useLoTrinh();
  return useMemo(() => {
    const loTrinh = cuaToi.find((lo) => lo.id === id);
    return loTrinh ? { homNay, loTrinh, tienDo: taoTienDoMau(loTrinh) } : null;
  }, [homNay, cuaToi, id]);
}

/** Nhiệm vụ bữa ăn của một lộ trình (kế hoạch và check-in nếu có); `null` khi không tìm thấy hoặc không phải bữa ăn. */
// TODO(ROUTE): chuyển sang useQuery gọi API chi tiết nhiệm vụ (kế hoạch của đúng ngày, N31) theo id.
export function useNhiemVuBuaAn(loTrinhId: string | undefined, nhiemVuId: string | undefined) {
  const { cuaToi } = useLoTrinh();
  return useMemo(() => {
    const loTrinh = cuaToi.find((lo) => lo.id === loTrinhId);
    const nhiemVu = loTrinh?.nhiemVuHomNay.find((nv) => nv.id === nhiemVuId);
    return loTrinh && nhiemVu?.buaAn ? { loTrinh, nhiemVu, buaAn: nhiemVu.buaAn } : null;
  }, [cuaToi, loTrinhId, nhiemVuId]);
}

/** Nhiệm vụ buổi tập của một lộ trình (kế hoạch và check-in nếu có); `null` khi không tìm thấy hoặc không phải buổi tập. */
// TODO(ROUTE): chuyển sang useQuery gọi API chi tiết nhiệm vụ (bài tập của đúng ngày, N31) theo id.
export function useNhiemVuBuoiTap(loTrinhId: string | undefined, nhiemVuId: string | undefined) {
  const { cuaToi } = useLoTrinh();
  return useMemo(() => {
    const loTrinh = cuaToi.find((lo) => lo.id === loTrinhId);
    const nhiemVu = loTrinh?.nhiemVuHomNay.find((nv) => nv.id === nhiemVuId);
    return loTrinh && nhiemVu?.buoiTap ? { loTrinh, nhiemVu, buoiTap: nhiemVu.buoiTap } : null;
  }, [cuaToi, loTrinhId, nhiemVuId]);
}

/**
 * Nhiệm vụ cân nặng của một lộ trình: cân nặng đã ghi (nếu có), lần cân gần nhất trước đó và mục tiêu để màn ghi so sánh.
 * `null` khi không tìm thấy hoặc không phải nhiệm vụ cân nặng.
 */
// TODO(ROUTE): chuyển sang useQuery gọi API (chi_so_co_the gần nhất của lộ trình, K7) theo id nhiệm vụ.
export function useNhiemVuCanNang(loTrinhId: string | undefined, nhiemVuId: string | undefined) {
  const { cuaToi } = useLoTrinh();
  return useMemo(() => {
    const loTrinh = cuaToi.find((lo) => lo.id === loTrinhId);
    const nhiemVu = loTrinh?.nhiemVuHomNay.find((nv) => nv.id === nhiemVuId);
    if (!loTrinh || nhiemVu?.loai !== 'can_nang') return null;
    return {
      loTrinh,
      nhiemVu,
      ghi: nhiemVu.canNangGhi,
      lanTruoc: taoTienDoMau(loTrinh).canNang.at(-1),
    };
  }, [cuaToi, loTrinhId, nhiemVuId]);
}

/**
 * Nhiệm vụ ảnh cơ thể của một lộ trình: ảnh đã ghi (nếu có) và ảnh mốc trước để so sánh. `null` khi không tìm thấy hoặc
 * không phải nhiệm vụ ảnh cơ thể.
 */
// TODO(ROUTE): chuyển sang useQuery gọi API (ảnh mốc gần nhất của lộ trình, K7) theo id nhiệm vụ.
export function useNhiemVuAnhCoThe(loTrinhId: string | undefined, nhiemVuId: string | undefined) {
  const { cuaToi } = useLoTrinh();
  return useMemo(() => {
    const loTrinh = cuaToi.find((lo) => lo.id === loTrinhId);
    const nhiemVu = loTrinh?.nhiemVuHomNay.find((nv) => nv.id === nhiemVuId);
    if (!loTrinh || nhiemVu?.loai !== 'anh_co_the') return null;
    return {
      loTrinh,
      nhiemVu,
      ghi: nhiemVu.anhCoTheGhi,
      anhMocTruoc: taoTienDoMau(loTrinh).anhMoc[0],
    };
  }, [cuaToi, loTrinhId, nhiemVuId]);
}
