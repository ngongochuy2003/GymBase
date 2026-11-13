/* eslint-disable @typescript-eslint/no-require-imports -- test nạp lại module sau khi đổi mock */
import { GoogleSignInUnavailableError, googleSignIn } from '@/core/auth/google-sign-in';
import { env } from '@/core/config/env';

/**
 * Ranh giới với thư viện native `@react-native-google-signin/google-signin`: thư viện được mock, nên kiểm được cách app gọi
 * nó (configure với web client ID, lấy ID token) và cách app đọc kết quả, hủy, lỗi, thiếu module native (Expo Go).
 */
const mockLibrary = {
  GoogleSignin: {
    configure: jest.fn(),
    hasPlayServices: jest.fn(async () => true),
    signIn: jest.fn(),
    signOut: jest.fn(async () => null),
  },
  statusCodes: {
    SIGN_IN_CANCELLED: 'SIGN_IN_CANCELLED',
    IN_PROGRESS: 'IN_PROGRESS',
    PLAY_SERVICES_NOT_AVAILABLE: 'PLAY_SERVICES_NOT_AVAILABLE',
  },
  isErrorWithCode: (error: unknown) =>
    typeof error === 'object' && error !== null && 'code' in error,
};

jest.mock('@react-native-google-signin/google-signin', () => mockLibrary);

const WEB_CLIENT_ID = 'web-client.apps.googleusercontent.com';

beforeEach(() => {
  env.googleWebClientId = WEB_CLIENT_ID;
  mockLibrary.GoogleSignin.hasPlayServices.mockResolvedValue(true);
});

const success = (idToken: string | null) => ({
  type: 'success',
  data: { idToken, user: { id: '1' } },
});

test('configures the mockLibrary with the web client ID and returns the ID token', async () => {
  mockLibrary.GoogleSignin.signIn.mockResolvedValue(success('google-id-token'));

  expect(await googleSignIn.getIdToken()).toBe('google-id-token');

  expect(mockLibrary.GoogleSignin.configure).toHaveBeenCalledWith({ webClientId: WEB_CLIENT_ID });
  expect(mockLibrary.GoogleSignin.hasPlayServices).toHaveBeenCalled();
});

test('a cancelled sign-in returns null', async () => {
  mockLibrary.GoogleSignin.signIn.mockResolvedValue({ type: 'cancelled', data: null });

  expect(await googleSignIn.getIdToken()).toBeNull();
});

test('a cancel reported as an error code also returns null', async () => {
  mockLibrary.GoogleSignin.signIn.mockRejectedValue({ code: 'SIGN_IN_CANCELLED' });

  expect(await googleSignIn.getIdToken()).toBeNull();
});

test('without a configured web client ID it refuses before touching the mockLibrary', async () => {
  env.googleWebClientId = undefined;

  await expect(googleSignIn.getIdToken()).rejects.toMatchObject({ reason: 'NOT_CONFIGURED' });
  expect(mockLibrary.GoogleSignin.signIn).not.toHaveBeenCalled();
});

test('missing Google Play Services is reported as its own reason', async () => {
  mockLibrary.GoogleSignin.hasPlayServices.mockRejectedValue({
    code: 'PLAY_SERVICES_NOT_AVAILABLE',
  });

  await expect(googleSignIn.getIdToken()).rejects.toBeInstanceOf(GoogleSignInUnavailableError);
  await expect(googleSignIn.getIdToken()).rejects.toMatchObject({
    reason: 'PLAY_SERVICES_UNAVAILABLE',
  });
});

test('a success without an ID token is an error, not a silent sign-in', async () => {
  mockLibrary.GoogleSignin.signIn.mockResolvedValue(success(null));

  await expect(googleSignIn.getIdToken()).rejects.toMatchObject({ reason: 'NO_ID_TOKEN' });
});

test('other mockLibrary errors are passed on', async () => {
  mockLibrary.GoogleSignin.signIn.mockRejectedValue(new Error('DEVELOPER_ERROR'));

  await expect(googleSignIn.getIdToken()).rejects.toThrow('DEVELOPER_ERROR');
});

test('signing out clears the Google account and never throws', async () => {
  await googleSignIn.signOut();
  expect(mockLibrary.GoogleSignin.signOut).toHaveBeenCalled();

  mockLibrary.GoogleSignin.signOut.mockRejectedValueOnce(new Error('network'));
  await expect(googleSignIn.signOut()).resolves.toBeUndefined();
});

test('when the native module is missing, as in Expo Go, it reports that instead of crashing', async () => {
  jest.resetModules();
  jest.doMock('@react-native-google-signin/google-signin', () => {
    throw new Error('RNGoogleSignin could not be found');
  });
  const { env: freshEnv } = require('@/core/config/env') as typeof import('@/core/config/env');
  freshEnv.googleWebClientId = WEB_CLIENT_ID;
  const fresh =
    require('@/core/auth/google-sign-in') as typeof import('@/core/auth/google-sign-in');

  await expect(fresh.googleSignIn.getIdToken()).rejects.toMatchObject({
    reason: 'NATIVE_MODULE_MISSING',
  });
  await expect(fresh.googleSignIn.signOut()).resolves.toBeUndefined();
});
