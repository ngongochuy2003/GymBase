import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react';
import { authApi, authRuntime } from '../api/auth-api';
import type { AuthResult, AuthUser } from './types';

type Session = {
  status: 'loading' | 'guest' | 'authenticated';
  user: AuthUser | null;
  /** Đăng nhập bằng email và mật khẩu; ném `ApiError` khi không thành công. */
  signIn(email: string, password: string): Promise<AuthResult>;
  signOut(): Promise<void>;
};
const SessionContext = createContext<Session | null>(null);

export function SessionProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [ready, setReady] = useState(false);

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

  const signIn = useCallback(async (email: string, password: string) => {
    const result = await authApi.login({
      email: email.trim().toLowerCase(),
      mat_khau: password,
    });
    await authRuntime.save({
      accessToken: result.access_token,
      refreshToken: result.refresh_token,
    });
    setUser(result.user);
    return result;
  }, []);

  const signOut = useCallback(async () => {
    try {
      // Thu hồi phiên trên server khi còn token; mất mạng thì vẫn đăng xuất cục bộ.
      await authApi.logout().catch(() => undefined);
    } finally {
      setUser(null);
      await authRuntime.clear();
    }
  }, []);

  const value = useMemo<Session>(
    () => ({
      status: !ready ? 'loading' : user ? 'authenticated' : 'guest',
      user,
      signIn,
      signOut,
    }),
    [ready, user, signIn, signOut],
  );
  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

export function useSession() {
  const session = useContext(SessionContext);
  if (!session) throw new Error('useSession requires SessionProvider');
  return session;
}
