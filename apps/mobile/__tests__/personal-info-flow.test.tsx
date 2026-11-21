import * as SecureStore from 'expo-secure-store';
import { fireEvent, renderRouter, screen, waitFor } from 'expo-router/testing-library';
import * as RootLayout from '@/app/_layout';
import EntryRoute from '@/app/index';
import AuthLayout from '@/app/(auth)/_layout';
import WelcomeRoute from '@/app/(auth)/welcome';
import LoginRoute from '@/app/(auth)/login';
import ProtectedLayout from '@/app/(protected)/_layout';
import HomeRoute from '@/app/(protected)/home';
import ProfileRoute from '@/app/(protected)/profile';
import ProfileSetupRoute from '@/app/(protected)/profile-setup';
import PersonalInfoRoute from '@/app/(protected)/profile-setup/ca-nhan';
import ProfileDetailRoute from '@/app/(protected)/profile-setup/chi-tiet';
import OnboardingRoute from '@/app/(protected)/onboarding';
import NotFound from '@/app/+not-found';
import { ApiError } from '@/core/api/api-error';
import { authApi } from '@/core/api/auth-api';
import { danhMucApi, type DanhMucNhom } from '@/shared/api/danh-muc-api';
import type { AuthUser } from '@/core/auth/types';

const user: AuthUser = {
  id: '0193f0a0-0000-7000-8000-000000000012',
  ten_dang_nhap: null,
  ho_ten: 'Minh Anh',
  email: 'minh.anh@gymbase.local',
  so_dien_thoai: '0912345678',
  vai_tro: 'User',
  dang_hoat_dong: true,
  ngay_tao: '2026-11-13T00:00:00Z',
  ngay_chinh_sua: null,
  is_dang_nhap_lan_dau: false,
};

const item = (ma: string, ten: string) => ({
  ma,
  ten,
  mo_ta: null,
  cap: 1,
  danh_muc_cap_tren_id: null,
});
const categories: DanhMucNhom[] = [
  { ma_dinh_danh: 'khu_vuc', items: [item('ha_noi', 'Hà Nội'), item('tp_hcm', 'TP. Hồ Chí Minh')] },
];

const routes = {
  _layout: RootLayout,
  index: EntryRoute,
  '(auth)/_layout': AuthLayout,
  '(auth)/welcome': WelcomeRoute,
  '(auth)/login': LoginRoute,
  '(protected)/_layout': ProtectedLayout,
  '(protected)/home': HomeRoute,
  '(protected)/profile': ProfileRoute,
  '(protected)/profile-setup/index': ProfileSetupRoute,
  '(protected)/profile-setup/ca-nhan': PersonalInfoRoute,
  '(protected)/profile-setup/chi-tiet': ProfileDetailRoute,
  '(protected)/onboarding': OnboardingRoute,
  '+not-found': NotFound,
};

const CONTINUE = { name: 'Tiếp tục' };

async function openPersonalInfo(url = '/profile-setup/ca-nhan?loai=hoc_vien') {
  jest
    .mocked(SecureStore.getItemAsync)
    .mockResolvedValue(
      JSON.stringify({ accessToken: 'stored-access', refreshToken: 'stored-refresh' }),
    );
  jest.spyOn(authApi, 'me').mockResolvedValue(user);
  await renderRouter(routes, { initialUrl: url });
  expect(await screen.findByText('Thông tin cá nhân')).toBeTruthy();
}

/** Mở ô chọn khi danh mục đã tải xong (lúc đang tải ô bị vô hiệu và bấm không có tác dụng). */
async function openSelect(label: string) {
  const field = await screen.findByRole('button', { name: label });
  await waitFor(() => expect(field).not.toBeDisabled());
  await fireEvent.press(field);
}

/** Chọn một mục trong ô chọn: mở bảng chọn rồi bấm mục. */
async function choose(label: string, option: string) {
  await openSelect(label);
  await fireEvent.press(await screen.findByRole('radio', { name: option }));
}

async function fillValid() {
  await fireEvent.changeText(screen.getByLabelText('Tên hiển thị *'), 'Minh Anh Gym');
  await fireEvent.changeText(screen.getByLabelText('Ngày sinh'), '20051998');
  await choose('Giới tính', 'Nữ');
  await choose('Khu vực', 'Hà Nội');
}

