export type TrangThaiLoTrinh = 'dang_hoat_dong' | 'hoan_thanh' | 'that_bai';

export type LoaiLoTrinh = 'don' | 'nhom';

export type LoaiNhiemVu = 'bua_an' | 'buoi_tap' | 'can_nang' | 'anh_co_the';

/** Trạng thái một nhiệm vụ trong ngày: đã check, đến lúc check-in (hiện nút), chưa tới, hoặc không hoàn thành (bỏ lỡ). */
export type TrangThaiNhiemVu = 'da_check' | 'check_in_ngay' | 'chua_check' | 'khong_hoan_thanh';

export interface NguoiLoTrinh {
  id: string;
  ten: string;
  anh?: string;
}

export interface NhiemVuHomNay {
  id: string;
  loai: LoaiNhiemVu;
  tieuDe: string;
  moTa: string;
  trangThai: TrangThaiNhiemVu;
  /** Chỉ có ở nhiệm vụ bữa ăn. */
  buaAn?: BuaAnNhiemVu;
  /** Chỉ có ở nhiệm vụ buổi tập. */
  buoiTap?: BuoiTapNhiemVu;
  /** Cân nặng đã ghi, chỉ có ở nhiệm vụ cân nặng. */
  canNangGhi?: GhiCanNang;
  /** Ảnh cơ thể đã ghi, chỉ có ở nhiệm vụ ảnh cơ thể. */
  anhCoTheGhi?: GhiAnhCoThe;
}

interface LoTrinhCoBan {
  id: string;
  ten: string;
  loai: LoaiLoTrinh;
  trangThai: TrangThaiLoTrinh;
  /** Ngày dạng `yyyy-MM-dd`. */
  ngayBatDau: string;
  ngayKetThuc: string;
  tongTuan: number;
  /** 0 đến 100. */
  tienDo: number;
  anhBia?: string;
}

/** Thẻ ở sub-tab "Lộ trình của tôi" (góc nhìn học viên). */
export interface LoTrinhCuaToi extends LoTrinhCoBan {
  pt: NguoiLoTrinh;
  tuanHienTai: number;
  mucTieu: { tu: number; den: number; donVi: string };
  /** Phần trăm tuân thủ kế hoạch, 0 đến 100. */
  tuanThu: number;
  checkInTiepTheo: string;
  nhiemVuHomNay: NhiemVuHomNay[];
}

/** Thẻ ở sub-tab "Lộ trình tôi quản lý" (góc nhìn PT). */
export interface LoTrinhQuanLy extends LoTrinhCoBan {
  nguoiThamGia: NguoiLoTrinh[];
  soCheckInChoDuyet: number;
  /** Số buổi tập của học viên rơi vào hôm nay (cộng lại thành ô "lịch tập hôm nay"). */
  soBuoiTapHomNay: number;
  /** Nhiệm vụ tiếp theo của học viên, `quaHan` để hiện cảnh báo. */
  nhiemVuTiepTheo?: { loai: LoaiNhiemVu; chiTiet: string; quaHan: boolean };
  /** Thất bại hoặc hoàn thành sớm: lý do PT nhập khi đóng lộ trình trước hạn. */
  lyDoDong?: string;
}

export interface ThangLoc {
  nam: number;
  /** 1 đến 12. */
  thang: number;
}

/** Bữa ăn trong kế hoạch dinh dưỡng của form tạo lộ trình. */
export interface BuaAnKeHoach {
  id: string;
  ten: string;
  /** `HH:mm`. */
  gio: string;
  bat: boolean;
  /** Món ăn và ghi chú món ưu tiên PT đặt cho bữa này (PT-02a). */
  moTa?: string;
  /** Năng lượng PT nhập tay, đơn vị kcal (N30). */
  kcal?: number;
  /** Ảnh minh họa món, tùy chọn. */
  anh?: string;
}

/** Một buổi tập trong tuần. `thu` từ 1 (Thứ 2) đến 7 (Chủ nhật). */
export interface BuoiTapTuan {
  thu: number;
  ten: string;
  /** `HH:mm`. */
  gio: string;
  /** Nhóm cơ tập trong buổi, ví dụ "Ngực, vai, tay sau" (PT-02b). */
  nhomCo?: string;
  thoiLuongPhut?: number;
  baiTap?: BaiTap[];
}

/**
 * Lịch tập theo tuần: `chung` là Tuần 1, các tuần sau tự theo `chung` trừ tuần đã sửa riêng (có khóa trong `rieng`).
 */
