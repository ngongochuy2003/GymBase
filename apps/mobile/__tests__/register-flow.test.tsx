import * as SecureStore from 'expo-secure-store';
import { router } from 'expo-router';
import { Linking } from 'react-native';
import { act, fireEvent, renderRouter, screen, waitFor } from 'expo-router/testing-library';
import * as RootLayout from '@/app/_layout';
import EntryRoute from '@/app/index';
import AuthLayout from '@/app/(auth)/_layout';
import WelcomeRoute from '@/app/(auth)/welcome';
import LoginRoute from '@/app/(auth)/login';
import RegisterRoute from '@/app/(auth)/register';
import ProtectedLayout from '@/app/(protected)/_layout';
import HomeRoute from '@/app/(protected)/home';
import ProfileRoute from '@/app/(protected)/profile';
import ProfileSetupRoute from '@/app/(protected)/profile-setup';
import RegisterSuccessRoute from '@/app/(protected)/register-success';
import NotFound from '@/app/+not-found';
import { ApiError } from '@/core/api/api-error';
import { authApi } from '@/core/api/auth-api';
import { env } from '@/core/config/env';
import type { AuthResult, AuthUser } from '@/core/auth/types';

const user: AuthUser = {
  id: '0193f0a0-0000-7000-8000-000000000010',
  ten_dang_nhap: null,
  ho_ten: 'Minh Anh',
  email: 'minh.anh@gymbase.local',
  so_dien_thoai: '0912345678',
  vai_tro: 'User',
  dang_hoat_dong: true,
  ngay_tao: '2026-11-13T00:00:00Z',
  ngay_chinh_sua: null,
};
const created: AuthResult = {
  access_token: 'access-new',
  expires_in: 900,
  refresh_token: 'refresh-new',
  user,
  tai_khoan_moi: true,
};

const routes = {
  _layout: RootLayout,
  index: EntryRoute,
  '(auth)/_layout': AuthLayout,
  '(auth)/welcome': WelcomeRoute,
  '(auth)/login': LoginRoute,
  '(auth)/register': RegisterRoute,
  '(protected)/_layout': ProtectedLayout,
  '(protected)/home': HomeRoute,
  '(protected)/profile': ProfileRoute,
  '(protected)/profile-setup': ProfileSetupRoute,
  '(protected)/register-success': RegisterSuccessRoute,
  '+not-found': NotFound,
};

const TERMS_ERROR = 'Bạn cần đồng ý Điều khoản sử dụng và Chính sách bảo mật.';
const SUBMIT = { name: 'Đăng ký' };

async function openRegister() {
  await renderRouter(routes, { initialUrl: '/register' });
  expect(await screen.findByLabelText('Họ và tên')).toBeTruthy();
}

async function fillForm(
  overrides: Partial<
    Record<'name' | 'email' | 'phone' | 'password' | 'confirm' | 'referral', string>
  > = {},
) {
  const values = {
    name: 'Minh Anh',
    email: 'minh.anh@gymbase.local',
    phone: '0912 345 678',
    password: 'Mk-thu-nghiem-1',
    confirm: 'Mk-thu-nghiem-1',
    referral: '',
    ...overrides,
  };
  await fireEvent.changeText(screen.getByLabelText('Họ và tên'), values.name);
  await fireEvent.changeText(screen.getByLabelText('Email'), values.email);
  await fireEvent.changeText(screen.getByLabelText('Số điện thoại'), values.phone);
  await fireEvent.changeText(screen.getByLabelText('Mật khẩu'), values.password);
  await fireEvent.changeText(screen.getByLabelText('Xác nhận mật khẩu'), values.confirm);
  if (values.referral) {
    await fireEvent.changeText(screen.getByLabelText('Mã giới thiệu (nếu có)'), values.referral);
  }
}

async function agreeAndSubmit() {
  await fireEvent.press(screen.getByRole('checkbox'));
  await fireEvent.press(screen.getByRole('button', SUBMIT));
}

afterEach(() => {
  jest.restoreAllMocks();
});

test('the welcome screen opens the registration form', async () => {
  await renderRouter(routes, { initialUrl: '/' });

  await fireEvent.press(await screen.findByRole('button', { name: 'Đăng ký tài khoản' }));

  await waitFor(() => expect(screen).toHavePathname('/register'));
  expect(await screen.findByLabelText('Xác nhận mật khẩu')).toBeTruthy();
});

