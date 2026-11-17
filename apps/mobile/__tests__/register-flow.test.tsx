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
import OnboardingRoute from '@/app/(protected)/onboarding';
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
  is_dang_nhap_lan_dau: true,
};
/** Kết quả đăng ký: chỉ có tài khoản, không có phiên. */
const created: AuthUser = user;
/** Lần đăng nhập đầu tiên sau khi đăng ký. */
const firstLogin: AuthResult = {
  access_token: 'access-new',
  expires_in: 900,
  refresh_token: 'refresh-new',
  user,
};
const onboarded: AuthUser = { ...user, is_dang_nhap_lan_dau: false };

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
  '(protected)/profile-setup/index': ProfileSetupRoute,
  '(protected)/onboarding': OnboardingRoute,
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

  expect(await screen.findByText('Vui lòng nhập họ và tên.')).toBeTruthy();
  expect(screen.getByText('Vui lòng nhập email.')).toBeTruthy();
  expect(screen.getByText('Vui lòng nhập số điện thoại.')).toBeTruthy();
  expect(screen.getByText('Vui lòng nhập mật khẩu.')).toBeTruthy();
  expect(screen.getByText('Vui lòng nhập lại mật khẩu.')).toBeTruthy();
  expect(screen.getByText(TERMS_ERROR)).toBeTruthy();
  expect(register).not.toHaveBeenCalled();
});

