import { z } from 'zod';
import type { VaiTro } from '../types/vai-tro';

/** Khớp luật của backend (`HoSoService`). */
export const CHIEU_CAO = { min: 100, max: 250 } as const;
export const CAN_NANG = { min: 20, max: 300 } as const;
export const KINH_NGHIEM_TOI_DA = 60;
export const CHUYEN_MON_TOI_DA = 10;
export const GIOI_THIEU_TOI_DA = 1000;

/** Vai trò `hoc_vien` và `ca_hai` có khối học viên. */
export const coKhoiHocVien = (vaiTro: VaiTro) => vaiTro !== 'pt';
/** Vai trò `pt` và `ca_hai` có khối PT. */
export const coKhoiPt = (vaiTro: VaiTro) => vaiTro !== 'hoc_vien';

/** Đọc số người dùng gõ; chấp nhận dấu phẩy thập phân (`68,5`). NaN nếu không phải số. */
export function parseSo(text: string): number {
  const normalized = text.trim().replace(',', '.');
  return normalized === '' ? Number.NaN : Number(normalized);
}

const capitalize = (text: string) => text.charAt(0).toUpperCase() + text.slice(1);

function addNumberRules(
  ctx: z.RefinementCtx,
  path: string,
  text: string,
  rule: { tenTruong: string; donVi: string; min: number; max: number; nguyen?: boolean },
) {
  const fail = (message: string) => ctx.addIssue({ code: 'custom', path: [path], message });
  const { tenTruong, donVi, min, max, nguyen } = rule;
  if (text.trim() === '') return fail(`Vui lòng nhập ${tenTruong}.`);
  const value = parseSo(text);
  if (!Number.isFinite(value)) return fail(`${capitalize(tenTruong)} phải là số.`);
  if (nguyen && !Number.isInteger(value))
    return fail(`${capitalize(tenTruong)} phải là số nguyên.`);
  if (value < min || value > max) {
    const range = donVi ? `${min}–${max} ${donVi}` : `${min}–${max}`;
    return fail(`${capitalize(tenTruong)} phải trong khoảng ${range}.`);
  }
}

/**
 * Form màn 03B. Chỉ khối thuộc vai trò mới được kiểm tra; khối còn lại bỏ qua (ô không hiện, API không nhận).
 * Chiều cao, cân nặng, số năm giữ dạng chuỗi như người dùng gõ, đổi sang số khi gửi.
 */
export function createProfileDetailSchema(vaiTro: VaiTro) {
  return z
    .object({
      muc_tieu_tap_luyen: z.string(),
      trinh_do_tap: z.string(),
      chieu_cao_cm: z.string(),
      can_nang_kg: z.string(),
      chuyen_mon: z.array(z.string()),
      so_nam_kinh_nghiem: z.string(),
      gioi_thieu: z.string(),
    })
    .superRefine((form, ctx) => {
      const fail = (path: string, message: string) =>
        ctx.addIssue({ code: 'custom', path: [path], message });
      if (coKhoiHocVien(vaiTro)) {
        if (!form.muc_tieu_tap_luyen)
          fail('muc_tieu_tap_luyen', 'Vui lòng chọn mục tiêu tập luyện.');
        if (!form.trinh_do_tap) fail('trinh_do_tap', 'Vui lòng chọn trình độ tập.');
        addNumberRules(ctx, 'chieu_cao_cm', form.chieu_cao_cm, {
          tenTruong: 'chiều cao',
          donVi: 'cm',
          ...CHIEU_CAO,
        });
        addNumberRules(ctx, 'can_nang_kg', form.can_nang_kg, {
          tenTruong: 'cân nặng',
          donVi: 'kg',
          ...CAN_NANG,
        });
      }
      if (coKhoiPt(vaiTro)) {
        if (form.chuyen_mon.length === 0) fail('chuyen_mon', 'Vui lòng chọn ít nhất 1 chuyên môn.');
        if (form.chuyen_mon.length > CHUYEN_MON_TOI_DA) {
          fail('chuyen_mon', `Chọn tối đa ${CHUYEN_MON_TOI_DA} chuyên môn.`);
        }
        addNumberRules(ctx, 'so_nam_kinh_nghiem', form.so_nam_kinh_nghiem, {
          tenTruong: 'số năm kinh nghiệm',
          donVi: '',
          min: 0,
          max: KINH_NGHIEM_TOI_DA,
          nguyen: true,
        });
        if (form.gioi_thieu.length > GIOI_THIEU_TOI_DA) {
          fail('gioi_thieu', `Giới thiệu tối đa ${GIOI_THIEU_TOI_DA} ký tự.`);
        }
      }
    });
}

export type ProfileDetailForm = z.infer<ReturnType<typeof createProfileDetailSchema>>;

/** Tên ô thuộc từng khối, để biết khối nào đang có lỗi. */
export const HOC_VIEN_FIELDS = [
  'muc_tieu_tap_luyen',
  'trinh_do_tap',
  'chieu_cao_cm',
  'can_nang_kg',
] as const;
export const PT_FIELDS = ['chuyen_mon', 'so_nam_kinh_nghiem', 'gioi_thieu'] as const;
