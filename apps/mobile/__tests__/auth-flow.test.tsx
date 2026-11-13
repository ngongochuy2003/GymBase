import * as SecureStore from 'expo-secure-store';
import { useQuery } from '@tanstack/react-query';
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
import NotFound from '@/app/+not-found';
import { ApiError } from '@/core/api/api-error';
import { authApi } from '@/core/api/auth-api';
import type { AuthResult, AuthUser } from '@/core/auth/types';

const user: AuthUser = {
  id: '0193f0a0-0000-7000-8000-000000000009',
  ten_dang_nhap: null,
  ho_ten: 'Minh Anh',
  email: 'minh.anh@gymbase.local',
  so_dien_thoai: '0912345678',
  vai_tro: 'User',
  dang_hoat_dong: true,
  ngay_tao: '2026-11-13T00:00:00Z',
  ngay_chinh_sua: null,
};
const result: AuthResult = {
  access_token: 'access-1',
  expires_in: 900,
  refresh_token: 'refresh-1',
  user,
  tai_khoan_moi: false,
};

function NeedsProfile() {
  useQuery({
    queryKey: ['needs-profile'],
    queryFn: () => {
      throw new ApiError('Cần thiết lập hồ sơ.', 403, 'PROFILE_REQUIRED');
    },
    retry: false,
  });
  return null;
}

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
  '+not-found': NotFound,
};

const storedTokens = JSON.stringify({
  accessToken: 'stored-access',
  refreshToken: 'stored-refresh',
});

async function openLogin() {
  await renderRouter(routes, { initialUrl: '/login' });
  expect(await screen.findByLabelText('Email')).toBeTruthy();
}

async function fillAndSubmit(email: string, password: string) {
  await fireEvent.changeText(screen.getByLabelText('Email'), email);
  await fireEvent.changeText(screen.getByLabelText('Mật khẩu'), password);
  await fireEvent.press(screen.getByRole('button', { name: 'Đăng nhập tài khoản' }));
}

afterEach(() => {
  jest.restoreAllMocks();
});

test('guest deep link goes to the welcome screen, not the protected page or not-found', async () => {
  await renderRouter(routes, { initialUrl: '/profile' });

  expect(await screen.findByRole('button', { name: 'Đăng ký tài khoản' })).toBeTruthy();
  expect(screen).toHavePathname('/welcome');
  expect(screen.queryByText('Không gian của bạn')).toBeNull();
  expect(screen.queryByText('Không tìm thấy màn hình')).toBeNull();
});

test('welcome offers sign in, register and Google, the last one not wired yet', async () => {
  await renderRouter(routes, { initialUrl: '/' });
  expect(await screen.findByRole('button', { name: 'Đăng ký tài khoản' })).toBeTruthy();

  await fireEvent.press(screen.getByRole('button', { name: 'Đăng nhập với Google' }));
  expect(await screen.findByText('Tính năng này sắp có.')).toBeTruthy();

  await fireEvent.press(screen.getByRole('button', { name: 'Đăng nhập' }));
  await waitFor(() => expect(screen).toHavePathname('/login'));
});

test('login shows a field error for each invalid field and does not call the API', async () => {
  const login = jest.spyOn(authApi, 'login');
  await openLogin();

  await fireEvent.press(screen.getByRole('button', { name: 'Đăng nhập tài khoản' }));
  expect(await screen.findByText('Nhập email.')).toBeTruthy();
  expect(screen.getByText('Nhập mật khẩu.')).toBeTruthy();

  await fireEvent.changeText(screen.getByLabelText('Email'), 'khong-phai-email');
  await fireEvent.changeText(screen.getByLabelText('Mật khẩu'), 'mk');
  await fireEvent.press(screen.getByRole('button', { name: 'Đăng nhập tài khoản' }));
  expect(await screen.findByText('Email không hợp lệ.')).toBeTruthy();
  expect(login).not.toHaveBeenCalled();
});

test('wrong credentials show the server message and stay on login', async () => {
  jest
    .spyOn(authApi, 'login')
    .mockRejectedValue(
      new ApiError('Email hoặc mật khẩu không đúng.', 401, 'AUTH_INVALID_CREDENTIALS'),
    );
  await openLogin();

  await fillAndSubmit('minh.anh@gymbase.local', 'Mk-sai-1234');

  expect(await screen.findByText('Email hoặc mật khẩu không đúng.')).toBeTruthy();
  expect(screen).toHavePathname('/login');
  expect(SecureStore.setItemAsync).not.toHaveBeenCalled();
});

test('signing in stores the tokens, opens the app and shows the account name', async () => {
  const login = jest.spyOn(authApi, 'login').mockResolvedValue(result);
  await openLogin();

  await fillAndSubmit('  Minh.Anh@GymBase.Local ', 'Mk-thu-nghiem-1');

  await waitFor(() => expect(screen).toHavePathname('/today'));
  expect(login).toHaveBeenCalledWith(
    expect.objectContaining({ email: 'minh.anh@gymbase.local', mat_khau: 'Mk-thu-nghiem-1' }),
  );
  expect(await screen.findByText('Chào Minh Anh,')).toBeTruthy();
  expect(SecureStore.setItemAsync).toHaveBeenCalledWith(
    'gymbase.session.tokens',
    JSON.stringify({ accessToken: 'access-1', refreshToken: 'refresh-1' }),
  );
});

