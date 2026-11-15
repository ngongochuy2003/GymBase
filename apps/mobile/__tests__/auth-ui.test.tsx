import { Linking } from 'react-native';
import { fireEvent, renderRouter, screen, waitFor } from 'expo-router/testing-library';
import * as RootLayout from '@/app/_layout';
import EntryRoute from '@/app/index';
import AuthLayout from '@/app/(auth)/_layout';
import WelcomeRoute from '@/app/(auth)/welcome';
import LoginRoute from '@/app/(auth)/login';
import RegisterRoute from '@/app/(auth)/register';
import NotFound from '@/app/+not-found';
import { ApiError } from '@/core/api/api-error';
import { authApi } from '@/core/api/auth-api';
import { env } from '@/core/config/env';

const routes = {
  _layout: RootLayout,
  index: EntryRoute,
  '(auth)/_layout': AuthLayout,
  '(auth)/welcome': WelcomeRoute,
  '(auth)/login': LoginRoute,
  '(auth)/register': RegisterRoute,
  '+not-found': NotFound,
};

afterEach(() => {
  jest.restoreAllMocks();
});

test('the welcome screen shows the legal notice and both links open their pages', async () => {
  const open = jest.spyOn(Linking, 'openURL').mockResolvedValue();
  await renderRouter(routes, { initialUrl: '/welcome' });
  expect(await screen.findByText('Hoặc tiếp tục với')).toBeTruthy();

  await fireEvent.press(screen.getByRole('link', { name: 'Điều khoản sử dụng' }));
  await fireEvent.press(screen.getByRole('link', { name: 'Chính sách bảo mật' }));

  expect(open).toHaveBeenNthCalledWith(1, env.termsUrl);
  expect(open).toHaveBeenNthCalledWith(2, env.privacyUrl);
});

test('the login form shows placeholders in the fields and keeps their accessible names', async () => {
  await renderRouter(routes, { initialUrl: '/login' });

  const email = await screen.findByLabelText('Email');
  const password = screen.getByLabelText('Mật khẩu');

  expect(email.props.placeholder).toBe('Email');
  expect(password.props.placeholder).toBe('Mật khẩu');
  expect(screen.queryByText('Quay lại', { exact: true })).toBeNull();
});

test('the back arrow on the login screen returns to the welcome screen', async () => {
  await renderRouter(routes, { initialUrl: '/login' });

  await fireEvent.press(await screen.findByRole('button', { name: 'Quay lại' }));

  await waitFor(() => expect(screen).toHavePathname('/welcome'));
});

test('the back arrow on the register screen returns to the welcome screen', async () => {
  await renderRouter(routes, { initialUrl: '/register' });

  await fireEvent.press(await screen.findByRole('button', { name: 'Quay lại' }));

  await waitFor(() => expect(screen).toHavePathname('/welcome'));
});

test('the register fields have placeholders and the referral info icon explains the code', async () => {
  await renderRouter(routes, { initialUrl: '/register' });
  expect((await screen.findByLabelText('Họ và tên')).props.placeholder).toBe('Họ và tên');
  expect(screen.getByLabelText('Mã giới thiệu (nếu có)').props.placeholder).toBe(
    'Mã giới thiệu (nếu có)',
  );
  expect(screen.queryByText('Mã giới thiệu là gì?')).toBeNull();

  await fireEvent.press(screen.getByRole('button', { name: 'Mã giới thiệu là gì?' }));
  expect(await screen.findByText('Người giới thiệu nhận được')).toBeTruthy();

  await fireEvent.press(screen.getByRole('button', { name: 'Đã hiểu' }));
  await waitFor(() => expect(screen.queryByText('Người giới thiệu nhận được')).toBeNull());
});

test('the welcome hero is a carousel of three slides starting on the first', async () => {
  await renderRouter(routes, { initialUrl: '/welcome' });

  expect(await screen.findByLabelText('Ảnh 1 trên 3')).toBeTruthy();
  expect(
    screen.getByText('Kết nối PT,\ntheo dõi lộ trình\nvà phát triển bản thân\ncùng GymBase'),
  ).toBeTruthy();
  expect(
    screen.getByText('Ghi lại bữa ăn\nvà buổi tập,\nnhận phản hồi\ntừ PT của bạn'),
  ).toBeTruthy();
  expect(
    screen.getByText('Đồng hành cùng PT\ntrên từng buổi tập,\nmỗi ngày một tiến bộ'),
  ).toBeTruthy();
});

test('after several wrong passwords the login screen warns about a temporary lock and clears the password', async () => {
  jest
    .spyOn(authApi, 'login')
    .mockRejectedValue(
      new ApiError('Email hoặc mật khẩu không đúng.', 401, 'AUTH_INVALID_CREDENTIALS'),
    );
  await renderRouter(routes, { initialUrl: '/login' });
  const submit = async () => {
    await fireEvent.changeText(await screen.findByLabelText('Email'), 'minh.anh@gymbase.local');
    await fireEvent.changeText(screen.getByLabelText('Mật khẩu'), 'Mk-sai-1234');
    await fireEvent.press(screen.getByRole('button', { name: 'Đăng nhập' }));
    await screen.findByText('Email hoặc mật khẩu không đúng.');
  };

  await submit();
  await submit();
  expect(screen.queryByText(/hãy đợi vài phút/)).toBeNull();
  await submit();

  expect(await screen.findByText(/hãy đợi vài phút/)).toBeTruthy();
  expect(screen.getByLabelText('Mật khẩu').props.value).toBe('');
});
