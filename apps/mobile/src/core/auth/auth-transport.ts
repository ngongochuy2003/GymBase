import { ApiError } from '../api/api-error';
import type { AuthTransport } from '../api/client';
import type { Tokens } from '../storage/token-storage';
import type { AuthResult } from './types';

export type TokenStore = {
  read(): Promise<Tokens | null>;
  write(tokens: Tokens): Promise<void>;
  clear(): Promise<void>;
};

type Deps = {
  storage: TokenStore;
  /** Đổi refresh token lấy cặp token mới (gọi API, không qua interceptor). */
  refresh(refreshToken: string): Promise<AuthResult>;
};

/**
 * Giữ token trong bộ nhớ (đọc SecureStore một lần) và cung cấp `AuthTransport` cho `createApiClient`.
 * Refresh token bị server từ chối (401) thì xóa phiên; lỗi khác (mất mạng) giữ nguyên token và ném lỗi lên.
 */
export function createAuthTransport({ storage, refresh }: Deps) {
  let cache: Tokens | null | undefined;
  let onSignedOut: () => void = () => {};

  const load = async () => {
    if (cache === undefined) cache = await storage.read();
    return cache;
  };
  const save = async (tokens: Tokens) => {
    cache = tokens;
    await storage.write(tokens);
  };
  const clear = async () => {
    cache = null;
    await storage.clear();
  };

  const transport: AuthTransport = {
    async getAccessToken() {
      return (await load())?.accessToken ?? null;
    },
    async refresh() {
      const current = await load();
      if (!current) return null;
      try {
        const result = await refresh(current.refreshToken);
        await save({ accessToken: result.access_token, refreshToken: result.refresh_token });
        return result.access_token;
      } catch (error) {
        if (error instanceof ApiError && error.status === 401) {
          await clear();
          return null;
        }
        throw error;
      }
    },
    async onUnauthorized() {
      await clear();
      onSignedOut();
    },
  };

  return {
    transport,
    save,
    clear,
    /** Quên token trong bộ nhớ và đọc lại từ storage (khi khởi động app). */
    reload() {
      cache = undefined;
      return load();
    },
    /** Phiên gọi khi token bị xóa ngoài ý muốn, để chuyển về trạng thái khách. */
    setOnSignedOut(callback: () => void) {
      onSignedOut = callback;
    },
  };
}
