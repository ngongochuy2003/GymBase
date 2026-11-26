import type { SuKienNhac } from '@/shared/notifications';
import type { KeHoachLoTrinh, LoTrinhCuaToi } from '../types';
import { buoiTapCuaTuan } from './lich-tap';
import { layNgayLech, soNgayConLai } from './ngay';

/**
 * Các sự kiện cần nhắc của học viên trong `soNgay` ngày tới, tính từ kế hoạch ăn và lịch tập của lộ trình đang hoạt động.
 * Bữa ăn bật nhắc lặp mỗi ngày; buổi tập theo thứ trong tuần của lộ trình (tuần nào có bản sửa riêng thì theo bản đó).
 * Nhiệm vụ hôm nay đã check thì không nhắc nữa (ADR-2, quyết định 14).
 */
export function suKienNhacCuaLoTrinh(
  loTrinhs: readonly LoTrinhCuaToi[],
  keHoach: KeHoachLoTrinh,
  homNay: Date,
  soNgay: number,
): SuKienNhac[] {
  // Hai lộ trình cùng giờ ăn hay giờ tập chỉ cần một lần nhắc.
  const theoKhoa = new Map<string, SuKienNhac>();
  const them = (sk: SuKienNhac) => {
    const khoa = `${sk.loai}|${sk.ngay}|${sk.gio}`;
    if (!theoKhoa.has(khoa)) theoKhoa.set(khoa, sk);
  };

  for (const lo of loTrinhs) {
    if (lo.trangThai !== 'dang_hoat_dong') continue;
    const daXongHomNay = new Set(
      lo.nhiemVuHomNay
        .filter((nv) => nv.trangThai === 'da_check')
        .map((nv) => (nv.buaAn ? `bua_an|${nv.buaAn.keHoach.gio}` : nv.buoiTap ? `buoi_tap|${nv.buoiTap.keHoach.gio}` : '')),
    );

    for (let lech = 0; lech <= soNgay; lech++) {
      const ngay = layNgayLech(homNay, lech);
      if (ngay < lo.ngayBatDau || ngay > lo.ngayKetThuc) continue;
      const tuan = Math.floor(-soNgayConLai(lo.ngayBatDau, new Date(homNay.getFullYear(), homNay.getMonth(), homNay.getDate() + lech)) / 7) + 1;
      const thuTrongTuan = (new Date(homNay.getFullYear(), homNay.getMonth(), homNay.getDate() + lech).getDay() + 6) % 7 + 1;

      for (const bua of keHoach.buaAn) {
        if (!bua.bat || (lech === 0 && daXongHomNay.has(`bua_an|${bua.gio}`))) continue;
        them({ loai: 'nhac_an', maSuKien: `${lo.id}:${bua.id}`, ngay, gio: bua.gio });
      }
      for (const buoi of buoiTapCuaTuan(keHoach.lichTap, tuan)) {
        if (buoi.thu !== thuTrongTuan || (lech === 0 && daXongHomNay.has(`buoi_tap|${buoi.gio}`))) continue;
        them({ loai: 'nhac_tap', maSuKien: `${lo.id}:tap-${buoi.thu}-${buoi.gio.replace(':', '')}`, ngay, gio: buoi.gio });
      }
    }
  }

  return [...theoKhoa.values()];
}
