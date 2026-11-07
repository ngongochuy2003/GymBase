import { createContext, useContext, useEffect, useState, type PropsWithChildren } from 'react';
import { env } from '../config/env';
import { tokenStorage } from '../storage/token-storage';

export type DemoUser = { name: string; mode: 'demo' };
type Session = {
  status: 'loading' | 'guest' | 'demo';
  user: DemoUser | null;
  enterDemo(name: string): void;
  signOut(): Promise<void>;
};
const SessionContext = createContext<Session | null>(null);

export function SessionProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<DemoUser | null>(null);
  const [ready, setReady] = useState(false);
  useEffect(() => {
    let active = true;
    // Demo sessions are intentionally ephemeral. Replace this bootstrap with
    // the verified backend session adapter when authentication is integrated.
    Promise.resolve().then(() => {
      if (active) setReady(true);
    });
    return () => {
      active = false;
    };
  }, []);

  const value: Session = {
    status: !ready ? 'loading' : user ? 'demo' : 'guest',
    user,
    enterDemo(name) {
      if (env.dataMode !== 'demo') throw new Error('Demo is disabled in API mode.');
      setUser({ name: name.trim(), mode: 'demo' });
    },
    async signOut() {
      // Remove access immediately, even if native secure storage fails.
      setUser(null);
      await tokenStorage.clear();
    },
  };
  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

export function useSession() {
  const session = useContext(SessionContext);
  if (!session) throw new Error('useSession requires SessionProvider');
  return session;
}