test('an empty form shows an error for every required field and does not call the API', async () => {
  const register = jest.spyOn(authApi, 'register');
  await openRegister();

  await fireEvent.press(screen.getByRole('button', SUBMIT));

  expect(await screen.findByText('Nhập họ và tên.')).toBeTruthy();
  expect(screen.getByText('Nhập email.')).toBeTruthy();
  expect(screen.getByText('Nhập số điện thoại.')).toBeTruthy();
  expect(screen.getByText('Nhập mật khẩu.')).toBeTruthy();
  expect(screen.getByText('Nhập lại mật khẩu để xác nhận.')).toBeTruthy();
  expect(screen.getByText(TERMS_ERROR)).toBeTruthy();
  expect(register).not.toHaveBeenCalled();
});

test('malformed values get their own messages', async () => {
  const register = jest.spyOn(authApi, 'register');
  await openRegister();
  await fillForm({ email: 'khong-phai-email', phone: '12345', password: 'ngan', confirm: 'khac' });

  await agreeAndSubmit();

  expect(await screen.findByText('Email không hợp lệ.')).toBeTruthy();
  expect(screen.getByText('Số điện thoại phải là số di động Việt Nam.')).toBeTruthy();
  expect(screen.getByText('Mật khẩu cần ít nhất 8 ký tự.')).toBeTruthy();
  expect(screen.getByText('Mật khẩu xác nhận không khớp.')).toBeTruthy();
  expect(register).not.toHaveBeenCalled();
});

test('the terms must be ticked even when every other field is valid', async () => {
  const register = jest.spyOn(authApi, 'register');
  await openRegister();
  await fillForm();

  await fireEvent.press(screen.getByRole('button', SUBMIT));

  expect(await screen.findByText(TERMS_ERROR)).toBeTruthy();
  expect(register).not.toHaveBeenCalled();
});

test('passwords can be shown and hidden', async () => {
  await openRegister();
  expect(screen.getByLabelText('Mật khẩu').props.secureTextEntry).toBe(true);

  await fireEvent.press(screen.getByRole('button', { name: 'Hiện mật khẩu' }));
  expect(screen.getByLabelText('Mật khẩu').props.secureTextEntry).toBe(false);
  expect(screen.getByLabelText('Xác nhận mật khẩu').props.secureTextEntry).toBe(true);

  await fireEvent.press(screen.getByRole('button', { name: 'Ẩn mật khẩu' }));
  expect(screen.getByLabelText('Mật khẩu').props.secureTextEntry).toBe(true);
});

test('the terms and privacy links open the configured pages', async () => {
  const open = jest.spyOn(Linking, 'openURL').mockResolvedValue(undefined);
  await openRegister();

  await fireEvent.press(screen.getByRole('link', { name: 'Điều khoản sử dụng' }));
  await fireEvent.press(screen.getByRole('link', { name: 'Chính sách bảo mật' }));

  expect(open).toHaveBeenNthCalledWith(1, env.termsUrl);
  expect(open).toHaveBeenNthCalledWith(2, env.privacyUrl);
});

test('registering stores the session and opens the success screen once', async () => {
  const register = jest.spyOn(authApi, 'register').mockResolvedValue(created);
  await openRegister();
  await fillForm({ email: '  Minh.Anh@GymBase.Local ', referral: 'KHOI123' });

  await agreeAndSubmit();

  await waitFor(() => expect(screen).toHavePathname('/register-success'));
  expect(await screen.findByText('Đăng ký thành công!')).toBeTruthy();
  expect(register).toHaveBeenCalledWith({
    ho_ten: 'Minh Anh',
    email: 'minh.anh@gymbase.local',
    so_dien_thoai: '0912 345 678',
    mat_khau: 'Mk-thu-nghiem-1',
    dong_y_dieu_khoan: true,
    ma_gioi_thieu: 'KHOI123',
  });
  expect(SecureStore.setItemAsync).toHaveBeenCalledWith(
    'gymbase.session.tokens',
    JSON.stringify({ accessToken: 'access-new', refreshToken: 'refresh-new' }),
  );
});

