// Web preview never persists credentials in localStorage. Real web auth needs
// its own server-managed cookie/BFF design when the backend contract is ready.
type Tokens = { accessToken: string; refreshToken: string };
let current: Tokens | null = null;
export const tokenStorage = {
  async read(): Promise<Tokens | null> {
    return current ? { ...current } : null;
  },
  async write(tokens: Tokens) {
    current = { ...tokens };
  },
  async clear() {
    current = null;
  },
};