beforeEach(() => {
  jest.spyOn(danhMucApi, 'get').mockResolvedValue(categories);
});

afterEach(() => {
  jest.restoreAllMocks();
});

test('the form opens with the display name prefilled from the account and no tab bar', async () => {
  await openPersonalInfo();

  expect(screen.getByLabelText('Tên hiển thị *')).toHaveDisplayValue('Minh Anh');
  expect(screen.getByLabelText('Ngày sinh')).toHaveDisplayValue('');
  expect(screen.queryByText('Khám phá')).toBeNull();
});

test('the (?) button beside the birth date explains who can see it and what it is for', async () => {
  await openPersonalInfo();

  await fireEvent.press(screen.getByRole('button', { name: 'Ngày sinh dùng để làm gì?' }));

  expect(await screen.findByText('Ngày sinh dùng để làm gì?')).toBeTruthy();
  expect(screen.getByText('Ai xem được?')).toBeTruthy();
  expect(
    screen.getByText(
      'Thông tin này không hiển thị công khai. Chỉ PT đã kết nối với bạn mới xem được.',
    ),
  ).toBeTruthy();
  expect(screen.getByText('Dùng để làm gì?')).toBeTruthy();
  expect(screen.getByText(/tính tuổi của bạn/)).toBeTruthy();

  await fireEvent.press(screen.getByRole('button', { name: 'Đã hiểu' }));

  await waitFor(() => expect(screen.queryByText('Ai xem được?')).toBeNull());
  expect(screen).toHavePathname('/profile-setup/ca-nhan');
});

test('the gender field has no (?) button', async () => {
  await openPersonalInfo();

  expect(screen.queryByRole('button', { name: 'Giới tính dùng để làm gì?' })).toBeNull();
});

test('a dropdown with no options says so instead of opening an empty sheet', async () => {
  jest.mocked(danhMucApi.get).mockResolvedValue([{ ma_dinh_danh: 'khu_vuc', items: [] }]);
  await openPersonalInfo();

  await openSelect('Khu vực');

  expect(await screen.findByText('Chưa có lựa chọn nào.')).toBeTruthy();
});

test('gender is a fixed list that never waits for the server, and areas come from the API', async () => {
  await openPersonalInfo();

  await openSelect('Giới tính');
  expect(
    (await screen.findAllByRole('radio')).map((radio) => radio.props.accessibilityLabel),
  ).toEqual(['Nam', 'Nữ', 'Khác']);
  await fireEvent.press(screen.getByRole('radio', { name: 'Nam' }));

  await openSelect('Khu vực');
  expect(
    (await screen.findAllByRole('radio')).map((radio) => radio.props.accessibilityLabel),
  ).toEqual(['Hà Nội', 'TP. Hồ Chí Minh']);
  expect(danhMucApi.get).toHaveBeenCalledWith(['khu_vuc']);
});

test('typing digits for the birth date inserts the slashes', async () => {
  await openPersonalInfo();

  await fireEvent.changeText(screen.getByLabelText('Ngày sinh'), '20051998');

  expect(screen.getByLabelText('Ngày sinh')).toHaveDisplayValue('20/05/1998');
});

test('an empty display name is the only required error and does not move on', async () => {
  await openPersonalInfo();
  await fireEvent.changeText(screen.getByLabelText('Tên hiển thị *'), '   ');

  await fireEvent.press(screen.getByRole('button', CONTINUE));

  expect(await screen.findByText('Vui lòng nhập tên hiển thị.')).toBeTruthy();
  expect(screen.queryByText('Vui lòng nhập ngày sinh.')).toBeNull();
  expect(screen.queryByText('Vui lòng chọn giới tính.')).toBeNull();
  expect(screen.queryByText('Vui lòng chọn khu vực.')).toBeNull();
  expect(screen).toHavePathname('/profile-setup/ca-nhan');
});

test('birth date, gender and area are optional: leaving them empty still moves on', async () => {
  await openPersonalInfo();
  await fireEvent.changeText(screen.getByLabelText('Tên hiển thị *'), 'Minh Anh Gym');

  await fireEvent.press(screen.getByRole('button', CONTINUE));

  await waitFor(() => expect(screen).toHavePathname('/profile-setup/chi-tiet'));
});

