import { useMemo } from 'react';
import {
  useAnhCoTheDaGhi,
  useCanNangDaGhi,
  useCheckInBuoiTapDaGui,
  useCheckInDaGui,
} from '../data/check-in-store';
import { useDanhGiaCheckIn } from '../data/danh-gia-check-in-store';
import { useLoTrinhDaDong } from '../data/dong-lo-trinh-store';
import { taoLichSuMau } from '../data/lich-su-check-in';
import { useLoiMoiDaTraLoi } from '../data/loi-moi-store';
import { taoChiTietQuanLyMau, taoDuLieuMau, taoLoiMoiMau, taoTienDoMau } from '../data/seed';
import { apDanhGia, demDaDanhGia } from '../utils/danh-gia-check-in';
import type {
  CheckInBuaAn,
  CheckInBuoiTap,
  CheckInQuanLy,
  ChiTietQuanLy,
  GhiAnhCoThe,
  GhiCanNang,
  LoTrinhCuaToi,
  LoTrinhQuanLy,
  NhiemVuHomNay,
  TienDoLoTrinh,
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
  const daDong = useLoTrinhDaDong();
  const danhGia = useDanhGiaCheckIn();
  const traLoi = useLoiMoiDaTraLoi();
  const goc = useMemo(() => {
    const homNay = new Date();
    return { homNay, ...taoDuLieuMau(homNay), moi: taoLoiMoiMau(homNay) };
  }, []);
  return useMemo(
    () => ({
      ...goc,
      loiMoi: goc.moi.filter((m) => !traLoi[m.loTrinh.id]),
      // Lộ trình vừa tham gia nối vào cuối, để khối "Nhiệm vụ hôm nay" vẫn theo lộ trình đang chạy từ trước.
      cuaToi: [
        ...goc.cuaToi,
        ...goc.moi.filter((m) => traLoi[m.loTrinh.id] === 'tham_gia').map((m) => m.loTrinh),
      ].map((lo): LoTrinhCuaToi => ({
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
      // Lộ trình PT vừa đóng trong phiên: đổi trạng thái và ghi lý do (N28).
      quanLy: goc.quanLy.map((lo) => {
        const dong = daDong[lo.id];
        // Check-in PT vừa đánh giá không còn chờ duyệt nữa (số trên thẻ giảm theo).
        const choDuyet = Math.max(0, lo.soCheckInChoDuyet - demDaDanhGia(danhGia, lo.id));
        const capNhat = { ...lo, soCheckInChoDuyet: choDuyet };
        return dong ? { ...capNhat, trangThai: dong.trangThai, lyDoDong: dong.lyDo } : capNhat;
      }),
    }),
    [goc, daGuiBuaAn, daGuiBuoiTap, canNangDaGhi, anhDaGhi, daDong, danhGia, traLoi],
  );
}

/** Một lời mời kèm câu trả lời trong phiên (nếu đã trả lời); `null` khi không có lời mời mang id đó. */
// TODO(ROUTE): chuyển sang useQuery gọi API chi tiết lời mời theo id; thêm isPending, isError.
export function useLoiMoi(id: string | undefined) {
  const { moi } = useLoTrinh();
  const traLoi = useLoiMoiDaTraLoi();
  const loiMoi = moi.find((m) => m.loTrinh.id === id);
  return loiMoi ? { loiMoi, traLoi: traLoi[loiMoi.loTrinh.id] } : null;
}

const CHUA_CO_TIEN_DO: TienDoLoTrinh = {
  canNang: [],
  buoiTap: { xong: 0, tong: 0 },
  buaAn: { xong: 0, tong: 0 },
  anhMoc: [],
};

/** Chi tiết một lộ trình của học viên kèm số liệu tiến độ; trả `null` khi không có lộ trình mang id đó. */
// TODO(ROUTE): chuyển sang useQuery gọi API chi tiết lộ trình theo id; thêm isPending, isError.
export function useChiTietLoTrinh(id: string | undefined) {
  const { homNay, cuaToi, moi } = useLoTrinh();
  return useMemo(() => {
    const loTrinh = cuaToi.find((lo) => lo.id === id);
    if (!loTrinh) return null;
    // Lộ trình vừa tham gia từ lời mời chưa có số đo nào, không sinh số liệu mẫu cho nó.
    const vuaThamGia = moi.some((m) => m.loTrinh.id === id);
    return { homNay, loTrinh, tienDo: vuaThamGia ? CHUA_CO_TIEN_DO : taoTienDoMau(loTrinh) };
  }, [homNay, cuaToi, moi, id]);
}

/** Chi tiết góc nhìn PT của một thẻ lộ trình, đã áp các đánh giá vừa gửi. */
function chiTietCua(
  lo: LoTrinhQuanLy,
  homNay: Date,
  danhGia: ReturnType<typeof useDanhGiaCheckIn>,
): ChiTietQuanLy {
  // Dữ liệu mẫu sinh danh sách check-in từ số bản chờ duyệt gốc; trả lại số gốc rồi mới áp đánh giá, để id không đổi.
  const goc = { ...lo, soCheckInChoDuyet: lo.soCheckInChoDuyet + demDaDanhGia(danhGia, lo.id) };
  return { ...apDanhGia(taoChiTietQuanLyMau(goc, homNay), danhGia), loTrinh: lo };
}

/** Một check-in đang chờ duyệt kèm lộ trình của nó (PT-05). */
export interface CheckInChoDuyet {
  loTrinhId: string;
  tenLoTrinh: string;
  checkIn: CheckInQuanLy;
}

/** Mọi check-in chờ duyệt của các lộ trình PT đang chạy; có `loTrinhId` thì chỉ lộ trình đó (N43). */
// TODO(ROUTE): chuyển sang useQuery gọi API check-in chờ duyệt của PT (phân trang; adr.md, N43).
export function useCheckInChoDuyet(loTrinhId?: string): CheckInChoDuyet[] {
  const { homNay, quanLy } = useLoTrinh();
  const danhGia = useDanhGiaCheckIn();
  return useMemo(
    () =>
      quanLy
        .filter((lo) => lo.trangThai === 'dang_hoat_dong' && (!loTrinhId || lo.id === loTrinhId))
        .flatMap((lo) =>
          chiTietCua(lo, homNay, danhGia)
            .checkIn.filter((c) => c.trangThai === 'cho_duyet')
            .map((checkIn) => ({ loTrinhId: lo.id, tenLoTrinh: lo.ten, checkIn })),
        ),
    [quanLy, homNay, danhGia, loTrinhId],
  );
}

/** Chi tiết một lộ trình ở góc nhìn PT (PT-03); trả `null` khi không có lộ trình mang id đó. */
// TODO(ROUTE): chuyển sang useQuery gọi API chi tiết lộ trình của PT theo id; thêm isPending, isError.
export function useChiTietQuanLy(id: string | undefined) {
  const { homNay, quanLy } = useLoTrinh();
  const danhGia = useDanhGiaCheckIn();
  return useMemo(() => {
    const lo = quanLy.find((l) => l.id === id);
    return lo ? chiTietCua(lo, homNay, danhGia) : null;
  }, [homNay, quanLy, id, danhGia]);
}

/** Một check-in của lộ trình PT kèm chi tiết lộ trình (PT-04); `null` khi không tìm thấy lộ trình hoặc check-in. */
// TODO(ROUTE): chuyển sang useQuery gọi API chi tiết check-in theo id (kế hoạch lúc check-in, K5; phiên bản, N13).
export function useCheckInQuanLy(loTrinhId: string | undefined, checkInId: string | undefined) {
  const chiTiet = useChiTietQuanLy(loTrinhId);
  const checkIn = chiTiet?.checkIn.find((c) => c.id === checkInId);
  return chiTiet && checkIn ? { chiTiet, checkIn } : null;
}

/** Lịch sử check-in của một lộ trình học viên (N42); `null` khi không có lộ trình mang id đó. */
export function useLichSuCheckIn(id: string | undefined) {
  const chiTiet = useChiTietLoTrinh(id);
  return useMemo(
    () =>
      chiTiet
        ? {
            loTrinh: chiTiet.loTrinh,
            tienDo: chiTiet.tienDo,
            muc: taoLichSuMau(chiTiet.loTrinh, chiTiet.tienDo, chiTiet.homNay),
          }
        : null,
    [chiTiet],
  );
}

/** Tìm nhiệm vụ trong hôm nay, không có thì trong lịch sử (mở check-in cũ từ màn Lịch sử). */
function useNhiemVu(loTrinhId: string | undefined, nhiemVuId: string | undefined) {
  const lichSu = useLichSuCheckIn(loTrinhId);
  const loTrinh = lichSu?.loTrinh;
  const nhiemVu =
    loTrinh?.nhiemVuHomNay.find((nv) => nv.id === nhiemVuId) ??
    lichSu?.muc.find((m) => m.nhiemVu.id === nhiemVuId)?.nhiemVu;
  return { loTrinh, nhiemVu };
}

/** Nhiệm vụ bữa ăn của một lộ trình (kế hoạch và check-in nếu có); `null` khi không tìm thấy hoặc không phải bữa ăn. */
// TODO(ROUTE): chuyển sang useQuery gọi API chi tiết nhiệm vụ (kế hoạch của đúng ngày, N31) theo id.
export function useNhiemVuBuaAn(loTrinhId: string | undefined, nhiemVuId: string | undefined) {
  const { loTrinh, nhiemVu } = useNhiemVu(loTrinhId, nhiemVuId);
  return loTrinh && nhiemVu?.buaAn ? { loTrinh, nhiemVu, buaAn: nhiemVu.buaAn } : null;
}

/** Nhiệm vụ buổi tập của một lộ trình (kế hoạch và check-in nếu có); `null` khi không tìm thấy hoặc không phải buổi tập. */
// TODO(ROUTE): chuyển sang useQuery gọi API chi tiết nhiệm vụ (bài tập của đúng ngày, N31) theo id.
export function useNhiemVuBuoiTap(loTrinhId: string | undefined, nhiemVuId: string | undefined) {
  const { loTrinh, nhiemVu } = useNhiemVu(loTrinhId, nhiemVuId);
  return loTrinh && nhiemVu?.buoiTap ? { loTrinh, nhiemVu, buoiTap: nhiemVu.buoiTap } : null;
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
