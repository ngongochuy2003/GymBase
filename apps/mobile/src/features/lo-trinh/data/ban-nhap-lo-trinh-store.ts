import { useSyncExternalStore } from 'react';
import type { BuaAnKeHoach, LichTapTheoTuan } from '../types';

interface BanNhap {
  buaAn: BuaAnKeHoach[];
  lichTap: LichTapTheoTuan;
}

/** Giá trị điền sẵn của form tạo lộ trình, để xem đủ giao diện. Mỗi lần gọi trả bản mới để không dùng chung tham chiếu. */
// TODO(ROUTE): form bắt đầu trống hoặc nạp bản nháp của PT (adr.md, N22); bỏ giá trị điền sẵn này.
function macDinh(): BanNhap {
  return {
    buaAn: [
      {
        id: 'sang',
        ten: 'Bữa sáng',
        gio: '07:00',
        bat: true,
        moTa: 'Yến mạch với sữa, 3 trứng luộc, 1 chuối',
        kcal: 550,
      },
      {
        id: 'trua',
        ten: 'Bữa trưa',
        gio: '12:00',
        bat: true,
        moTa: 'Cơm gạo lứt, ức gà áp chảo, rau xanh luộc',
        kcal: 700,
      },
      {
        id: 'toi',
        ten: 'Bữa tối',
        gio: '19:00',
        bat: true,
        moTa: 'Khoai lang, cá hồi nướng, salad',
        kcal: 600,
      },
    ],
    lichTap: {
      chung: [
        {
          thu: 1,
          ten: 'Upper A',
          gio: '17:30',
          nhomCo: 'Ngực, vai, tay sau',
          thoiLuongPhut: 60,
          baiTap: [
            { id: 'ua-1', ten: 'Đẩy ngực với tạ đòn', soSet: 4, soLan: '8–10 rep' },
            { id: 'ua-2', ten: 'Đẩy vai với tạ đơn', soSet: 3, soLan: '10 rep' },
            { id: 'ua-3', ten: 'Đẩy tay sau với dây cáp', soSet: 3, soLan: '12 rep' },
          ],
        },
        {
          thu: 3,
          ten: 'Lower A',
          gio: '17:30',
          nhomCo: 'Chân, mông',
          thoiLuongPhut: 60,
          baiTap: [
            { id: 'la-1', ten: 'Squat với tạ đòn', soSet: 4, soLan: '8 rep' },
            { id: 'la-2', ten: 'Romanian deadlift', soSet: 3, soLan: '10 rep' },
            { id: 'la-3', ten: 'Đạp đùi trên máy', soSet: 3, soLan: '12 rep' },
          ],
        },
        {
          thu: 5,
          ten: 'Upper B',
          gio: '17:30',
          nhomCo: 'Lưng, tay trước',
          thoiLuongPhut: 60,
          baiTap: [
            { id: 'ub-1', ten: 'Kéo xà', soSet: 4, soLan: '6–8 rep' },
            { id: 'ub-2', ten: 'Kéo cáp ngồi', soSet: 3, soLan: '10 rep' },
            { id: 'ub-3', ten: 'Cuốn tạ đơn', soSet: 3, soLan: '12 rep' },
          ],
        },
      ],
      rieng: {},
    },
  };
}

let trangThai: BanNhap = macDinh();
const nguoiNghe = new Set<() => void>();

function capNhat(moi: BanNhap) {
  trangThai = moi;
  nguoiNghe.forEach((nghe) => nghe());
}

function dangKy(nghe: () => void) {
  nguoiNghe.add(nghe);
  return () => nguoiNghe.delete(nghe);
}

/**
 * Bản nháp kế hoạch của form tạo lộ trình (bữa ăn và lịch tập), giữ trong bộ nhớ để màn PT-02 và hai màn con PT-02a,
 * PT-02b cùng đọc và sửa. Quay lại PT-01 không làm mất; tạo lộ trình xong thì đặt lại.
 */
// TODO(ROUTE): thay bằng bản nháp lưu ở backend (adr.md, N22, K8); bỏ kho này.
export function useBanNhapLoTrinh(): BanNhap {
  return useSyncExternalStore(dangKy, () => trangThai);
}

/** Bản nháp hiện tại, đọc ngay lúc gọi (dùng trong xử lý bất đồng bộ, nơi state của màn có thể đã cũ). */
export function layBanNhap(): BanNhap {
  return trangThai;
}

export function datBuaAn(buaAn: BuaAnKeHoach[]) {
  capNhat({ ...trangThai, buaAn });
}

export function datLichTap(lichTap: LichTapTheoTuan) {
  capNhat({ ...trangThai, lichTap });
}

/** Đưa bản nháp về giá trị ban đầu. */
export function datLaiBanNhap() {
  capNhat(macDinh());
}
