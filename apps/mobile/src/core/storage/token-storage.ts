import { Platform } from 'react-native';
import * as SecureStore from 'expo-secure-store';

const KEY = 'gymbase.session.tokens';
const ACCESS_TOKEN_KEY = 'access_token';
const REFRESH_TOKEN_KEY = 'refresh_token';

export type Tokens = { accessToken: string; refreshToken: string };

function isWeb(): boolean {
  return (
    Platform.OS === 'web' ||
    (typeof window !== 'undefined' && typeof window.localStorage !== 'undefined')
  );
}

export const tokenStorage = {
  async read(): Promise<Tokens | null> {
    try {
      let raw: string | null = null;
      if (isWeb()) {
        raw = window.localStorage.getItem(KEY);
        if (!raw) {
          const at = window.localStorage.getItem(ACCESS_TOKEN_KEY);
          const rt = window.localStorage.getItem(REFRESH_TOKEN_KEY);
          if (at && rt) {
            return { accessToken: at, refreshToken: rt };
          }
        }
      } else {
        raw = await SecureStore.getItemAsync(KEY);
      }

      if (!raw) return null;
      const value: unknown = JSON.parse(raw);
      if (
        typeof value === 'object' &&
        value !== null &&
        'accessToken' in value &&
        typeof value.accessToken === 'string' &&
        'refreshToken' in value &&
        typeof value.refreshToken === 'string'
      ) {
        return { accessToken: value.accessToken, refreshToken: value.refreshToken };
      }
    } catch {
      /* Bỏ qua dữ liệu hỏng */
    }

    await tokenStorage.clear();
    return null;
  },

  async write(tokens: Tokens): Promise<void> {
    const raw = JSON.stringify(tokens);
    if (isWeb()) {
      try {
        window.localStorage.setItem(KEY, raw);
        window.localStorage.setItem(ACCESS_TOKEN_KEY, tokens.accessToken);
        window.localStorage.setItem(REFRESH_TOKEN_KEY, tokens.refreshToken);
      } catch {
        /* Bỏ qua lỗi hạn mức localStorage */
      }
    }

    try {
      await SecureStore.setItemAsync(KEY, raw);
    } catch {
      /* Bỏ qua nếu môi trường không hỗ trợ SecureStore */
    }
  },

  async clear(): Promise<void> {
    if (isWeb()) {
      try {
        window.localStorage.removeItem(KEY);
        window.localStorage.removeItem(ACCESS_TOKEN_KEY);
        window.localStorage.removeItem(REFRESH_TOKEN_KEY);
      } catch {
        /* ignore */
      }
    }

    try {
      await SecureStore.deleteItemAsync(KEY);
    } catch {
      /* ignore */
    }
  },
};
