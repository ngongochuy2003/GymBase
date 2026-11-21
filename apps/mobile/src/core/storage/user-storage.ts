import { Platform } from 'react-native';
import * as SecureStore from 'expo-secure-store';
import type { AuthUser } from '../auth/types';

const USER_KEY = 'gymbase.session.user';

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
        raw = window.localStorage.getItem(USER_KEY);
      } else {
        raw = await SecureStore.getItemAsync(USER_KEY);
      }
      if (!raw) return null;
      const value: unknown = JSON.parse(raw);
      if (
        typeof value === 'object' &&
        value !== null &&
        'id' in value &&
        typeof value.id === 'string' &&
        'ho_ten' in value &&
        typeof value.ho_ten === 'string'
      ) {
        return value as AuthUser;
      }
      return null;
    } catch {
      return null;
    }
  },

  async write(user: AuthUser): Promise<void> {
    const raw = JSON.stringify(user);
    if (isWeb()) {
      try {
        window.localStorage.setItem(USER_KEY, raw);
      } catch {
        /* Lỗi lưu trữ: bỏ qua, người dùng vẫn dùng được trong phiên này. */
      }
    }

    try {
      await SecureStore.setItemAsync(USER_KEY, raw);
    } catch {
      /* Lỗi lưu trữ: bỏ qua, người dùng vẫn dùng được trong phiên này. */
    }
  },

  async clear(): Promise<void> {
    if (isWeb()) {
      try {
        window.localStorage.removeItem(USER_KEY);
      } catch {
        /* Lỗi lưu trữ: bỏ qua, người dùng vẫn dùng được trong phiên này. */
      }
    }

    try {
      await SecureStore.deleteItemAsync(USER_KEY);
    } catch {
      /* Lỗi lưu trữ: bỏ qua, người dùng vẫn dùng được trong phiên này. */
    }
  },
};
