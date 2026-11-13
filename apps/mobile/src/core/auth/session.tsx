import { createContext, useContext, useEffect, useState, type PropsWithChildren } from 'react';
import axios from 'axios';
import { toApiError } from '../api/api-error';
import { tokenStorage } from '../storage/token-storage';
import { createMobHttpClient, resolveMobApiBaseUrl } from '../api/mobile-client';

export type Account = {
  id: string;
  ten_dang_nhap: string | null;
  ho_ten: string;
  email: string | null;
  so_dien_thoai: string | null;
  vai_tro: string;
  dang_hoat_dong: boolean;
};
type Session = {
  status: 'loading' | 'guest' | 'authenticated';
  user: Account | null;
  login(userName: string, password: string): Promise<void>;
  signOut(): Promise<void>;
};
const SessionContext = createContext<Session | null>(null);

export function SessionProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<Account | null>(null);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    let active = true;
    void (async () => {
      try {
        if (await tokenStorage.read()) {
          const response = await createMobHttpClient().get<Account>('/auth/me');
          if (active) setUser(response.data);
        }
      } catch {
        await tokenStorage.clear();
      } finally {
        if (active) setReady(true);
      }
    })();
    return () => { active = false; };
  }, []);

  const value: Session = {
    status: !ready ? 'loading' : user ? 'authenticated' : 'guest',
    user,
    async login(userName, password) {
      try {
        const response = await axios.post<{ access_token: string; refresh_token: string; user: Account }>(
          `${resolveMobApiBaseUrl()}/auth/login`,
          { ten_dang_nhap: userName, mat_khau: password, ten_thiet_bi: 'GymBase Mobile' },
          { timeout: 15_000 },
        );
        const result = response.data;
        await tokenStorage.write({ accessToken: result.access_token, refreshToken: result.refresh_token });
        setUser(result.user);
      } catch (error) {
        throw toApiError(error);
      }
    },
    async signOut() {
      try { await createMobHttpClient().post('/auth/logout'); } finally {
        setUser(null);
        await tokenStorage.clear();
      }
    },
  };
  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

export function useSession() {
  const session = useContext(SessionContext);
  if (!session) throw new Error('useSession requires SessionProvider');
  return session;
}