test('an empty referral code is not sent', async () => {
  const register = jest.spyOn(authApi, 'register').mockResolvedValue(created);
  await openRegister();
  await fillForm();

  await agreeAndSubmit();

  await waitFor(() => expect(register).toHaveBeenCalled());
  expect(register.mock.calls[0]?.[0]).not.toHaveProperty('ma_gioi_thieu');
});

test('skipping goes to the home screen and the success screen does not come back', async () => {
  jest.spyOn(authApi, 'register').mockResolvedValue(created);
  await openRegister();
  await fillForm();
  await agreeAndSubmit();

  await fireEvent.press(await screen.findByRole('button', { name: 'Bỏ qua, vào Trang chủ' }));

  await waitFor(() => expect(screen).toHavePathname('/home'));
  await act(async () => router.navigate('/' as never));
  await waitFor(() => expect(screen).toHavePathname('/home'));
  expect(screen.queryByText('Đăng ký thành công!')).toBeNull();
});

test('setting up the profile now opens the profile setup placeholder', async () => {
  jest.spyOn(authApi, 'register').mockResolvedValue(created);
  await openRegister();
  await fillForm();
  await agreeAndSubmit();

  await fireEvent.press(await screen.findByRole('button', { name: 'Thiết lập hồ sơ ngay' }));

  await waitFor(() => expect(screen).toHavePathname('/profile-setup'));
  expect(await screen.findByText('Thiết lập hồ sơ')).toBeTruthy();
});

test('a taken email says so and offers to sign in instead', async () => {
  jest.spyOn(authApi, 'register').mockRejectedValue(
    new ApiError("Email 'minh.anh@gymbase.local' đã được dùng.", 409, 'USER_EMAIL_TAKEN', {
      email: ["Email 'minh.anh@gymbase.local' đã được dùng."],
    }),
  );
  await openRegister();
  await fillForm();

  await agreeAndSubmit();

  expect(
    await screen.findByText('Email này đã được dùng. Hãy đăng nhập hoặc dùng email khác.'),
  ).toBeTruthy();
  expect(screen).toHavePathname('/register');

  await fireEvent.press(screen.getByRole('button', { name: 'Đã có tài khoản? Đăng nhập' }));
  await waitFor(() => expect(screen).toHavePathname('/login'));
});

test('server field errors are shown under the matching field', async () => {
  jest.spyOn(authApi, 'register').mockRejectedValue(
    new ApiError('Thông tin chưa hợp lệ.', 400, 'VALIDATION_FAILED', {
      so_dien_thoai: ['Số điện thoại phải là số di động Việt Nam.'],
      mat_khau: ['Mật khẩu này quá phổ biến, hãy chọn mật khẩu khác.'],
    }),
  );
  await openRegister();
  await fillForm();

  await agreeAndSubmit();

  expect(await screen.findByText('Số điện thoại phải là số di động Việt Nam.')).toBeTruthy();
  expect(screen.getByText('Mật khẩu này quá phổ biến, hãy chọn mật khẩu khác.')).toBeTruthy();
});

test('a network failure shows a message and a retry button that registers', async () => {
  const register = jest
    .spyOn(authApi, 'register')
    .mockRejectedValueOnce(
      new ApiError('Không thể kết nối. Kiểm tra mạng và thử lại.', undefined, 'NETWORK'),
    )
    .mockResolvedValue(created);
  await openRegister();
  await fillForm();
  await agreeAndSubmit();
  expect(await screen.findByText('Không thể kết nối. Kiểm tra mạng và thử lại.')).toBeTruthy();

  await fireEvent.press(screen.getByRole('button', { name: 'Thử lại' }));

  await waitFor(() => expect(screen).toHavePathname('/register-success'));
  expect(register).toHaveBeenCalledTimes(2);
});

test('signing in again later goes straight to the app, not the success screen', async () => {
  jest
    .mocked(SecureStore.getItemAsync)
    .mockResolvedValue(
      JSON.stringify({ accessToken: 'stored-access', refreshToken: 'stored-refresh' }),
    );
  jest.spyOn(authApi, 'me').mockResolvedValue(user);

  await renderRouter(routes, { initialUrl: '/' });

  await waitFor(() => expect(screen).toHavePathname('/home'));
  expect(screen.queryByText('Đăng ký thành công!')).toBeNull();
});
