import * as SecureStore from 'expo-secure-store';
import { fireEvent, renderRouter, screen, waitFor } from 'expo-router/testing-library';
import * as RootLayout from '@/app/_layout';
import EntryRoute from '@/app/index';
import AuthLayout from '@/app/(auth)/_layout';
import WelcomeRoute from '@/app/(auth)/welcome';
import LoginRoute from '@/app/(auth)/login';
import ProtectedLayout from '@/app/(protected)/_layout';
import TodayRoute from '@/app/(protected)/today';
import ProfileRoute from '@/app/(protected)/profile';
import ProfileSetupRoute from '@/app/(protected)/profile-setup';
import RegisterSuccessRoute from '@/app/(protected)/register-success';
import NotFound from '@/app/+not-found';
import { ApiError } from '@/core/api/api-error';
import { authApi } from '@/core/api/auth-api';
import { GoogleSignInUnavailableError, googleSignIn } from '@/core/auth/google-sign-in';
import type { AuthResult, AuthUser } from '@/core/auth/types';

const user: AuthUser = {
  id: '0193f0a0-0000-7000-8000-000000000020',
  ten_dang_nhap: null,
  ho_ten: 'Người Google',
  email: 'nguoi.google@gmail.test',
  so_dien_thoai: null,
  vai_tro: 'User',
  dang_hoat_dong: true,
  ngay_tao: '2026-11-13T00:00:00Z',
  ngay_chinh_sua: null,
};
const result = (isNew: boolean): AuthResult => ({
  access_token: 'access-google',
  expires_in: 900,
  refresh_token: 'refresh-google',
  user,
  tai_khoan_moi: isNew,
});

const routes = {
  _layout: RootLayout,
  index: EntryRoute,
  '(auth)/_layout': AuthLayout,
  '(auth)/welcome': WelcomeRoute,
  '(auth)/login': LoginRoute,
  '(protected)/_layout': ProtectedLayout,
  '(protected)/today': TodayRoute,
  '(protected)/profile': ProfileRoute,
  '(protected)/profile-setup': ProfileSetupRoute,
  '(protected)/register-success': RegisterSuccessRoute,
  '+not-found': NotFound,
};

const GOOGLE = { name: 'Đăng nhập với Google' };

async function openWelcome() {
  await renderRouter(routes, { initialUrl: '/welcome' });
  expect(await screen.findByRole('button', GOOGLE)).toBeTruthy();
}

beforeEach(() => {
  jest.spyOn(googleSignIn, 'getIdToken').mockResolvedValue('google-id-token');
  jest.spyOn(googleSignIn, 'signOut').mockResolvedValue(undefined);
});

afterEach(() => {
  jest.restoreAllMocks();
});

test('first Google sign-in stores the session, shows the success screen once, then the app', async () => {
  const google = jest.spyOn(authApi, 'google').mockResolvedValue(result(true));
  await openWelcome();

  await fireEvent.press(screen.getByRole('button', GOOGLE));

  await waitFor(() => expect(screen).toHavePathname('/register-success'));
  expect(await screen.findByText('Đăng ký thành công!')).toBeTruthy();
  expect(google).toHaveBeenCalledWith({ id_token: 'google-id-token' });
  expect(SecureStore.setItemAsync).toHaveBeenCalledWith(
    'gymbase.session.tokens',
    JSON.stringify({ accessToken: 'access-google', refreshToken: 'refresh-google' }),
  );

  await fireEvent.press(screen.getByRole('button', { name: 'Bỏ qua, vào Trang chủ' }));
  await waitFor(() => expect(screen).toHavePathname('/today'));
  expect(await screen.findByText('Chào Người Google,')).toBeTruthy();
});

test('a returning Google user goes straight to the app', async () => {
  jest.spyOn(authApi, 'google').mockResolvedValue(result(false));
  await openWelcome();

  await fireEvent.press(screen.getByRole('button', GOOGLE));

  await waitFor(() => expect(screen).toHavePathname('/today'));
  expect(screen.queryByText('Đăng ký thành công!')).toBeNull();
});

test('the Google button on the login screen works too', async () => {
  const google = jest.spyOn(authApi, 'google').mockResolvedValue(result(false));
  await renderRouter(routes, { initialUrl: '/login' });

  await fireEvent.press(await screen.findByRole('button', GOOGLE));

  await waitFor(() => expect(screen).toHavePathname('/today'));
  expect(google).toHaveBeenCalledWith({ id_token: 'google-id-token' });
});

