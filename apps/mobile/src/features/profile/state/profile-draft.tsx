import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react';
import type { ChungChiPt } from '../api/profile-api';

/**
 * Bản nháp thiết lập hồ sơ: gom dữ liệu các bước (thông tin cá nhân, rồi thông tin theo vai trò) để gửi
 * `PUT /ho-so` một lần ở bước cuối (màn 03B). Chỉ giữ trong bộ nhớ: thoát nhóm đã đăng nhập (đăng xuất) là mất.
 */
export type ProfileDraft = {
  ten_hien_thi: string;
  /** Giữ đúng chuỗi người dùng gõ (`dd/MM/yyyy`); đổi sang `yyyy-MM-dd` khi gửi. */
  ngay_sinh: string;
  gioi_tinh: string;
  khu_vuc_hoat_dong: string;
  // Khối học viên (màn 03B). Chiều cao và cân nặng giữ đúng chuỗi người dùng gõ.
  muc_tieu_tap_luyen: string;
  trinh_do_tap: string;
  chieu_cao_cm: string;
  can_nang_kg: string;
  // Khối PT (màn 03B).
  chuyen_mon: string[];
  so_nam_kinh_nghiem: string;
  gioi_thieu: string;
  /** Chứng chỉ PDF đã tải lên (tùy chọn). */
  chung_chi: ChungChiPt | null;
};

const EMPTY: ProfileDraft = {
  ten_hien_thi: '',
  ngay_sinh: '',
  gioi_tinh: '',
  khu_vuc_hoat_dong: '',
  muc_tieu_tap_luyen: '',
  trinh_do_tap: '',
  chieu_cao_cm: '',
  can_nang_kg: '',
  chuyen_mon: [],
  so_nam_kinh_nghiem: '',
  gioi_thieu: '',
  chung_chi: null,
};

type ProfileDraftContext = {
  draft: ProfileDraft;
  update(patch: Partial<ProfileDraft>): void;
  reset(): void;
};

const Context = createContext<ProfileDraftContext | null>(null);

export function ProfileDraftProvider({ children }: PropsWithChildren) {
  const [draft, setDraft] = useState<ProfileDraft>(EMPTY);
  const update = useCallback(
    (patch: Partial<ProfileDraft>) => setDraft((current) => ({ ...current, ...patch })),
    [],
  );
  const reset = useCallback(() => setDraft(EMPTY), []);
  const value = useMemo(() => ({ draft, update, reset }), [draft, update, reset]);
  return <Context.Provider value={value}>{children}</Context.Provider>;
}

export function useProfileDraft() {
  const context = useContext(Context);
  if (!context) throw new Error('useProfileDraft requires ProfileDraftProvider');
  return context;
}