export interface LichTapTheoTuan {
  chung: BuoiTapTuan[];
  rieng: Record<number, BuoiTapTuan[]>;
}

/** Một lần cân nặng. Mỗi tuần cân 2 lần: `lan` là 1 hoặc 2 trong tuần (Tuần 1 là tuần bắt đầu lộ trình). */
export interface DiemCanNang {
  tuan: number;
  lan: number;
  kg: number;
}

export interface AnhCoTheMoc {
  /** Mốc tuần tính từ ngày bắt đầu lộ trình. */
  tuan: number;
  uri: string;
  /** Ai xem được ảnh này (N33). */
  quyenXem: QuyenXemAnh;
}

/** Nhận xét mới nhất của PT trên một check-in. */
export interface DanhGiaTuPt {
  pt: NguoiLoTrinh;
  noiDung: string;
  /** Thời điểm đã định dạng để hiển thị, ví dụ `Hôm nay`. */
  thoiDiem: string;
}

/** Số liệu tiến độ của một lộ trình (tab Tổng quan, HV-07). */
export interface TienDoLoTrinh {
  canNang: DiemCanNang[];
  buoiTap: { xong: number; tong: number };
  buaAn: { xong: number; tong: number };
  anhMoc: AnhCoTheMoc[];
  danhGia?: DanhGiaTuPt;
}

/** Kết quả bữa ăn học viên báo trong check-in (N3: chỉ hai giá trị, không có "ăn một phần"). */
export type TrangThaiBuaAn = 'da_an' | 'bo_bua';

/** Trạng thái duyệt của một check-in, theo quy tắc sửa ở N13. */
export type TrangThaiDuyet = 'cho_duyet' | 'can_bo_sung' | 'da_duyet';

/** Bữa ăn PT đặt trong kế hoạch của ngày (snapshot, K5). */
export interface KeHoachBuaAn {
  ten: string;
  /** `HH:mm`. */
  gio: string;
  moTa: string;
  kcal: number;
  anh?: string;
}

/** Check-in bữa ăn học viên đã gửi. */
export interface CheckInBuaAn {
  trangThaiBua: TrangThaiBuaAn;
  /** Ảnh bữa ăn, tối đa 3. */
  anh: string[];
  ghiChu: string;
  trangThaiDuyet: TrangThaiDuyet;
  /** `HH:mm` lúc gửi. */
  gioGui: string;
  /** Nhận xét của PT khi duyệt hoặc yêu cầu bổ sung. */
  danhGia?: DanhGiaTuPt;
}

/** Chi tiết bữa ăn của một nhiệm vụ: kế hoạch và (nếu đã gửi) check-in. */
export interface BuaAnNhiemVu {
  keHoach: KeHoachBuaAn;
  checkIn?: CheckInBuaAn;
}

/** Kết quả buổi tập học viên báo trong check-in (HV-06: ba giá trị). */
export type TrangThaiBuoiTap = 'da_tap' | 'tap_mot_phan' | 'nghi_tap';

/** Một bài trong buổi tập. */
export interface BaiTap {
  id: string;
  ten: string;
  /** Số set (hiệp). */
  soSet: number;
  /** Số lần mỗi set kèm đơn vị, ví dụ "8–10 rep", "45 giây", "20 phút". */
  soLan: string;
  anh?: string;
}

/** Buổi tập PT đặt trong kế hoạch của ngày (snapshot, K5). */
export interface KeHoachBuoiTap {
  ten: string;
  /** `HH:mm`. */
  gio: string;
  nhomCo: string;
  thoiLuongPhut: number;
  anh?: string;
  baiTap: BaiTap[];
}

/** Check-in buổi tập học viên đã gửi. */
export interface CheckInBuoiTap {
  trangThaiBuoi: TrangThaiBuoiTap;
  /** Id các bài đã tick hoàn thành. */
  baiTapXong: string[];
  ghiChu: string;
  /** Một ảnh buổi tập, tùy chọn. */
  anh?: string;
  trangThaiDuyet: TrangThaiDuyet;
  /** `HH:mm` lúc gửi. */
  gioGui: string;
  danhGia?: DanhGiaTuPt;
}

/** Chi tiết buổi tập của một nhiệm vụ: kế hoạch và (nếu đã gửi) check-in. */
export interface BuoiTapNhiemVu {
  keHoach: KeHoachBuoiTap;
  checkIn?: CheckInBuoiTap;
}

