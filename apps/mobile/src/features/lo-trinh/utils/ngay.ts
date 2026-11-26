import type { LoTrinhQuanLy, ThangLoc, TrangThaiLoTrinh } from '../types';

const MS_MOT_NGAY = 24 * 60 * 60 * 1000;

/** `yyyy-MM-dd` theo giờ địa phương (không dùng toISOString vì lệch múi giờ). */
export function dinhDangNgayIso(ngay: Date): string {
  const thang = String(ngay.getMonth() + 1).padStart(2, '0');
  const ngayTrongThang = String(ngay.getDate()).padStart(2, '0');
  return `${ngay.getFullYear()}-${thang}-${ngayTrongThang}`;
}

export function layNgayLech(goc: Date, soNgay: number): string {
  const ngay = new Date(goc.getFullYear(), goc.getMonth(), goc.getDate() + soNgay);
  return dinhDangNgayIso(ngay);
}

function docNgayIso(iso: string): Date {
  const [nam = 0, thang = 1, ngay = 1] = iso.split('-').map(Number);
  return new Date(nam, thang - 1, ngay);
}

/** Số ngày còn lại tính từ `homNay` đến `ngayKetThuc` (âm nếu đã qua). */
export function soNgayConLai(ngayKetThuc: string, homNay: Date): number {
  const dau = new Date(homNay.getFullYear(), homNay.getMonth(), homNay.getDate()).getTime();
  return Math.round((docNgayIso(ngayKetThuc).getTime() - dau) / MS_MOT_NGAY);
}

export function thangHienTai(homNay: Date): ThangLoc {
  return { nam: homNay.getFullYear(), thang: homNay.getMonth() + 1 };
}

export function doiThang(thang: ThangLoc, soThang: number): ThangLoc {
  const ngay = new Date(thang.nam, thang.thang - 1 + soThang, 1);
  return { nam: ngay.getFullYear(), thang: ngay.getMonth() + 1 };
}

export function nhanThang(thang: ThangLoc): string {
  return `Tháng ${thang.thang}/${thang.nam}`;
}

/** Lộ trình thuộc tháng khi khoảng ngày bắt đầu đến kết thúc giao với tháng đó. */
export function giaoVoiThang(
  lo: Pick<LoTrinhQuanLy, 'ngayBatDau' | 'ngayKetThuc'>,
  thang: ThangLoc,
): boolean {
  const dauThang = new Date(thang.nam, thang.thang - 1, 1).getTime();
  const cuoiThang = new Date(thang.nam, thang.thang, 0).getTime();
  return (
    docNgayIso(lo.ngayBatDau).getTime() <= cuoiThang &&
    docNgayIso(lo.ngayKetThuc).getTime() >= dauThang
  );
}

/** `dd/MM` để in gọn trên thẻ. */
export function ngayNganGon(iso: string): string {
  const ngay = docNgayIso(iso);
  return `${String(ngay.getDate()).padStart(2, '0')}/${String(ngay.getMonth() + 1).padStart(2, '0')}`;
}

export function locLoTrinhQuanLy(
  danhSach: readonly LoTrinhQuanLy[],
  boLoc: { trangThai: TrangThaiLoTrinh; thang: ThangLoc },
): LoTrinhQuanLy[] {
  return danhSach.filter((lo) => lo.trangThai === boLoc.trangThai && giaoVoiThang(lo, boLoc.thang));
}

const THU = ['Chủ nhật', 'Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7'];

/** Ví dụ `Thứ 2, 9 Tháng 11`. */
export function nhanNgayDayDu(ngay: Date): string {
  return `${THU[ngay.getDay()]}, ${ngay.getDate()} Tháng ${ngay.getMonth() + 1}`;
}

/** `HH:mm` của một thời điểm (mặc định bây giờ). */
export function gioHienTai(ngay: Date = new Date()): string {
  return `${String(ngay.getHours()).padStart(2, '0')}:${String(ngay.getMinutes()).padStart(2, '0')}`;
}

/** Tuần thứ mấy của lộ trình tại `homNay` (Tuần 1 là tuần bắt đầu), kẹp trong 1 đến `tongTuan`. */
export function tuanHienTaiCua(ngayBatDau: string, homNay: Date, tongTuan: number): number {
  const dau = new Date(homNay.getFullYear(), homNay.getMonth(), homNay.getDate()).getTime();
  const soNgay = Math.round((dau - docNgayIso(ngayBatDau).getTime()) / MS_MOT_NGAY);
  return Math.max(1, Math.min(tongTuan, Math.floor(soNgay / 7) + 1));
}