test('a network failure shows a message and a retry button that signs in', async () => {
  const login = jest
    .spyOn(authApi, 'login')
    .mockRejectedValueOnce(
      new ApiError('Không thể kết nối. Kiểm tra mạng và thử lại.', undefined, 'NETWORK'),
    )
    .mockResolvedValue(result);
  await openLogin();

  await fillAndSubmit('minh.anh@gymbase.local', 'Mk-thu-nghiem-1');
  expect(await screen.findByText('Không thể kết nối. Kiểm tra mạng và thử lại.')).toBeTruthy();

  await fireEvent.press(screen.getByRole('button', { name: 'Thử lại' }));

  await waitFor(() => expect(screen).toHavePathname('/today'));
  expect(login).toHaveBeenCalledTimes(2);
});

test('reopening the app restores the stored session without asking to sign in', async () => {
  jest.mocked(SecureStore.getItemAsync).mockResolvedValue(storedTokens);
  const me = jest.spyOn(authApi, 'me').mockResolvedValue(user);

  await renderRouter(routes, { initialUrl: '/' });

  await waitFor(() => expect(screen).toHavePathname('/today'));
  expect(me).toHaveBeenCalled();
  expect(await screen.findByText('Chào Minh Anh,')).toBeTruthy();
});

test('a stored session the server rejects sends the user to the welcome screen', async () => {
  jest.mocked(SecureStore.getItemAsync).mockResolvedValue(storedTokens);
  jest.spyOn(authApi, 'me').mockRejectedValue(new ApiError('Hết phiên.', 401, 'UNAUTHORIZED'));

  await renderRouter(routes, { initialUrl: '/today' });

  expect(await screen.findByRole('button', { name: 'Đăng ký tài khoản' })).toBeTruthy();
  expect(screen).toHavePathname('/welcome');
});

test('signing out revokes the session, clears the tokens and returns to the welcome screen', async () => {
  jest.mocked(SecureStore.getItemAsync).mockResolvedValue(storedTokens);
  jest.spyOn(authApi, 'me').mockResolvedValue(user);
  const logout = jest.spyOn(authApi, 'logout').mockResolvedValue(undefined);
  await renderRouter(routes, { initialUrl: '/profile' });
  expect(await screen.findByText('minh.anh@gymbase.local')).toBeTruthy();

  await fireEvent.press(screen.getByRole('button', { name: 'Đăng xuất' }));

  await waitFor(() => expect(screen).toHavePathname('/welcome'));
  expect(logout).toHaveBeenCalled();
  expect(SecureStore.deleteItemAsync).toHaveBeenCalledWith('gymbase.session.tokens');
  expect(screen.queryByText('Không tìm thấy màn hình')).toBeNull();
});

test('signing out still works when the server cannot be reached', async () => {
  jest.mocked(SecureStore.getItemAsync).mockResolvedValue(storedTokens);
  jest.spyOn(authApi, 'me').mockResolvedValue(user);
  jest
    .spyOn(authApi, 'logout')
    .mockRejectedValue(new ApiError('Không thể kết nối.', undefined, 'NETWORK'));
  await renderRouter(routes, { initialUrl: '/profile' });
  await screen.findByText('minh.anh@gymbase.local');

  await fireEvent.press(screen.getByRole('button', { name: 'Đăng xuất' }));

  await waitFor(() => expect(screen).toHavePathname('/welcome'));
  expect(SecureStore.deleteItemAsync).toHaveBeenCalledWith('gymbase.session.tokens');
});

test('the theme can be switched from the profile tab', async () => {
  jest.mocked(SecureStore.getItemAsync).mockResolvedValue(storedTokens);
  jest.spyOn(authApi, 'me').mockResolvedValue(user);
  await renderRouter(routes, { initialUrl: '/profile' });

  await fireEvent.press(await screen.findByRole('button', { name: 'Chuyển sang giao diện tối' }));

  expect(await screen.findByText('Hiện tại: Tối')).toBeTruthy();
});

test('a PROFILE_REQUIRED error sends the signed-in user to the profile setup placeholder', async () => {
  jest.mocked(SecureStore.getItemAsync).mockResolvedValue(storedTokens);
  jest.spyOn(authApi, 'me').mockResolvedValue(user);

  // Màn "Hôm nay" được thay bằng một màn có query trả PROFILE_REQUIRED.
  await renderRouter({ ...routes, '(protected)/today': NeedsProfile }, { initialUrl: '/today' });

  await waitFor(() => expect(screen).toHavePathname('/profile-setup'));
  expect(await screen.findByText('Thiết lập hồ sơ')).toBeTruthy();
});