/** Cân nặng học viên ghi cho một nhiệm vụ. Ghi trực tiếp, không qua PT duyệt (N16). */
export interface GhiCanNang {
  kg: number;
  /** Ảnh kèm theo (ví dụ mặt cân), tùy chọn. */
  anh?: string;
  /** `HH:mm` lúc ghi. */
  gioGhi: string;
}

/** Ai xem được ảnh cơ thể (N33): chỉ học viên, hoặc cả PT trong tiến độ. */
export type QuyenXemAnh = 'rieng_tu' | 'cho_pt';

/** Ảnh cơ thể học viên ghi cho một nhiệm vụ. Ghi trực tiếp, không qua PT duyệt (N16). */
export interface GhiAnhCoThe {
  anh: string;
  /** Ghi chú của học viên về ảnh, có thể để trống. */
  ghiChu: string;
  quyenXem: QuyenXemAnh;
  /** `HH:mm` lúc ghi. */
  gioGhi: string;
}

/** Trạng thái lượt tham gia của một học viên trong lộ trình (K9, N23). */
export type TrangThaiThamGia = 'cho_chap_nhan' | 'tham_gia' | 'tu_choi' | 'da_roi';

export interface ThanhVienLoTrinh extends NguoiLoTrinh {
  thamGia: TrangThaiThamGia;
}

/** Mốc của một nhiệm vụ chung trong ngày, tính theo giờ: quá hạn, đến giờ chờ check-in, sắp tới, hoặc để mai. */
export type MocNhiemVuNhom = 'qua_han' | 'cho_check_in' | 'sap_toi' | 'ngay_mai';

/**
 * Nhiệm vụ hôm nay ở góc nhìn PT. Nhiệm vụ là của cả lộ trình (nhóm cũng chung một danh sách); `daCheckIn` là id các thành
 * viên đã check-in hoặc đã ghi. Trạng thái hoàn thành suy ra từ đó (xem `trangThaiNhiemVuNhom`).
 */
export interface NhiemVuNhom {
  id: string;
  loai: LoaiNhiemVu;
  tieuDe: string;
  moTa: string;
  /** `HH:mm`. */
  gio: string;
  moc: MocNhiemVuNhom;
  daCheckIn: string[];
}

/** Một check-in bữa ăn hoặc buổi tập ở góc nhìn PT (cân nặng và ảnh cơ thể không qua duyệt, N16). */
export interface CheckInQuanLy {
  id: string;
  loai: 'bua_an' | 'buoi_tap';
  tieuDe: string;
  /** Đã định dạng để hiển thị, ví dụ `Hôm nay, 12:00`. */
  thoiDiem: string;
  hocVien: NguoiLoTrinh;
  trangThai: TrangThaiDuyet;
  /** Tăng mỗi lần học viên sửa check-in; PT đánh giá bản cũ thì bị chặn (adr.md, N13). */
  phienBan: number;
  /** Chi tiết check-in bữa ăn (kế hoạch lúc check-in, ảnh, ghi chú); có khi `loai` là `bua_an`. */
  bua?: BuaAnNhiemVu;
  /** Chi tiết check-in buổi tập; có khi `loai` là `buoi_tap`. */
  buoiTap?: BuoiTapNhiemVu;
  /** Nhận xét PT đã gửi khi duyệt hoặc yêu cầu bổ sung. */
  nhanXet?: DanhGiaTuPt;
}

export interface HoatDongGanDay {
  id: string;
  nguoi: NguoiLoTrinh;
  noiDung: string;
  /** Ví dụ `Vừa xong`. */
  thoiDiem: string;
}

/** Kế hoạch ăn và lịch tập đã đặt cho lộ trình (PT-02, PT-02a, PT-02b). */
export interface KeHoachLoTrinh {
  buaAn: BuaAnKeHoach[];
  lichTap: LichTapTheoTuan;
}

/** Chi tiết một lộ trình ở góc nhìn PT (PT-03): lộ trình đơn hay nhóm đều dùng chung cấu trúc này (adr.md, M17). */
export interface ChiTietQuanLy {
  loTrinh: LoTrinhQuanLy;
  pt: NguoiLoTrinh;
  tuanHienTai: number;
  mucTieu: { tu: number; den: number; donVi: string };
  thanhVien: ThanhVienLoTrinh[];
  nhiemVuHomNay: NhiemVuNhom[];
  checkIn: CheckInQuanLy[];
  hoatDong: HoatDongGanDay[];
  keHoach: KeHoachLoTrinh;
}
