/** Vai trò sử dụng người dùng chọn ở màn 02A; khớp `loai_nguoi_dung` của API hồ sơ. */
export type VaiTro = 'hoc_vien' | 'pt' | 'ca_hai';

const VAI_TRO: readonly VaiTro[] = ['hoc_vien', 'pt', 'ca_hai'];

/** Đọc `loai` từ query; thiếu hoặc sai thì mặc định `hoc_vien`. */
export function parseVaiTro(raw: string | string[] | undefined): VaiTro {
  const value = Array.isArray(raw) ? raw[0] : raw;
  return VAI_TRO.find((vaiTro) => vaiTro === value) ?? 'hoc_vien';
}
