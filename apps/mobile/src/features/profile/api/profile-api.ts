import { getMobClient } from '@/core/api/mobile-client';

/**
 * Body của `PUT /api/v1/mob/ho-so` (docs/api/ho-so.md). Khối nào có tùy vai trò: `hoc_vien` gửi khối học viên, `pt` gửi khối
 * PT, `ca_hai` gửi cả hai.
 */
export type HoSoLuuInput = {
  loai_nguoi_dung: string;
  ten_hien_thi: string;
  /** `yyyy-MM-dd`; bỏ trống thì không gửi. */
  ngay_sinh?: string;
  gioi_tinh?: string;
  /** Mã phường/xã, hoặc mã tỉnh/thành nếu không chọn phường/xã. */
  khu_vuc_hoat_dong?: string;
  /** `file_id` ảnh đại diện đã tải lên; bỏ trống thì giữ ảnh đang có. */
  anh_dai_dien_tep_tin_id?: string;
  // Khối học viên.
  muc_tieu_tap_luyen?: string;
  trinh_do_tap?: string;
  chieu_cao_cm?: number;
  can_nang_kg?: number;
  // Khối PT.
  chuyen_mon?: string[];
  so_nam_kinh_nghiem?: number;
  gioi_thieu?: string;
  /** `file_id` của chứng chỉ PDF đã tải lên; bỏ trống thì hồ sơ không có chứng chỉ (chứng chỉ đang có bị gỡ). */
  chung_chi_tep_tin_id?: string;
};

/** Chứng chỉ PT (tệp PDF riêng tư) đang gắn vào hồ sơ. */
export type ChungChiPt = {
  file_id: string;
  ten_tep: string;
  kich_thuoc_byte: number;
};

/** Hồ sơ của chính chủ do API trả về. */
export type HoSo = {
  loai_nguoi_dung: string;
  ten_hien_thi: string;
  ngay_sinh: string | null;
  gioi_tinh: string | null;
  trinh_do_tap: string | null;
  muc_tieu_tap_luyen: string | null;
  khu_vuc_hoat_dong: string | null;
  chuyen_mon: string[] | null;
  so_nam_kinh_nghiem: number | null;
  gioi_thieu: string | null;
  trang_thai_xac_minh: string;
  chi_so_co_the: {
    ngay_do: string;
    chieu_cao_cm: number | null;
    can_nang_kg: number | null;
  } | null;
  chung_chi: ChungChiPt | null;
};

/** Endpoint `/api/v1/mob/ho-so`: một lần lưu ghi cả hồ sơ lẫn chỉ số cơ thể trong một transaction. */
export const profileApi = {
  /** Hồ sơ hiện tại; chưa thiết lập thì lỗi `PROFILE_NOT_FOUND` (404). */
  async get(): Promise<HoSo> {
    const { data } = await getMobClient().get<HoSo>('/ho-so');
    return data;
  },
  async save(input: HoSoLuuInput): Promise<HoSo> {
    const { data } = await getMobClient().put<HoSo>('/ho-so', input);
    return data;
  },
};
