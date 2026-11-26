import { suKienNhacCuaLoTrinh } from '../src/features/lo-trinh/utils/nhac-gio';
import type { KeHoachLoTrinh, LoTrinhCuaToi, NhiemVuHomNay } from '../src/features/lo-trinh/types';

// Thứ Ba 24/11/2026 (giờ địa phương).
const HOM_NAY = new Date(2026, 10, 24, 12, 0, 0);

const KE_HOACH: KeHoachLoTrinh = {
  buaAn: [
    { id: 'sang', ten: 'Bữa sáng', gio: '07:00', bat: true },
    { id: 'trua', ten: 'Bữa trưa', gio: '12:00', bat: true },
    { id: 'toi', ten: 'Bữa tối', gio: '19:00', bat: false },
  ],
  lichTap: {
    // Thứ 2 và Thứ 3 (24/11/2026 là Thứ 3).
    chung: [
      { thu: 1, ten: 'Upper A', gio: '17:30' },
      { thu: 2, ten: 'Lower A', gio: '18:00' },
    ],
    rieng: { 2: [{ thu: 4, ten: 'Tuần 2 riêng', gio: '06:00' }] },
  },
};

function loTrinh(phan: Partial<LoTrinhCuaToi> = {}): LoTrinhCuaToi {
  return {
    id: 'lt1',
    ten: 'Lộ trình',
    loai: 'don',
    trangThai: 'dang_hoat_dong',
    ngayBatDau: '2026-11-23', // Thứ 2, tuần 1
    ngayKetThuc: '2026-12-31',
    tongTuan: 6,
    tienDo: 0,
    pt: { id: 'pt', ten: 'PT' },
    tuanHienTai: 1,
    mucTieu: { tu: 60, den: 70, donVi: 'kg' },
    tuanThu: 100,
    checkInTiepTheo: '',
    nhiemVuHomNay: [],
    ...phan,
  };
}

const gom = (suKien: ReturnType<typeof suKienNhacCuaLoTrinh>) => suKien.map((s) => `${s.loai}|${s.ngay}|${s.gio}`);

describe('suKienNhacCuaLoTrinh', () => {
  it('bữa ăn bật nhắc mỗi ngày, bữa tắt thì không', () => {
    const suKien = suKienNhacCuaLoTrinh([loTrinh()], KE_HOACH, HOM_NAY, 1);

    const an = gom(suKien).filter((s) => s.startsWith('nhac_an'));
    expect(an).toEqual(
      expect.arrayContaining(['nhac_an|2026-11-24|07:00', 'nhac_an|2026-11-24|12:00', 'nhac_an|2026-11-25|07:00', 'nhac_an|2026-11-25|12:00']),
    );
    expect(an.some((s) => s.endsWith('19:00'))).toBe(false);
  });

  it('buổi tập rơi vào đúng thứ trong tuần của kế hoạch', () => {
    const tap = gom(suKienNhacCuaLoTrinh([loTrinh()], KE_HOACH, HOM_NAY, 10)).filter((s) => s.startsWith('nhac_tap'));

    // Tuần 1 (23/11 đến 29/11) theo lịch chung: chỉ còn hôm nay, Thứ 3 18:00. Tuần 2 (từ 30/11) có bản riêng chỉ gồm Thứ 4
    // 06:00 (03/12), nên Thứ 2 và Thứ 3 của tuần 2 không có buổi tập.
    expect(tap).toEqual(['nhac_tap|2026-11-24|18:00', 'nhac_tap|2026-12-03|06:00']);
  });

  it('tuần chưa sửa riêng thì theo lịch chung', () => {
    const tap = gom(suKienNhacCuaLoTrinh([loTrinh()], KE_HOACH, new Date(2026, 11, 7, 12), 0)).filter((s) => s.startsWith('nhac_tap'));

    // 07/12/2026 là Thứ 2 của tuần 3, không có bản riêng nên theo lịch chung (Thứ 2 17:30).
    expect(tap).toEqual(['nhac_tap|2026-12-07|17:30']);
  });

  it('không nhắc ngoài khoảng ngày của lộ trình', () => {
    const lo = loTrinh({ ngayBatDau: '2026-11-25', ngayKetThuc: '2026-11-25' });

    const ngay = new Set(suKienNhacCuaLoTrinh([lo], KE_HOACH, HOM_NAY, 7).map((s) => s.ngay));

    expect(ngay).toEqual(new Set(['2026-11-25']));
  });

  it('chỉ lộ trình đang hoạt động mới có nhắc', () => {
    const suKien = suKienNhacCuaLoTrinh(
      [loTrinh({ trangThai: 'hoan_thanh' }), loTrinh({ id: 'lt2', trangThai: 'that_bai' })],
      KE_HOACH,
      HOM_NAY,
      7,
    );

    expect(suKien).toEqual([]);
  });

  it('nhiệm vụ hôm nay đã check thì không nhắc nữa; ngày khác không bị ảnh hưởng', () => {
    const xongBuaTrua: NhiemVuHomNay = {
      id: 'nv', loai: 'bua_an', tieuDe: 't', moTa: '', trangThai: 'da_check',
      buaAn: { keHoach: { ten: 'Bữa trưa', gio: '12:00', moTa: '', kcal: 0 } },
    };
    const xongTap: NhiemVuHomNay = {
      id: 'nv2', loai: 'buoi_tap', tieuDe: 't', moTa: '', trangThai: 'da_check',
      buoiTap: { keHoach: { ten: 'Lower A', gio: '18:00', nhomCo: '', thoiLuongPhut: 60, baiTap: [] } },
    };

    const s = gom(suKienNhacCuaLoTrinh([loTrinh({ nhiemVuHomNay: [xongBuaTrua, xongTap] })], KE_HOACH, HOM_NAY, 1));

    expect(s).not.toContain('nhac_an|2026-11-24|12:00');
    expect(s).not.toContain('nhac_tap|2026-11-24|18:00');
    expect(s).toContain('nhac_an|2026-11-24|07:00');
    expect(s).toContain('nhac_an|2026-11-25|12:00');
  });

  it('nhiệm vụ chưa check hoặc không hoàn thành vẫn giữ nhắc theo kế hoạch', () => {
    const chuaCheck: NhiemVuHomNay = {
      id: 'nv', loai: 'bua_an', tieuDe: 't', moTa: '', trangThai: 'chua_check',
      buaAn: { keHoach: { ten: 'Bữa trưa', gio: '12:00', moTa: '', kcal: 0 } },
    };

    const s = gom(suKienNhacCuaLoTrinh([loTrinh({ nhiemVuHomNay: [chuaCheck] })], KE_HOACH, HOM_NAY, 0));

    expect(s).toContain('nhac_an|2026-11-24|12:00');
  });

  it('hai lộ trình cùng giờ ăn thì chỉ nhắc một lần', () => {
    const suKien = suKienNhacCuaLoTrinh([loTrinh(), loTrinh({ id: 'lt2' })], KE_HOACH, HOM_NAY, 1);

    const an0700 = suKien.filter((s) => s.loai === 'nhac_an' && s.ngay === '2026-11-24' && s.gio === '07:00');
    expect(an0700).toHaveLength(1);
  });

  it('mã sự kiện ổn định giữa hai lần tính, để identifier không đổi', () => {
    const a = suKienNhacCuaLoTrinh([loTrinh()], KE_HOACH, HOM_NAY, 7);
    const b = suKienNhacCuaLoTrinh([loTrinh()], KE_HOACH, HOM_NAY, 7);

    expect(a).toEqual(b);
    expect(a[0]?.maSuKien).toMatch(/^lt1:/);
  });
});
