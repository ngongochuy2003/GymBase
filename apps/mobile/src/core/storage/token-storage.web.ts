export type Tokens = { accessToken: string; refreshToken: string };

const KEY = 'gymbase.session.tokens';

let memoryCache: Tokens | null = null;

function getLocalStorage(): Storage | null {
  if (typeof window !== 'undefined' && typeof window.localStorage !== 'undefined') {
    return window.localStorage;
  }
  return null;
}

export const tokenStorage = {
  async read(): Promise<Tokens | null> {
    if (memoryCache) {
      return { ...memoryCache };
    }

    const storage = getLocalStorage();
    if (!storage) return null;

    try {
      const raw = storage.getItem(KEY);
      if (raw) {
        const parsed = JSON.parse(raw) as Partial<Tokens>;
        if (typeof parsed?.accessToken === 'string' && typeof parsed?.refreshToken === 'string') {
          memoryCache = { accessToken: parsed.accessToken, refreshToken: parsed.refreshToken };
          return { ...memoryCache };
        }
      }

    } catch {
      // Bỏ qua lỗi cú pháp JSON
    }

    return null;
  },

  async write(tokens: Tokens): Promise<void> {
    memoryCache = { ...tokens };
    const storage = getLocalStorage();
    if (!storage) return;

    try {
      storage.setItem(KEY, JSON.stringify(tokens));
    } catch {
      // Bỏ qua lỗi hạn mức lưu trữ
    }
  },

  async clear(): Promise<void> {
    memoryCache = null;
    const storage = getLocalStorage();
    if (!storage) return;

    try {
      storage.removeItem(KEY);
    } catch {
      // Không có gì để xóa hoặc không xóa được: bỏ qua.
    }
  },
};
