import * as SecureStore from 'expo-secure-store';
import { fireEvent, renderRouter, screen, waitFor } from 'expo-router/testing-library';
import * as RootLayout from '@/app/_layout';
import EntryRoute from '@/app/index';
import AuthLayout from '@/app/(auth)/_layout';
import WelcomeRoute from '@/app/(auth)/welcome';
import LoginRoute from '@/app/(auth)/login';
import ProtectedLayout from '@/app/(protected)/_layout';
import HomeRoute from '@/app/(protected)/home';
import TabsLayout from '@/app/(protected)/(tabs)/_layout';
import TodayRoute from '@/app/(protected)/(tabs)/today';
import ProfileRoute from '@/app/(protected)/(tabs)/profile';
import ProfileSetupRoute from '@/app/(protected)/profile-setup';
import ProfileDetailRoute from '@/app/(protected)/profile-setup/chi-tiet';
import OnboardingRoute from '@/app/(protected)/onboarding';
import NotFound from '@/app/+not-found';
import { authApi } from '@/core/api/auth-api';
import type { AuthUser } from '@/core/auth/types';

const firstTimeUser: AuthUser = {
  id: '0193f0a0-0000-7000-8000-000000000011',
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

const routes = {
  _layout: RootLayout,
  index: EntryRoute,
  '(auth)/_layout': AuthLayout,
  '(auth)/welcome': WelcomeRoute,
  '(auth)/login': LoginRoute,
  '(protected)/_layout': ProtectedLayout,
  '(protected)/home': HomeRoute,
  '(protected)/(tabs)/_layout': TabsLayout,
  '(protected)/(tabs)/today': TodayRoute,
  '(protected)/(tabs)/profile': ProfileRoute,
  '(protected)/profile-setup/index': ProfileSetupRoute,
  '(protected)/profile-setup/chi-tiet': ProfileDetailRoute,
  '(protected)/onboarding': OnboardingRoute,
  '+not-found': NotFound,
};

const TITLE = 'Bạn muốn sử dụng GymBase như thế nào?';
const CONTINUE = { name: 'Tiếp tục' };

/** Người dùng đã có phiên, đang ở onboarding lần đầu; bấm "Thiết lập hồ sơ ngay" để tới màn chọn vai trò. */
async function openRolePicker() {
  jest
    .mocked(SecureStore.getItemAsync)
    .mockResolvedValue(
      JSON.stringify({ accessToken: 'stored-access', refreshToken: 'stored-refresh' }),
    );
  jest.spyOn(authApi, 'me').mockResolvedValue(firstTimeUser);
  jest
    .spyOn(authApi, 'hoanTatDangNhapLanDau')
    .mockResolvedValue({ ...firstTimeUser, is_dang_nhap_lan_dau: false });
  await renderRouter(routes, { initialUrl: '/onboarding' });
  await fireEvent.press(await screen.findByRole('button', { name: 'Thiết lập hồ sơ ngay' }));
  await waitFor(() => expect(screen).toHavePathname('/profile-setup'));
  expect(await screen.findByText(TITLE)).toBeTruthy();
}

afterEach(() => {
  jest.restoreAllMocks();
});

test('the role picker shows the three roles with no tab bar and no placeholder', async () => {
  await openRolePicker();

  expect(screen.getByText('Chọn vai trò phù hợp với nhu cầu của bạn.')).toBeTruthy();
  expect(screen.getAllByRole('radio')[0]?.parent?.props.accessibilityRole).toBe('radiogroup');
  expect(screen.getAllByRole('radio')).toHaveLength(3);
  expect(screen.getByText('Tôi muốn tập luyện (Học viên)')).toBeTruthy();
  expect(screen.getByText('Tôi là PT')).toBeTruthy();
  expect(screen.getByText('Tôi muốn cả hai')).toBeTruthy();
  expect(screen.getByText('Một tài khoản, một hồ sơ chung.')).toBeTruthy();
  expect(screen.queryByText('Hôm nay')).toBeNull();
  expect(screen.queryByText('Cá nhân')).toBeNull();
  expect(screen.queryByRole('button', { name: 'Để sau' })).toBeNull();
});

test('no role is selected at first and Continue stays disabled', async () => {
  await openRolePicker();

  for (const radio of screen.getAllByRole('radio')) {
    expect(radio).not.toBeChecked();
  }
  expect(screen.getByRole('button', CONTINUE)).toBeDisabled();

  await fireEvent.press(screen.getByRole('button', CONTINUE));
  expect(screen).toHavePathname('/profile-setup');
});

test.each([
  [
    'Tôi muốn tập luyện (Học viên)',
    'Theo dõi tập luyện, xây dựng lộ trình, quản lý tiến độ.',
    'hoc_vien',
  ],
  ['Tôi là PT', 'Tạo & quản lý học viên, xây dựng lịch tập, theo dõi kết quả.', 'pt'],
  ['Tôi muốn cả hai', 'Vừa tập luyện cho bản thân, vừa huấn luyện, quản lý học viên.', 'ca_hai'],
])(
  'choosing "%s" and continuing opens the detail screen with loai=%s',
  async (title, description, loai) => {
    await openRolePicker();

    await fireEvent.press(screen.getByRole('radio', { name: `${title}. ${description}` }));

    expect(screen.getByRole('radio', { name: `${title}. ${description}` })).toBeChecked();
    expect(
      screen
        .getAllByRole('radio')
        .filter(
          (radio) =>
            radio.props['aria-checked'] === true || radio.props.accessibilityState?.checked,
        ),
    ).toHaveLength(1);
    await fireEvent.press(screen.getByRole('button', CONTINUE));

    await waitFor(() => expect(screen).toHavePathname('/profile-setup/chi-tiet'));
    expect(screen).toHaveSearchParams({ loai });
  },
);

test('the back arrow returns to the onboarding screen', async () => {
  await openRolePicker();

  await fireEvent.press(screen.getByRole('button', { name: 'Quay lại' }));

  await waitFor(() => expect(screen).toHavePathname('/onboarding'));
});
