import { ApiError } from '@/core/api/api-error';
import { createAuthTransport } from '@/core/auth/auth-transport';
import type { AuthResult, AuthUser } from '@/core/auth/types';
import type { Tokens } from '@/core/storage/token-storage';

const user = { id: '1', email: 'a@gymbase.local', ho_ten: 'A', vai_tro: 'User' } as AuthUser;
const refreshed: AuthResult = {
  access_token: 'access-2',
  expires_in: 900,
  refresh_token: 'refresh-2',
  user,
  tai_khoan_moi: false,
};

function setup(initial: Tokens | null, refresh = jest.fn(async () => refreshed)) {
  const storage = {
    read: jest.fn(async () => initial),
    write: jest.fn(async () => undefined),
    clear: jest.fn(async () => undefined),
  };
  const onSignedOut = jest.fn();
  const auth = createAuthTransport({ storage, refresh });
  auth.setOnSignedOut(onSignedOut);
  return { auth, storage, refresh, onSignedOut };
}

const tokens: Tokens = { accessToken: 'access-1', refreshToken: 'refresh-1' };

test('reads the access token from storage once and then from memory', async () => {
  const { auth, storage } = setup(tokens);

  expect(await auth.transport.getAccessToken()).toBe('access-1');
  expect(await auth.transport.getAccessToken()).toBe('access-1');

  expect(storage.read).toHaveBeenCalledTimes(1);
});

test('has no access token when nothing is stored', async () => {
  const { auth } = setup(null);

  expect(await auth.transport.getAccessToken()).toBeNull();
  expect(await auth.transport.refresh()).toBeNull();
});

test('refresh rotates the stored tokens and returns the new access token', async () => {
  const { auth, storage, refresh } = setup(tokens);

  expect(await auth.transport.refresh()).toBe('access-2');

  expect(refresh).toHaveBeenCalledWith('refresh-1');
  expect(storage.write).toHaveBeenCalledWith({
    accessToken: 'access-2',
    refreshToken: 'refresh-2',
  });
  expect(await auth.transport.getAccessToken()).toBe('access-2');
});

test('a rejected refresh token clears the session and reports no token', async () => {
  const refresh = jest.fn(async () => {
    throw new ApiError('Hết phiên.', 401, 'UNAUTHORIZED');
  });
  const { auth, storage } = setup(tokens, refresh);

  expect(await auth.transport.refresh()).toBeNull();

  expect(storage.clear).toHaveBeenCalled();
  expect(await auth.transport.getAccessToken()).toBeNull();
});

test('a refresh that fails for another reason keeps the tokens and surfaces the error', async () => {
  const refresh = jest.fn(async () => {
    throw new ApiError('Không thể kết nối.', undefined, 'NETWORK');
  });
  const { auth, storage } = setup(tokens, refresh);

  await expect(auth.transport.refresh()).rejects.toMatchObject({ code: 'NETWORK' });

  expect(storage.clear).not.toHaveBeenCalled();
  expect(await auth.transport.getAccessToken()).toBe('access-1');
});

test('onUnauthorized clears the tokens and tells the session it is signed out', async () => {
  const { auth, storage, onSignedOut } = setup(tokens);

  await auth.transport.onUnauthorized();

  expect(storage.clear).toHaveBeenCalled();
  expect(onSignedOut).toHaveBeenCalledTimes(1);
  expect(await auth.transport.getAccessToken()).toBeNull();
});

test('reload forgets the in-memory tokens and reads storage again', async () => {
  const { auth, storage } = setup(tokens);
  await auth.transport.getAccessToken();

  await auth.reload();

  expect(storage.read).toHaveBeenCalledTimes(2);
});