test.each([
  ['31022000', 'Ngày sinh chưa đúng, ví dụ 20/05/1998.'],
  ['2005', 'Ngày sinh chưa đúng, ví dụ 20/05/1998.'],
  ['01019999', 'Ngày sinh không được ở tương lai.'],
  ['01012020', 'Tuổi phải từ 13 đến 100.'],
  ['01011900', 'Tuổi phải từ 13 đến 100.'],
])('birth date %s is rejected with its own message', async (digits, message) => {
  await openPersonalInfo();
  await fillValid();
  await fireEvent.changeText(screen.getByLabelText('Ngày sinh'), digits);

  await fireEvent.press(screen.getByRole('button', CONTINUE));

  expect(await screen.findByText(message)).toBeTruthy();
  expect(screen).toHavePathname('/profile-setup/ca-nhan');
});

test.each(['hoc_vien', 'pt', 'ca_hai'])(
  'a valid form moves on to the detail screen with loai=%s',
  async (loai) => {
    await openPersonalInfo(`/profile-setup/ca-nhan?loai=${loai}`);
    await fillValid();

    await fireEvent.press(screen.getByRole('button', CONTINUE));

    await waitFor(() => expect(screen).toHavePathname('/profile-setup/chi-tiet'));
    expect(screen).toHaveSearchParams({ loai });
  },
);

test('a missing or wrong loai falls back to hoc_vien', async () => {
  await openPersonalInfo('/profile-setup/ca-nhan?loai=admin');
  await fillValid();

  await fireEvent.press(screen.getByRole('button', CONTINUE));

  await waitFor(() => expect(screen).toHavePathname('/profile-setup/chi-tiet'));
  expect(screen).toHaveSearchParams({ loai: 'hoc_vien' });
});

test('going back from the next step keeps what was typed, and no save call is made', async () => {
  const save = jest.spyOn(authApi, 'hoanTatDangNhapLanDau');
  await openPersonalInfo();
  await fillValid();
  await fireEvent.press(screen.getByRole('button', CONTINUE));
  await waitFor(() => expect(screen).toHavePathname('/profile-setup/chi-tiet'));

  await fireEvent.press(await screen.findByRole('button', { name: 'Quay lại' }));

  await waitFor(() => expect(screen).toHavePathname('/profile-setup/ca-nhan'));
  expect(screen.getByLabelText('Tên hiển thị *')).toHaveDisplayValue('Minh Anh Gym');
  expect(screen.getByLabelText('Ngày sinh')).toHaveDisplayValue('20/05/1998');
  expect(screen.getByRole('button', { name: 'Giới tính' })).toHaveAccessibilityValue({
    text: 'Nữ',
  });
  expect(screen.getByRole('button', { name: 'Khu vực' })).toHaveAccessibilityValue({
    text: 'Hà Nội',
  });
  expect(save).not.toHaveBeenCalled();
});

test('when the categories cannot be loaded the dropdowns say so and Retry reloads them', async () => {
  // Lỗi 4xx không bị react-query retry nên hiện lỗi ngay (lỗi mạng thì retry một lần sau 1 giây).
  jest
    .mocked(danhMucApi.get)
    .mockRejectedValueOnce(new ApiError('Không có quyền.', 403, 'FORBIDDEN'));
  await openPersonalInfo();

  expect(
    (await screen.findAllByText('Không tải được danh sách. Vui lòng thử lại.')).length,
  ).toBeGreaterThan(0);
  expect(screen.getByRole('button', { name: 'Khu vực' })).toBeDisabled();
  // Giới tính là danh sách cố định nên vẫn chọn được dù danh mục khu vực lỗi.
  expect(screen.getByRole('button', { name: 'Giới tính' })).not.toBeDisabled();

  await fireEvent.press(screen.getByRole('button', { name: 'Thử lại' }));

  await waitFor(() => expect(screen.getByRole('button', { name: 'Khu vực' })).not.toBeDisabled());
  expect(screen.queryByText('Không tải được danh sách. Vui lòng thử lại.')).toBeNull();
});
