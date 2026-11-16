import { Platform } from 'react-native';
import * as SecureStore from 'expo-secure-store';
import type { AuthUser } from '../auth/types';

const USER_KEY = 'gymbase.session.user';
const USER_KEY_SHORT = 'user';

function isWeb(): boolean {
  return (
    Platform.OS === 'web' ||
    (typeof window !== 'undefined' && typeof window.localStorage !== 'undefined')
  );
}

export const userStorage = {
  async read(): Promise<AuthUser | null> {
    try {
      let raw: string | null = null;
      if (isWeb()) {
        raw = window.localStorage.getItem(USER_KEY) || window.localStorage.getItem(USER_KEY_SHORT);
      } else {
        raw = await SecureStore.getItemAsync(USER_KEY);
      }
      if (!raw) return null;
      return JSON.parse(raw) as AuthUser;
    } catch {
      return null;
    }
  },

  async write(user: AuthUser): Promise<void> {
    const raw = JSON.stringify(user);
    if (isWeb()) {
      try {
        window.localStorage.setItem(USER_KEY, raw);
        window.localStorage.setItem(USER_KEY_SHORT, raw);
      } catch {
        /* ignore */
      }
    }

    try {
      await SecureStore.setItemAsync(USER_KEY, raw);
    } catch {
      /* ignore */
    }
  },

  async clear(): Promise<void> {
    if (isWeb()) {
      try {
        window.localStorage.removeItem(USER_KEY);
        window.localStorage.removeItem(USER_KEY_SHORT);
      } catch {
        /* ignore */
      }
    }

    try {
      await SecureStore.deleteItemAsync(USER_KEY);
    } catch {
      /* ignore */
    }
  },
};
