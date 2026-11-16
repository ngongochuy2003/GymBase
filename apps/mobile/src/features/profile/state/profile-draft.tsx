import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react';

/**
 * Bản nháp thiết lập hồ sơ: gom dữ liệu các bước (thông tin cá nhân, rồi thông tin theo vai trò) để gửi
 * `PUT /ho-so` một lần ở bước cuối. Chỉ giữ trong bộ nhớ: thoát nhóm đã đăng nhập (đăng xuất) là mất.
 */
export type ProfileDraft = {
  ten_hien_thi: string;
  /** Giữ đúng chuỗi người dùng gõ (`dd/MM/yyyy`); đổi sang `yyyy-MM-dd` khi gửi. */
  ngay_sinh: string;
  gioi_tinh: string;
  khu_vuc_hoat_dong: string;
};

const EMPTY: ProfileDraft = {
  ten_hien_thi: '',
  ngay_sinh: '',
  gioi_tinh: '',
  khu_vuc_hoat_dong: '',
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
