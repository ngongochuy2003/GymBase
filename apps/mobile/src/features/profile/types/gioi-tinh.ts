/** Mã giới tính gửi lên API hồ sơ (`gioi_tinh`); khớp enum `GioiTinh` của backend. */
export type GioiTinh = 'nam' | 'nu' | 'khac' | 'khong_muon_noi';

/** Danh sách cố định: giới tính không đọc từ danh mục trên server. Hiển thị `ten`, gửi `ma`. */
export const GIOI_TINH_OPTIONS: readonly { ma: GioiTinh; ten: string }[] = [
  { ma: 'nam', ten: 'Nam' },
  { ma: 'nu', ten: 'Nữ' },
  { ma: 'khac', ten: 'Khác' },
];