test('malformed values get their own messages', async () => {
  const register = jest.spyOn(authApi, 'register');
  await openRegister();
  await fillForm({ email: 'khong-phai-email', phone: '12345', password: 'ngan', confirm: 'khac' });

  await agreeAndSubmit();

  expect(await screen.findByText('Email chưa đúng định dạng, ví dụ ban@email.com.')).toBeTruthy();
  expect(screen.getByText('Nhập số di động Việt Nam, ví dụ 0912 345 678.')).toBeTruthy();
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

test('registering creates the account without a session and opens login with the email filled in', async () => {
  const register = jest.spyOn(authApi, 'register').mockResolvedValue(created);
  await openRegister();
  await fillForm({ email: '  Minh.Anh@GymBase.Local ', referral: 'KHOI123' });

  await agreeAndSubmit();

  await waitFor(() => expect(screen).toHavePathname('/login'));
  expect(await screen.findByText('Đăng ký thành công. Hãy đăng nhập để tiếp tục.')).toBeTruthy();
  expect(screen.getByLabelText('Email').props.value).toBe('minh.anh@gymbase.local');
  expect(register).toHaveBeenCalledWith({
    ho_ten: 'Minh Anh',
    email: 'minh.anh@gymbase.local',
    so_dien_thoai: '0912 345 678',
    mat_khau: 'Mk-thu-nghiem-1',
    dong_y_dieu_khoan: true,
    ma_gioi_thieu: 'KHOI123',
  });
  expect(SecureStore.setItemAsync).not.toHaveBeenCalled();
});

test('an empty referral code is not sent', async () => {
  const register = jest.spyOn(authApi, 'register').mockResolvedValue(created);
  await openRegister();
  await fillForm();

  await agreeAndSubmit();

  await waitFor(() => expect(register).toHaveBeenCalled());
  expect(register.mock.calls[0]?.[0]).not.toHaveProperty('ma_gioi_thieu');
});

/** Đăng ký rồi đăng nhập ngay trên màn đăng nhập được điền sẵn email; kết thúc ở màn onboarding. */
async function registerThenSignIn() {
  jest.spyOn(authApi, 'register').mockResolvedValue(created);
  jest.spyOn(authApi, 'login').mockResolvedValue(firstLogin);
  await openRegister();
  await fillForm();
  await agreeAndSubmit();
  await fireEvent.changeText(await screen.findByLabelText('Mật khẩu'), 'Mk-thu-nghiem-1');
  await fireEvent.press(screen.getByRole('button', { name: 'Đăng nhập' }));
  await waitFor(() => expect(screen).toHavePathname('/onboarding'));
  expect(await screen.findByText('Đăng nhập thành công!')).toBeTruthy();
}

test('the first sign-in after registering shows onboarding, and skipping goes home for good', async () => {
  const complete = jest.spyOn(authApi, 'hoanTatDangNhapLanDau').mockResolvedValue(onboarded);
  await registerThenSignIn();
  expect(SecureStore.setItemAsync).toHaveBeenCalledWith(
    'gymbase.session.tokens',
    JSON.stringify({ accessToken: 'access-new', refreshToken: 'refresh-new' }),
  );

  await fireEvent.press(await screen.findByRole('button', { name: 'Bỏ qua, vào Trang chủ' }));

  await waitFor(() => expect(screen).toHavePathname('/home'));
  expect(complete).toHaveBeenCalledTimes(1);
  await act(async () => router.navigate('/' as never));
  await waitFor(() => expect(screen).toHavePathname('/home'));
  expect(screen.queryByText('Đăng nhập thành công!')).toBeNull();
});

test('setting up the profile now opens the role picker', async () => {
  const complete = jest.spyOn(authApi, 'hoanTatDangNhapLanDau').mockResolvedValue(onboarded);
  await registerThenSignIn();

  await fireEvent.press(await screen.findByRole('button', { name: 'Thiết lập hồ sơ ngay' }));

  await waitFor(() => expect(screen).toHavePathname('/profile-setup'));
  expect(complete).toHaveBeenCalledTimes(1);
  expect(await screen.findByText('Bạn muốn sử dụng GymBase như thế nào?')).toBeTruthy();
});

test('onboarding still lets the user in when the server cannot be reached', async () => {
  jest
    .spyOn(authApi, 'hoanTatDangNhapLanDau')
    .mockRejectedValue(new ApiError('Không thể kết nối.', undefined, 'NETWORK'));
  await registerThenSignIn();

  await fireEvent.press(await screen.findByRole('button', { name: 'Bỏ qua, vào Trang chủ' }));

  await waitFor(() => expect(screen).toHavePathname('/home'));
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

  // Câu lỗi có kèm liên kết "Đăng nhập ngay" nên khớp theo phần đầu câu.
  expect(
    await screen.findByText(/Email này đã được dùng\. Hãy đăng nhập hoặc dùng email khác\./),
  ).toBeTruthy();
  expect(screen).toHavePathname('/register');

  await fireEvent.press(screen.getByRole('link', { name: 'Đăng nhập ngay' }));
  await waitFor(() => expect(screen).toHavePathname('/login'));
});

test('server field errors are shown under the matching field', async () => {
  jest.spyOn(authApi, 'register').mockRejectedValue(
    new ApiError('Thông tin chưa hợp lệ.', 400, 'VALIDATION_FAILED', {
      so_dien_thoai: ['Số điện thoại phải là số di động Việt Nam.'],
      mat_khau: ['Mật khẩu tối đa 128 ký tự.'],
    }),
  );
  await openRegister();
  await fillForm();

  await agreeAndSubmit();

  expect(await screen.findByText('Số điện thoại phải là số di động Việt Nam.')).toBeTruthy();
  expect(screen.getByText('Mật khẩu tối đa 128 ký tự.')).toBeTruthy();
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

  await waitFor(() => expect(screen).toHavePathname('/login'));
  expect(register).toHaveBeenCalledTimes(2);
});

test('a stored session after onboarding goes straight to the app', async () => {
  jest
    .mocked(SecureStore.getItemAsync)
    .mockResolvedValue(
      JSON.stringify({ accessToken: 'stored-access', refreshToken: 'stored-refresh' }),
    );
  jest.spyOn(authApi, 'me').mockResolvedValue(onboarded);

  await renderRouter(routes, { initialUrl: '/' });

  await waitFor(() => expect(screen).toHavePathname('/home'));
  expect(screen.queryByText('Đăng nhập thành công!')).toBeNull();
});

test('a stored session that never finished onboarding shows it again', async () => {
  jest
    .mocked(SecureStore.getItemAsync)
    .mockResolvedValue(
      JSON.stringify({ accessToken: 'stored-access', refreshToken: 'stored-refresh' }),
    );
  jest.spyOn(authApi, 'me').mockResolvedValue(user);

  await renderRouter(routes, { initialUrl: '/' });

  await waitFor(() => expect(screen).toHavePathname('/onboarding'));
});
