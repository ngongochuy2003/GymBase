import { z } from 'zod';

/** Khớp luật của backend (`HoSoService`): tuổi hợp lệ 13–100. */
export const TUOI_NHO_NHAT = 13;
export const TUOI_LON_NHAT = 100;

/** Giữ tối đa 8 chữ số và tự chèn dấu `/` để người dùng gõ `20051998` ra `20/05/1998`. */
export function formatNgaySinhInput(text: string): string {
  const digits = text.replace(/\D/g, '').slice(0, 8);
  if (digits.length <= 2) return digits;
  if (digits.length <= 4) return `${digits.slice(0, 2)}/${digits.slice(2)}`;
  return `${digits.slice(0, 2)}/${digits.slice(2, 4)}/${digits.slice(4)}`;
}

/** Đọc `dd/MM/yyyy` thành ngày; null nếu sai định dạng hoặc ngày không tồn tại (ví dụ 31/02). */
export function parseNgaySinh(text: string): Date | null {
  const match = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(text);
  if (!match) return null;
  const [day, month, year] = [Number(match[1]), Number(match[2]), Number(match[3])];
  const date = new Date(year, month - 1, day);
  const valid =
    date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day;
  return valid ? date : null;
}

/** `dd/MM/yyyy` sang `yyyy-MM-dd` (định dạng gửi lên API). */
export function toIsoNgaySinh(text: string): string {
  const [day, month, year] = text.split('/');
  return `${year}-${month}-${day}`;
}

/** Tuổi tròn tại thời điểm `now`. */
export function tinhTuoi(ngaySinh: Date, now: Date): number {
  let tuoi = now.getFullYear() - ngaySinh.getFullYear();
  const chuaSinhNhat =
    now.getMonth() < ngaySinh.getMonth() ||
    (now.getMonth() === ngaySinh.getMonth() && now.getDate() < ngaySinh.getDate());
  if (chuaSinhNhat) tuoi -= 1;
  return tuoi;
}

export const personalInfoSchema = z
  .object({
    ten_hien_thi: z
      .string()
      .trim()
      .min(1, 'Vui lòng nhập tên hiển thị.')
      .max(100, 'Tên hiển thị tối đa 100 ký tự.'),
    ngay_sinh: z.string(),
    gioi_tinh: z.string().min(1, 'Vui lòng chọn giới tính.'),
    khu_vuc_hoat_dong: z.string().min(1, 'Vui lòng chọn khu vực.'),
  })
  .superRefine((form, ctx) => {
    const fail = (message: string) =>
      ctx.addIssue({ code: 'custom', path: ['ngay_sinh'], message });
    if (form.ngay_sinh.trim() === '') return fail('Vui lòng nhập ngày sinh.');
    const ngaySinh = parseNgaySinh(form.ngay_sinh);
    if (!ngaySinh) return fail('Ngày sinh chưa đúng, ví dụ 20/05/1998.');
    const now = new Date();
    if (ngaySinh > now) return fail('Ngày sinh không được ở tương lai.');
    const tuoi = tinhTuoi(ngaySinh, now);
    if (tuoi < TUOI_NHO_NHAT || tuoi > TUOI_LON_NHAT) {
      return fail(`Tuổi phải từ ${TUOI_NHO_NHAT} đến ${TUOI_LON_NHAT}.`);
    }
  });

export type PersonalInfoForm = z.infer<typeof personalInfoSchema>;
