import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react';
import { authApi, authRuntime, type RegisterInput } from '../api/auth-api';
import { googleSignIn } from './google-sign-in';
import type { AuthResult, AuthUser } from './types';

type Session = {
  status: 'loading' | 'guest' | 'authenticated';
  user: AuthUser | null;
  /** Đăng nhập bằng email và mật khẩu; ném `ApiError` khi không thành công. */
  signIn(email: string, password: string): Promise<AuthResult>;
  /** Đăng nhập hoặc đăng ký bằng ID token của Google; ném `ApiError` khi không thành công. */
  signInWithGoogle(idToken: string): Promise<AuthResult>;
  /** Đăng ký bằng mật khẩu, có phiên ngay; ném `ApiError` khi không thành công. */
  register(input: RegisterInput): Promise<AuthResult>;
  /** True ngay sau khi tài khoản vừa được tạo, cho tới khi người dùng chọn thiết lập hồ sơ hoặc bỏ qua. */
  pendingWelcome: boolean;
  completeWelcome(): void;
  signOut(): Promise<void>;
};
const SessionContext = createContext<Session | null>(null);

export function SessionProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [ready, setReady] = useState(false);
  const [pendingWelcome, setPendingWelcome] = useState(false);

  useEffect(() => {
    let active = true;
    authRuntime.setOnSignedOut(() => {
      if (active) setUser(null);
    });
    void (async () => {
      try {
        const tokens = await authRuntime.reload();
        // Token còn nhưng không xác nhận được (hết phiên, mất mạng): vào màn khách, đăng nhập lại ghi đè token cũ.
        if (tokens) {
          const current = await authApi.me();
          if (active) setUser(current);
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

  /** Lưu token và chuyển sang trạng thái đã đăng nhập; `tai_khoan_moi` quyết định có hiện màn "Đăng ký thành công" không. */
  const startSession = useCallback(async (result: AuthResult) => {
    await authRuntime.save({
      accessToken: result.access_token,
      refreshToken: result.refresh_token,
    });
    setPendingWelcome(result.tai_khoan_moi);
    setUser(result.user);
    return result;
  }, []);

  const signIn = useCallback(
    async (email: string, password: string) =>
      startSession(await authApi.login({ email: email.trim().toLowerCase(), mat_khau: password })),
    [startSession],
  );

  const register = useCallback(
    async (input: RegisterInput) =>
      startSession(
        await authApi.register({
          ...input,
          ho_ten: input.ho_ten.trim(),
          email: input.email.trim().toLowerCase(),
          so_dien_thoai: input.so_dien_thoai.trim(),
        }),
      ),
    [startSession],
  );

  const signInWithGoogle = useCallback(
    async (idToken: string) => startSession(await authApi.google({ id_token: idToken })),
    [startSession],
  );

  const completeWelcome = useCallback(() => setPendingWelcome(false), []);

  const signOut = useCallback(async () => {
    try {
      // Thu hồi phiên trên server khi còn token; mất mạng thì vẫn đăng xuất cục bộ.
      await authApi.logout().catch(() => undefined);
    } finally {
      setUser(null);
      setPendingWelcome(false);
      await authRuntime.clear();
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
      pendingWelcome,
      completeWelcome,
      signOut,
    }),
    [ready, user, signIn, signInWithGoogle, register, pendingWelcome, completeWelcome, signOut],
  );
  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

export function useSession() {
  const session = useContext(SessionContext);
  if (!session) throw new Error('useSession requires SessionProvider');
  return session;
}
