import * as SecureStore from 'expo-secure-store';
import { tokenStorage } from '@/core/storage/token-storage';
import { parseEnv } from '@/core/config/env';

test('allows backend-free demo but requires valid URL for API mode', () => {
  expect(parseEnv({}).dataMode).toBe('demo');
  expect(() => parseEnv({ dataMode: 'api' })).toThrow('EXPO_PUBLIC_API_URL');
  expect(() => parseEnv({ apiUrl: 'not-a-url' })).toThrow();
});

test('stores tokens in native secure storage and clears them', async () => {
  const tokens = { accessToken: 'test-access', refreshToken: 'test-refresh' };
  await tokenStorage.write(tokens);
  expect(SecureStore.setItemAsync).toHaveBeenCalledWith(
    'gymbase.session.tokens',
    JSON.stringify(tokens),
  );
  jest.mocked(SecureStore.getItemAsync).mockResolvedValueOnce(JSON.stringify(tokens));
  expect(await tokenStorage.read()).toEqual(tokens);
  await tokenStorage.clear();
  expect(SecureStore.deleteItemAsync).toHaveBeenCalledWith('gymbase.session.tokens');
});

test('discards malformed persisted tokens', async () => {
  jest.mocked(SecureStore.getItemAsync).mockResolvedValueOnce('{broken json');
  expect(await tokenStorage.read()).toBeNull();
  expect(SecureStore.deleteItemAsync).toHaveBeenCalledWith('gymbase.session.tokens');
});
