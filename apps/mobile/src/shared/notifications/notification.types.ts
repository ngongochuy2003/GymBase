/** Năm loại thông báo, trùng tên `LoaiThongBao` của backend; cũng là id của kênh thông báo Android. */
export const LOAI_THONG_BAO = ['nhac_an', 'nhac_tap', 'tin_nhan', 'danh_gia', 'ket_noi_pt'] as const;
export type LoaiThongBao = (typeof LOAI_THONG_BAO)[number];

/** Một dòng hộp thư (`GET /thong-bao`). `loai_thong_bao` là chuỗi: server có thể thêm loại mới trước khi app cập nhật. */
export interface ThongBaoDto {
  id: string;
  loai_thong_bao: string;
  tieu_de: string;
  noi_dung: string | null;
  loai_doi_tuong: string | null;
  doi_tuong_id: string | null;
  /** ISO 8601 (UTC): lúc thông báo tới hạn. */
  thoi_diem: string;
  da_doc: boolean;
}

export interface HopThuDto {
  data: ThongBaoDto[];
  con_nua: boolean;
}
