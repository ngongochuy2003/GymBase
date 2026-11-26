import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react';
import { ApiError } from '../api/api-error';
import { authApi, authRuntime, type RegisterInput } from '../api/auth-api';
import { xoaCacheAnh } from '@/shared/file/image-cache';
import { huyTatCaNhacGioAsync } from '@/shared/notifications/nhac-gio';
import { goThietBiAsync } from '@/shared/notifications/thiet-bi';
import { userStorage } from '../storage/user-storage';
import { googleSignIn } from './google-sign-in';
import type { AuthResult, AuthUser } from './types';

type Session = {
  status: 'loading' | 'guest' | 'authenticated';
  user: AuthUser | null;
  /** Đăng nhập bằng email và mật khẩu; ném `ApiError` khi không thành công. */
  signIn(email: string, password: string): Promise<AuthResult>;
  /** Đăng nhập hoặc đăng ký bằng ID token của Google; ném `ApiError` khi không thành công. */
  signInWithGoogle(idToken: string): Promise<AuthResult>;
  /** Đăng ký bằng mật khẩu, không có phiên (người dùng đăng nhập sau); ném `ApiError` khi không thành công. */
  register(input: RegisterInput): Promise<AuthUser>;
  /** Người dùng bấm một trong hai nút của màn onboarding: ghi lên server rồi tắt cờ lần đầu của phiên. */
  completeOnboarding(): Promise<void>;
  signOut(): Promise<void>;
};
const SessionContext = createContext<Session | null>(null);

export function SessionProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    let active = true;
    authRuntime.setOnSignedOut(() => {
      if (active) {
        void userStorage.clear();
        setUser(null);
      }
    });
    void (async () => {
      try {
        const tokens = await authRuntime.reload();
        if (tokens) {
          const cachedUser = await userStorage.read();
          if (cachedUser && active) {
            setUser(cachedUser);
          }
          try {
            const current = await authApi.me();
            if (active) {
              setUser(current);
              await userStorage.write(current);
            }
          } catch (err: unknown) {
            // Client thường đã tự xử lý 401 (refresh, rồi xóa phiên); giữ nhánh này làm lớp bảo vệ.
            if (err instanceof ApiError && err.status === 401) {
              await authRuntime.clear();
              await userStorage.clear();
              if (active) setUser(null);
            }
            /* Lỗi mạng tạm thời, nếu đã có cachedUser thì giữ phiên */
          }
        }
      } catch {
        /* Không khôi phục được phiên: coi như khách. */
      } finally {
        if (active) setReady(true);
      }
    })();

    return () => {
      active = false;
      authRuntime.setOnSignedOut(() => {});
    };
  }, []);

  /** Lưu token và chuyển sang trạng thái đã đăng nhập; `user.is_dang_nhap_lan_dau` quyết định có hiện onboarding không. */
  const startSession = useCallback(async (result: AuthResult) => {
    await authRuntime.save({
      accessToken: result.access_token,
      refreshToken: result.refresh_token,
    });
    setUser(result.user);
    await userStorage.write(result.user);
    return result;
  }, []);

  const signIn = useCallback(
    async (email: string, password: string) =>
      startSession(await authApi.login({ email: email.trim().toLowerCase(), mat_khau: password })),
    [startSession],
  );

  const register = useCallback(
    async (input: RegisterInput) =>
      authApi.register({
        ...input,
        ho_ten: input.ho_ten.trim(),
        email: input.email.trim().toLowerCase(),
        ...(input.so_dien_thoai ? { so_dien_thoai: input.so_dien_thoai.trim() } : {}),
      }),
    [],
  );

  const signInWithGoogle = useCallback(
    async (idToken: string) => startSession(await authApi.google({ id_token: idToken })),
    [startSession],
  );

  const completeOnboarding = useCallback(async () => {
    try {
      const updated = await authApi.hoanTatDangNhapLanDau();
      setUser(updated);
      await userStorage.write(updated);
    } catch {
      // Không chặn người dùng khi mất mạng: vào app bình thường, lần mở sau server vẫn báo lần đầu nên onboarding hiện lại.
      setUser((current) => (current ? { ...current, is_dang_nhap_lan_dau: false } : current));
    }
  }, []);

  const signOut = useCallback(async () => {
    try {
      // Máy dùng chung: hủy các nhắc ăn, nhắc tập đã đặt trên máy để người kế tiếp không nhận nhắc theo lịch của người trước.
      await huyTatCaNhacGioAsync().catch(() => undefined);
      // Gỡ token push của máy này khi còn phiên, để người dùng kế tiếp không nhận thông báo của người trước; mất mạng thì bỏ qua.
      await goThietBiAsync().catch(() => undefined);
      // Thu hồi phiên trên server khi còn token; mất mạng thì vẫn đăng xuất cục bộ.
      await authApi.logout().catch(() => undefined);
    } finally {
      setUser(null);
      await authRuntime.clear();
      await userStorage.clear();
      // Máy dùng chung: không để ảnh riêng tư của người vừa đăng xuất nằm lại trong cache.
      await xoaCacheAnh();
      await googleSignIn.signOut();
    }
  }, []);

  const value = useMemo<Session>(
    () => ({
      status: !ready ? 'loading' : user ? 'authenticated' : 'guest',
      user,
      signIn,
      signInWithGoogle,
      register,
      completeOnboarding,
      signOut,
    }),
    [ready, user, signIn, signInWithGoogle, register, completeOnboarding, signOut],
  );
  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

export function useSession() {
  const session = useContext(SessionContext);
  if (!session) throw new Error('useSession requires SessionProvider');
  return session;
}

export function useOptionalSession() {
  return useContext(SessionContext);
}
