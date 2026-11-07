import * as SecureStore from 'expo-secure-store';

const KEY = 'gymbase.session.tokens';
export type Tokens = { accessToken: string; refreshToken: string };

export const tokenStorage = {
  async read(): Promise<Tokens | null> {
    const raw = await SecureStore.getItemAsync(KEY);
    if (!raw) return null;
    try {
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
      /* Discard corrupt persisted data. */
    }
    await SecureStore.deleteItemAsync(KEY);
    return null;
  },
  async write(tokens: Tokens) {
    await SecureStore.setItemAsync(KEY, JSON.stringify(tokens));
  },
  async clear() {
    await SecureStore.deleteItemAsync(KEY);
  },
};
