import type { LoaiCheckIn, LoTrinhCuaToi, MucLichSu, NhiemVuHomNay, TienDoLoTrinh } from '../types';
import { layNgayLech, ngayNganGon, tuanHienTaiCua } from '../utils/ngay';

const BUA = [
  { ten: 'Bữa tối', gio: '19:00', moTa: 'Khoai lang, cá hồi, salad', kcal: 650 },
  { ten: 'Bữa trưa', gio: '12:00', moTa: 'Cơm gạo lứt, ức gà, rau luộc', kcal: 700 },
  { ten: 'Bữa sáng', gio: '07:00', moTa: 'Yến mạch, trứng luộc, chuối', kcal: 550 },
];
const BUOI = ['Upper A', 'Lower A', 'Upper B'];

/** Cứ bao nhiêu mục thì có một mục bỏ lỡ, để đủ `boLo` mục rải đều trong `tong`. */
function laBoLo(i: number, tong: number, boLo: number): boolean {
  return Math.floor(((i + 1) * boLo) / tong) > Math.floor((i * boLo) / tong);
}

function nhiemVuCu(
  loTrinhId: string,
  loai: LoaiCheckIn,
  i: number,
  ngay: string,
  boLo: boolean,
): NhiemVuHomNay {
  const id = `ls-${loTrinhId}-${loai}-${i}`;
  const trangThai = boLo ? 'khong_hoan_thanh' : 'da_check';
  if (loai === 'bua_an') {
    const bua = BUA[i % BUA.length] ?? BUA[0]!;
    return {
      id,
      loai,
      tieuDe: `${bua.ten} – ${bua.gio}`,
      moTa: bua.moTa,
      trangThai,
      buaAn: {
        keHoach: { ten: bua.ten, gio: bua.gio, moTa: bua.moTa, kcal: bua.kcal },
        checkIn: boLo
          ? undefined
          : {
              trangThaiBua: 'da_an',
              anh: [`https://picsum.photos/seed/${id}/400/400`],
              ghiChu: '',
              trangThaiDuyet: 'da_duyet',
              gioGui: `${bua.gio} ngày ${ngayNganGon(ngay)}`,
            },
      },
    };
  }
  const ten = BUOI[i % BUOI.length] ?? 'Buổi tập';
  const baiTap = [
    { id: `${id}-1`, ten: 'Bài khởi động', soSet: 1, soLan: '10 phút' },
    { id: `${id}-2`, ten: 'Bài chính', soSet: 4, soLan: '8–10 rep' },
  ];
  return {
    id,
    loai,
    tieuDe: `Buổi tập – ${ten}`,
    moTa: ten,
    trangThai,
    buoiTap: {
      keHoach: { ten, gio: '17:30', nhomCo: 'Toàn thân', thoiLuongPhut: 60, baiTap },
      checkIn: boLo
        ? undefined
        : {
            trangThaiBuoi: 'da_tap',
            baiTapXong: baiTap.map((b) => b.id),
            ghiChu: '',
            trangThaiDuyet: 'da_duyet',
            gioGui: `18:45 ngày ${ngayNganGon(ngay)}`,
          },
    },
  };
}

/**
 * Lịch sử mẫu khớp với số "x/y" ở khối chỉ số: check-in hôm nay cộng các ngày trước, mới nhất ở trên.
 */
// TODO(ROUTE): thay bằng API lịch sử check-in (phân trang theo ngày, lọc loại và trạng thái; adr.md, N42).
export function taoLichSuMau(
  lo: LoTrinhCuaToi,
  tienDo: Pick<TienDoLoTrinh, 'buaAn' | 'buoiTap'>,
  homNay: Date,
): MucLichSu[] {
  const taoLoai = (loai: LoaiCheckIn, so: { xong: number; tong: number }): MucLichSu[] => {
    const homNayDaCo = lo.nhiemVuHomNay.filter(
      (nv) =>
        nv.loai === loai && (nv.trangThai === 'da_check' || nv.trangThai === 'khong_hoan_thanh'),
    );
    const tong = Math.max(0, so.tong - homNayDaCo.length);
    const xong = Math.min(
      tong,
      Math.max(0, so.xong - homNayDaCo.filter((nv) => nv.trangThai === 'da_check').length),
    );
    const cu = Array.from({ length: tong }, (_, i) => {
      // Mỗi ngày 3 bữa; buổi tập cách ngày.
      const lui = loai === 'bua_an' ? Math.floor(i / BUA.length) + 1 : i * 2 + 1;
      const ngay = layNgayLech(homNay, -lui);
      return { ngay, nhiemVu: nhiemVuCu(lo.id, loai, i, ngay, laBoLo(i, tong, tong - xong)) };
    });
    const moi = homNayDaCo.map((nhiemVu) => ({ ngay: layNgayLech(homNay, 0), nhiemVu }));
    return [...moi, ...cu].map((m) => ({
      ...m,
      tuan: tuanHienTaiCua(lo.ngayBatDau, new Date(`${m.ngay}T00:00:00`), lo.tongTuan),
    }));
  };
  return [...taoLoai('bua_an', tienDo.buaAn), ...taoLoai('buoi_tap', tienDo.buoiTap)].sort((a, b) =>
    b.ngay.localeCompare(a.ngay),
  );
}
