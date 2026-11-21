import * as SecureStore from 'expo-secure-store';
import type { Tokens } from './types';

// Bản native (Keychain/Keystore). Web dùng `token-storage.web.ts`, Metro tự chọn theo nền tảng.
const KEY = 'gymbase.session.tokens';

export type { Tokens };

export const tokenStorage = {
  async read(): Promise<Tokens | null> {
    try {
      const raw = await SecureStore.getItemAsync(KEY);
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
      /* Dữ liệu hỏng: xóa bên dưới. */
    }

    await tokenStorage.clear();
    return null;
  },

  /** Ném lỗi khi không lưu được, để đăng nhập báo thất bại thay vì để phiên mất âm thầm ở lần mở sau. */
  async write(tokens: Tokens): Promise<void> {
    await SecureStore.setItemAsync(KEY, JSON.stringify(tokens));
  },

  async clear(): Promise<void> {
    try {
      await SecureStore.deleteItemAsync(KEY);
    } catch {
      /* Không có gì để xóa. */
    }
  },
};