test('cancelling the account chooser stays on the screen without an error or an API call', async () => {
  jest.mocked(googleSignIn.getIdToken).mockResolvedValue(null);
  const google = jest.spyOn(authApi, 'google');
  await openWelcome();

  await fireEvent.press(screen.getByRole('button', GOOGLE));

  await waitFor(() => expect(googleSignIn.getIdToken).toHaveBeenCalled());
  expect(google).not.toHaveBeenCalled();
  expect(screen).toHavePathname('/welcome');
  expect(screen.queryByRole('alert')).toBeNull();
});

test.each([
  ['NOT_CONFIGURED', 'Đăng nhập Google chưa được cấu hình trên bản này.'],
  ['NATIVE_MODULE_MISSING', 'Đăng nhập Google cần bản dev build, không chạy trong Expo Go.'],
  ['PLAY_SERVICES_UNAVAILABLE', 'Thiết bị này chưa có Google Play Services.'],
  ['NO_ID_TOKEN', 'Không lấy được thông tin từ Google. Vui lòng thử lại.'],
] as const)(
  'when Google sign-in is unavailable (%s) the user sees a clear message',
  async (reason, message) => {
    jest
      .mocked(googleSignIn.getIdToken)
      .mockRejectedValue(new GoogleSignInUnavailableError(reason));
    await openWelcome();

    await fireEvent.press(screen.getByRole('button', GOOGLE));

    expect(await screen.findByText(message)).toBeTruthy();
    expect(screen).toHavePathname('/welcome');
  },
);

test('an email already used with a password tells the user to sign in with the password', async () => {
  jest
    .spyOn(authApi, 'google')
    .mockRejectedValue(
      new ApiError('Email này đã đăng ký bằng mật khẩu.', 409, 'AUTH_EMAIL_USED_PASSWORD'),
    );
  await openWelcome();

  await fireEvent.press(screen.getByRole('button', GOOGLE));

  expect(
    await screen.findByText(
      'Email này đã đăng ký bằng mật khẩu. Hãy đăng nhập bằng email và mật khẩu.',
    ),
  ).toBeTruthy();
  expect(screen).toHavePathname('/welcome');
});

test('a rejected ID token shows the server message', async () => {
  jest
    .spyOn(authApi, 'google')
    .mockRejectedValue(
      new ApiError(
        'Không xác minh được tài khoản Google. Vui lòng thử lại.',
        401,
        'AUTH_GOOGLE_TOKEN_INVALID',
      ),
    );
  await openWelcome();

  await fireEvent.press(screen.getByRole('button', GOOGLE));

  expect(
    await screen.findByText('Không xác minh được tài khoản Google. Vui lòng thử lại.'),
  ).toBeTruthy();
});

test('after a network failure pressing the button again signs in', async () => {
  const google = jest
    .spyOn(authApi, 'google')
    .mockRejectedValueOnce(
      new ApiError('Không thể kết nối. Kiểm tra mạng và thử lại.', undefined, 'NETWORK'),
    )
    .mockResolvedValue(result(false));
  await openWelcome();
  await fireEvent.press(screen.getByRole('button', GOOGLE));
  expect(await screen.findByText('Không thể kết nối. Kiểm tra mạng và thử lại.')).toBeTruthy();

  await fireEvent.press(screen.getByRole('button', GOOGLE));

  await waitFor(() => expect(screen).toHavePathname('/today'));
  expect(google).toHaveBeenCalledTimes(2);
});

test('signing out also signs out of the Google account', async () => {
  jest
    .mocked(SecureStore.getItemAsync)
    .mockResolvedValue(
      JSON.stringify({ accessToken: 'stored-access', refreshToken: 'stored-refresh' }),
    );
  jest.spyOn(authApi, 'me').mockResolvedValue(user);
  jest.spyOn(authApi, 'logout').mockResolvedValue(undefined);
  await renderRouter(routes, { initialUrl: '/profile' });

  await fireEvent.press(await screen.findByRole('button', { name: 'Đăng xuất' }));

  await waitFor(() => expect(screen).toHavePathname('/welcome'));
  expect(googleSignIn.signOut).toHaveBeenCalled();
});
