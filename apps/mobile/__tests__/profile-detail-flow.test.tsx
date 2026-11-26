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
import ProfileDoneRoute from '@/app/(protected)/profile-setup/hoan-tat';
import OnboardingRoute from '@/app/(protected)/onboarding';
import NotFound from '@/app/+not-found';
import { ApiError } from '@/core/api/api-error';
import { authApi } from '@/core/api/auth-api';
import { danhMucApi, type DanhMucNhom } from '@/shared/api/danh-muc-api';
import * as DocumentPicker from 'expo-document-picker';
import { profileApi, type HoSo } from '@/features/profile/api/profile-api';
import * as uploader from '@/shared/file/file-uploader';
import type { AuthUser } from '@/core/auth/types';

const user: AuthUser = {
  id: '0193f0a0-0000-7000-8000-000000000013',
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
  {
    ma_dinh_danh: 'muc_tieu_tap_luyen',
    items: [item('tang_co', 'Tăng cơ'), item('giam_mo', 'Giảm mỡ')],
  },
  {
    ma_dinh_danh: 'chuyen_mon_pt',
    items: [
      item('tang_co', 'Tăng cơ'),
      item('dinh_duong', 'Dinh dưỡng'),
      item('giam_mo', 'Giảm mỡ'),
    ],
  },
  {
    ma_dinh_danh: 'trinh_do_tap',
    items: [item('moi_bat_dau', 'Mới bắt đầu'), item('co_nen_tang', 'Có nền tảng')],
  },
];

const saved: HoSo = {
  loai_nguoi_dung: 'hoc_vien',
  ten_hien_thi: 'Minh Anh',
  ngay_sinh: '1998-05-20',
  gioi_tinh: 'nu',
  trinh_do_tap: 'co_nen_tang',
  muc_tieu_tap_luyen: 'tang_co',
  khu_vuc_hoat_dong: 'ha_noi',
  chuyen_mon: null,
  so_nam_kinh_nghiem: null,
  gioi_thieu: null,
  trang_thai_xac_minh: 'chua_xac_minh',
  chi_so_co_the: { ngay_do: '2026-11-16T00:00:00Z', chieu_cao_cm: 175, can_nang_kg: 70 },
  chung_chi: null,
};

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
  '(protected)/profile-setup/hoan-tat': ProfileDoneRoute,
  '(protected)/onboarding': OnboardingRoute,
  '+not-found': NotFound,
};

const SAVE = { name: 'Lưu thiết lập' };

async function signedIn(url: string) {
  jest
    .mocked(SecureStore.getItemAsync)
    .mockResolvedValue(
      JSON.stringify({ accessToken: 'stored-access', refreshToken: 'stored-refresh' }),
    );
  jest.spyOn(authApi, 'me').mockResolvedValue(user);
  await renderRouter(routes, { initialUrl: url });
}

/** Mở ô chọn khi danh mục đã tải xong (lúc đang tải ô bị vô hiệu và bấm không có tác dụng). */
async function openSelect(label: string) {
  const field = await screen.findByRole('button', { name: label });
  await waitFor(() => expect(field).not.toBeDisabled());
  await fireEvent.press(field);
}

async function choose(label: string, option: string) {
  await openSelect(label);
  await fireEvent.press(await screen.findByRole('radio', { name: option }));
}

/** Đi qua màn thông tin cá nhân (điền hợp lệ) tới màn 03B. */
async function openDetail(loai = 'hoc_vien') {
  await signedIn(`/profile-setup/ca-nhan?loai=${loai}`);
  expect(await screen.findByText('Thông tin cá nhân')).toBeTruthy();
  await fireEvent.changeText(screen.getByLabelText('Ngày sinh'), '20051998');
  await choose('Giới tính', 'Nữ');
  await choose('Khu vực', 'Hà Nội');
  await fireEvent.press(screen.getByRole('button', { name: 'Tiếp tục' }));
  await waitFor(() => expect(screen).toHavePathname('/profile-setup/chi-tiet'));
  expect(await screen.findByText('Thiết lập hồ sơ')).toBeTruthy();
}

async function fillValid(overrides: { chieuCao?: string; canNang?: string } = {}) {
  await choose('Mục tiêu tập luyện *', 'Tăng cơ');
  await choose('Trình độ tập *', 'Có nền tảng');
  await fireEvent.changeText(screen.getByLabelText('Chiều cao *'), overrides.chieuCao ?? '175');
  await fireEvent.changeText(screen.getByLabelText('Cân nặng *'), overrides.canNang ?? '70');
}

/** Điền khối PT hợp lệ: 2 chuyên môn, 4 năm kinh nghiệm, có giới thiệu. */
async function fillPt() {
  await fireEvent.press(await screen.findByRole('checkbox', { name: 'Tăng cơ' }));
  await fireEvent.press(screen.getByRole('checkbox', { name: 'Dinh dưỡng' }));
  await fireEvent.changeText(screen.getByLabelText('Số năm kinh nghiệm *'), '4');
  await fireEvent.changeText(screen.getByLabelText('Giới thiệu bản thân'), '  HLV 4 năm.  ');
}

beforeEach(() => {
  jest.spyOn(danhMucApi, 'get').mockResolvedValue(categories);
});

afterEach(() => {
  jest.restoreAllMocks();
});

test('opening the screen directly without the personal info goes back to that step', async () => {
  await signedIn('/profile-setup/chi-tiet?loai=pt');

  await waitFor(() => expect(screen).toHavePathname('/profile-setup/ca-nhan'));
  expect(screen).toHaveSearchParams({ loai: 'pt' });
});

test('a learner sees only the learner block, with units and no tabs', async () => {
  await openDetail('hoc_vien');

  expect(screen.getByText('Thiết lập hồ sơ')).toBeTruthy();
  expect(
    screen.getByText(
      'Hoàn thiện thông tin để cá nhân hóa trải nghiệm và sử dụng đầy đủ tính năng.',
    ),
  ).toBeTruthy();
  expect(screen.getByText('Thông tin học viên')).toBeTruthy();
  expect(screen.getByText('cm')).toBeTruthy();
  expect(screen.getByText('kg')).toBeTruthy();
  expect(screen.queryByRole('tab')).toBeNull();
  expect(screen.queryByText('Thông tin PT')).toBeNull();
  expect(screen.queryByLabelText('Số năm kinh nghiệm *')).toBeNull();
  expect(screen.queryByText('Khám phá')).toBeNull();
  expect(screen.queryByRole('button', { name: 'Để sau' })).toBeNull();
});

test('a PT sees only the PT block, with no tabs and no body measurements', async () => {
  await openDetail('pt');

  expect(screen.getByText('Thông tin PT')).toBeTruthy();
  expect(await screen.findByRole('checkbox', { name: 'Tăng cơ' })).toBeTruthy();
  expect(screen.getByLabelText('Số năm kinh nghiệm *')).toBeTruthy();
  expect(screen.getByLabelText('Giới thiệu bản thân')).toBeTruthy();
  expect(screen.queryByRole('tab')).toBeNull();
  expect(screen.queryByText('Thông tin học viên')).toBeNull();
  expect(screen.queryByLabelText('Chiều cao *')).toBeNull();
  expect(screen.queryByLabelText('Cân nặng *')).toBeNull();
});

test('choosing both roles shows two tabs and switching keeps what was typed in each', async () => {
  await openDetail('ca_hai');

  expect(screen.getAllByRole('tab').map((tab) => tab.props.accessibilityLabel)).toEqual([
    'Hồ sơ học viên',
    'Hồ sơ PT',
  ]);
  expect(screen.getByRole('tab', { name: 'Hồ sơ học viên' })).toBeSelected();
  expect(screen.getByText('Thông tin học viên')).toBeTruthy();
  await fireEvent.changeText(screen.getByLabelText('Chiều cao *'), '175');

  await fireEvent.press(screen.getByRole('tab', { name: 'Hồ sơ PT' }));

  expect(screen.getByRole('tab', { name: 'Hồ sơ PT' })).toBeSelected();
  expect(screen.getByText('Thông tin PT')).toBeTruthy();
  expect(screen.queryByLabelText('Chiều cao *')).toBeNull();
  await fireEvent.changeText(screen.getByLabelText('Số năm kinh nghiệm *'), '7');

  await fireEvent.press(screen.getByRole('tab', { name: 'Hồ sơ học viên' }));
  expect(screen.getByLabelText('Chiều cao *')).toHaveDisplayValue('175');
  await fireEvent.press(screen.getByRole('tab', { name: 'Hồ sơ PT' }));
  expect(screen.getByLabelText('Số năm kinh nghiệm *')).toHaveDisplayValue('7');
});

test('an empty form shows every error and does not call the API', async () => {
  const save = jest.spyOn(profileApi, 'save');
  await openDetail();

  await fireEvent.press(screen.getByRole('button', SAVE));

  expect(await screen.findByText('Vui lòng chọn mục tiêu tập luyện.')).toBeTruthy();
  expect(screen.getByText('Vui lòng chọn trình độ tập.')).toBeTruthy();
  expect(screen.getByText('Vui lòng nhập chiều cao.')).toBeTruthy();
  expect(screen.getByText('Vui lòng nhập cân nặng.')).toBeTruthy();
  expect(save).not.toHaveBeenCalled();
});

test.each([
  ['99', '70', 'Chiều cao phải trong khoảng 100–250 cm.'],
  ['251', '70', 'Chiều cao phải trong khoảng 100–250 cm.'],
  ['abc', '70', 'Chiều cao phải là số.'],
  ['175', '19', 'Cân nặng phải trong khoảng 20–300 kg.'],
  ['175', '301', 'Cân nặng phải trong khoảng 20–300 kg.'],
])('height %s and weight %s is rejected: %s', async (chieuCao, canNang, message) => {
  const save = jest.spyOn(profileApi, 'save');
  await openDetail();
  await fillValid({ chieuCao, canNang });

  await fireEvent.press(screen.getByRole('button', SAVE));

  expect(await screen.findByText(message)).toBeTruthy();
  expect(save).not.toHaveBeenCalled();
});

test('a learner saving sends one request with the personal and learner data, then opens Today for good', async () => {
  const save = jest.spyOn(profileApi, 'save').mockResolvedValue(saved);
  await openDetail('hoc_vien');
  await fillValid({ chieuCao: '172,5', canNang: '68,4' });

  await fireEvent.press(screen.getByRole('button', SAVE));

  await waitFor(() => expect(screen).toHavePathname('/profile-setup/hoan-tat'));
  expect(save).toHaveBeenCalledTimes(1);
  expect(save).toHaveBeenCalledWith({
    loai_nguoi_dung: 'hoc_vien',
    ten_hien_thi: 'Minh Anh',
    ngay_sinh: '1998-05-20',
    gioi_tinh: 'nu',
    khu_vuc_hoat_dong: 'ha_noi',
    muc_tieu_tap_luyen: 'tang_co',
    trinh_do_tap: 'co_nen_tang',
    chieu_cao_cm: 172.5,
    can_nang_kg: 68.4,
  });
  expect(screen.queryByText('Thiết lập hồ sơ')).toBeNull();
});

test('going back to the personal info keeps both steps, and forward again keeps the learner fields', async () => {
  await openDetail();
  await fillValid();

  await fireEvent.press(screen.getByRole('button', { name: 'Quay lại' }));
  await waitFor(() => expect(screen).toHavePathname('/profile-setup/ca-nhan'));
  expect(screen.getByLabelText('Ngày sinh')).toHaveDisplayValue('20/05/1998');

  await fireEvent.press(screen.getByRole('button', { name: 'Tiếp tục' }));

  await waitFor(() => expect(screen).toHavePathname('/profile-setup/chi-tiet'));
  expect(await screen.findByLabelText('Chiều cao *')).toHaveDisplayValue('175');
  expect(screen.getByLabelText('Cân nặng *')).toHaveDisplayValue('70');
  expect(screen.getByRole('button', { name: 'Mục tiêu tập luyện *' })).toHaveAccessibilityValue({
    text: 'Tăng cơ',
  });
});

test('a server error on a field of this screen shows under that field', async () => {
  jest.spyOn(profileApi, 'save').mockRejectedValue(
    new ApiError('Thông tin chưa hợp lệ.', 400, 'VALIDATION_FAILED', {
      chieu_cao_cm: ['Chiều cao phải trong khoảng 100–250 cm.'],
    }),
  );
  await openDetail();
  await fillValid();

  await fireEvent.press(screen.getByRole('button', SAVE));

  expect(await screen.findByText('Chiều cao phải trong khoảng 100–250 cm.')).toBeTruthy();
  expect(screen).toHavePathname('/profile-setup/chi-tiet');
});

test('a server error on a personal field sends the user back to fix it, keeping what was typed', async () => {
  jest.spyOn(profileApi, 'save').mockRejectedValue(
    new ApiError('Thông tin chưa hợp lệ.', 400, 'VALIDATION_FAILED', {
      ngay_sinh: ['Tuổi phải trong khoảng 13–100.'],
    }),
  );
  await openDetail();
  await fillValid();

  await fireEvent.press(screen.getByRole('button', SAVE));
  expect(await screen.findByText('Thông tin chưa hợp lệ.')).toBeTruthy();
  await fireEvent.press(screen.getByRole('button', { name: 'Sửa thông tin cá nhân' }));

  await waitFor(() => expect(screen).toHavePathname('/profile-setup/ca-nhan'));
  expect(screen.getByLabelText('Ngày sinh')).toHaveDisplayValue('20/05/1998');
});

test('a network error keeps the data and Retry sends it again', async () => {
  const save = jest
    .spyOn(profileApi, 'save')
    .mockRejectedValueOnce(
      new ApiError('Không thể kết nối. Kiểm tra mạng và thử lại.', undefined, 'NETWORK'),
    )
    .mockResolvedValueOnce(saved);
  await openDetail();
  await fillValid();

  await fireEvent.press(screen.getByRole('button', SAVE));

  expect(await screen.findByText('Không thể kết nối. Kiểm tra mạng và thử lại.')).toBeTruthy();
  expect(screen.getByLabelText('Chiều cao *')).toHaveDisplayValue('175');
  expect(screen).toHavePathname('/profile-setup/chi-tiet');

  await fireEvent.press(screen.getByRole('button', { name: 'Thử lại' }));

  await waitFor(() => expect(screen).toHavePathname('/profile-setup/hoan-tat'));
  expect(save).toHaveBeenCalledTimes(2);
});

test('when the categories cannot be loaded the dropdowns say so and height and weight stay usable', async () => {
  // Chỉ danh mục của màn này lỗi; màn cá nhân vẫn tải được khu vực để đi tiếp.
  jest.mocked(danhMucApi.get).mockImplementation(async (maDinhDanhs) => {
    if (maDinhDanhs.includes('muc_tieu_tap_luyen')) {
      throw new ApiError('Không có quyền.', 403, 'FORBIDDEN');
    }
    return categories;
  });
  await openDetail();

  expect(
    (await screen.findAllByText('Không tải được danh sách. Vui lòng thử lại.')).length,
  ).toBeGreaterThan(0);
  await fireEvent.changeText(screen.getByLabelText('Chiều cao *'), '175');
  expect(screen.getByLabelText('Chiều cao *')).toHaveDisplayValue('175');
});

test('a PT saving sends the PT block only, with no learner fields', async () => {
  const save = jest
    .spyOn(profileApi, 'save')
    .mockResolvedValue({ ...saved, loai_nguoi_dung: 'pt' });
  await openDetail('pt');
  await fillPt();

  await fireEvent.press(screen.getByRole('button', SAVE));

  await waitFor(() => expect(screen).toHavePathname('/profile-setup/hoan-tat'));
  expect(save).toHaveBeenCalledTimes(1);
  expect(save).toHaveBeenCalledWith({
    loai_nguoi_dung: 'pt',
    ten_hien_thi: 'Minh Anh',
    ngay_sinh: '1998-05-20',
    gioi_tinh: 'nu',
    khu_vuc_hoat_dong: 'ha_noi',
    chuyen_mon: ['tang_co', 'dinh_duong'],
    so_nam_kinh_nghiem: 4,
    gioi_thieu: 'HLV 4 năm.',
  });
});

test('an empty PT form shows its errors and does not call the API', async () => {
  const save = jest.spyOn(profileApi, 'save');
  await openDetail('pt');

  await fireEvent.press(screen.getByRole('button', SAVE));

  expect(await screen.findByText('Vui lòng chọn ít nhất 1 chuyên môn.')).toBeTruthy();
  expect(screen.getByText('Vui lòng nhập số năm kinh nghiệm.')).toBeTruthy();
  expect(save).not.toHaveBeenCalled();
});

test.each([
  ['61', 'Số năm kinh nghiệm phải trong khoảng 0–60.'],
  ['2,5', 'Số năm kinh nghiệm phải là số nguyên.'],
  ['abc', 'Số năm kinh nghiệm phải là số.'],
])('PT experience %s is rejected: %s', async (soNam, message) => {
  const save = jest.spyOn(profileApi, 'save');
  await openDetail('pt');
  await fillPt();
  await fireEvent.changeText(screen.getByLabelText('Số năm kinh nghiệm *'), soNam);

  await fireEvent.press(screen.getByRole('button', SAVE));

  expect(await screen.findByText(message)).toBeTruthy();
  expect(save).not.toHaveBeenCalled();
});

test('a PT with zero years of experience and no introduction can save', async () => {
  const save = jest.spyOn(profileApi, 'save').mockResolvedValue(saved);
  await openDetail('pt');
  await fireEvent.press(await screen.findByRole('checkbox', { name: 'Giảm mỡ' }));
  await fireEvent.changeText(screen.getByLabelText('Số năm kinh nghiệm *'), '0');

  await fireEvent.press(screen.getByRole('button', SAVE));

  await waitFor(() => expect(screen).toHavePathname('/profile-setup/hoan-tat'));
  expect(save).toHaveBeenCalledWith(
    expect.objectContaining({
      chuyen_mon: ['giam_mo'],
      so_nam_kinh_nghiem: 0,
      gioi_thieu: undefined,
    }),
  );
});

test('a chip can be turned off again', async () => {
  const save = jest.spyOn(profileApi, 'save');
  await openDetail('pt');
  await fireEvent.press(await screen.findByRole('checkbox', { name: 'Tăng cơ' }));
  expect(screen.getByRole('checkbox', { name: 'Tăng cơ' })).toBeChecked();

  await fireEvent.press(screen.getByRole('checkbox', { name: 'Tăng cơ' }));

  expect(screen.getByRole('checkbox', { name: 'Tăng cơ' })).not.toBeChecked();
  await fireEvent.changeText(screen.getByLabelText('Số năm kinh nghiệm *'), '2');
  await fireEvent.press(screen.getByRole('button', SAVE));
  expect(await screen.findByText('Vui lòng chọn ít nhất 1 chuyên môn.')).toBeTruthy();
  expect(save).not.toHaveBeenCalled();
});

test('choosing both roles saves both blocks in one request', async () => {
  const save = jest
    .spyOn(profileApi, 'save')
    .mockResolvedValue({ ...saved, loai_nguoi_dung: 'ca_hai' });
  await openDetail('ca_hai');
  await fillValid();
  await fireEvent.press(screen.getByRole('tab', { name: 'Hồ sơ PT' }));
  await fillPt();

  await fireEvent.press(screen.getByRole('button', SAVE));

  await waitFor(() => expect(screen).toHavePathname('/profile-setup/hoan-tat'));
  expect(save).toHaveBeenCalledTimes(1);
  expect(save).toHaveBeenCalledWith({
    loai_nguoi_dung: 'ca_hai',
    ten_hien_thi: 'Minh Anh',
    ngay_sinh: '1998-05-20',
    gioi_tinh: 'nu',
    khu_vuc_hoat_dong: 'ha_noi',
    muc_tieu_tap_luyen: 'tang_co',
    trinh_do_tap: 'co_nen_tang',
    chieu_cao_cm: 175,
    can_nang_kg: 70,
    chuyen_mon: ['tang_co', 'dinh_duong'],
    so_nam_kinh_nghiem: 4,
    gioi_thieu: 'HLV 4 năm.',
  });
});

test('when both roles are chosen and the other tab has errors, saving switches to it and marks it', async () => {
  const save = jest.spyOn(profileApi, 'save');
  await openDetail('ca_hai');
  await fillValid();

  await fireEvent.press(screen.getByRole('button', SAVE));

  // Tab học viên đã đủ, tab PT còn trống: chuyển sang tab PT và hiện lỗi ở đó.
  expect(await screen.findByText('Vui lòng chọn ít nhất 1 chuyên môn.')).toBeTruthy();
  expect(screen.getByRole('tab', { name: 'Hồ sơ PT' })).toBeSelected();
  expect(save).not.toHaveBeenCalled();
});

test('a server error on a PT field shows under it, in the PT tab', async () => {
  jest.spyOn(profileApi, 'save').mockRejectedValue(
    new ApiError('Thông tin chưa hợp lệ.', 400, 'VALIDATION_FAILED', {
      chuyen_mon: ['Mã chuyên môn PT không hợp lệ.'],
    }),
  );
  await openDetail('ca_hai');
  await fillValid();
  await fireEvent.press(screen.getByRole('tab', { name: 'Hồ sơ PT' }));
  await fillPt();
  await fireEvent.press(screen.getByRole('tab', { name: 'Hồ sơ học viên' }));

  await fireEvent.press(screen.getByRole('button', SAVE));

  expect(await screen.findByText('Mã chuyên môn PT không hợp lệ.')).toBeTruthy();
  expect(screen.getByRole('tab', { name: 'Hồ sơ PT' })).toBeSelected();
});

const PDF = { uri: 'file:///cache/PT_Level1_FI.pdf', name: 'PT_Level1_FI.pdf', size: 2048 };

function pickPdfOnce(asset: { uri: string; name: string; size?: number } = PDF) {
  jest
    .mocked(DocumentPicker.getDocumentAsync)
    .mockResolvedValueOnce({ canceled: false, assets: [asset] } as never);
}

function uploadResult(
  overrides: Partial<{ file_id: string; original_name: string; size: number }> = {},
) {
  return {
    file_id: '0193f0a0-0000-7000-8000-0000000000f1',
    original_name: 'PT_Level1_FI.pdf',
    content_type: 'application/pdf',
    size: 2048,
    purpose: 'chung_chi_pt',
    ...overrides,
  };
}

test('the PT block has an optional certificate field under years of experience', async () => {
  await openDetail('pt');

  expect(await screen.findByText('Chứng chỉ (nếu có)')).toBeTruthy();
  expect(screen.getByRole('button', { name: 'Chọn tệp PDF chứng chỉ' })).toBeTruthy();
  expect(screen.getByText('Chỉ nhận tệp PDF, tối đa 10,0 MB. Tệp được lưu riêng tư.')).toBeTruthy();
});

test('a learner has no certificate field', async () => {
  await openDetail('hoc_vien');

  await screen.findByText('Thông tin học viên');
  expect(screen.queryByText('Chứng chỉ (nếu có)')).toBeNull();
});

test('picking a PDF uploads it as a private certificate and saving sends its file id', async () => {
  const upload = jest.spyOn(uploader, 'uploadMultipartFile').mockResolvedValue(uploadResult());
  const save = jest
    .spyOn(profileApi, 'save')
    .mockResolvedValue({ ...saved, loai_nguoi_dung: 'pt' });
  await openDetail('pt');
  await fillPt();
  pickPdfOnce();

  await fireEvent.press(screen.getByRole('button', { name: 'Chọn tệp PDF chứng chỉ' }));

  expect(await screen.findByText('PT_Level1_FI.pdf')).toBeTruthy();
  expect(screen.getByText('PDF · 2 KB')).toBeTruthy();
  expect(upload).toHaveBeenCalledWith(PDF.uri, 'chung_chi_pt', {
    contentType: 'application/pdf',
    fileName: 'PT_Level1_FI.pdf',
    sizeBytes: 2048,
  });

  await fireEvent.press(screen.getByRole('button', SAVE));

  await waitFor(() => expect(screen).toHavePathname('/profile-setup/hoan-tat'));
  expect(save).toHaveBeenCalledWith(
    expect.objectContaining({ chung_chi_tep_tin_id: '0193f0a0-0000-7000-8000-0000000000f1' }),
  );
});

test('removing the certificate with the cross sends no file id', async () => {
  jest.spyOn(uploader, 'uploadMultipartFile').mockResolvedValue(uploadResult());
  const save = jest
    .spyOn(profileApi, 'save')
    .mockResolvedValue({ ...saved, loai_nguoi_dung: 'pt' });
  await openDetail('pt');
  await fillPt();
  pickPdfOnce();
  await fireEvent.press(screen.getByRole('button', { name: 'Chọn tệp PDF chứng chỉ' }));
  await fireEvent.press(await screen.findByRole('button', { name: 'Xóa chứng chỉ' }));

  expect(screen.queryByText('PT_Level1_FI.pdf')).toBeNull();
  await fireEvent.press(screen.getByRole('button', SAVE));

  await waitFor(() => expect(screen).toHavePathname('/profile-setup/hoan-tat'));
  expect(save.mock.calls[0]?.[0].chung_chi_tep_tin_id).toBeUndefined();
});

test('cancelling the file picker changes nothing', async () => {
  const upload = jest.spyOn(uploader, 'uploadMultipartFile');
  await openDetail('pt');

  await fireEvent.press(await screen.findByRole('button', { name: 'Chọn tệp PDF chứng chỉ' }));

  expect(upload).not.toHaveBeenCalled();
  expect(screen.getByRole('button', { name: 'Chọn tệp PDF chứng chỉ' })).toBeTruthy();
});

test('a PDF over 10 MB is rejected before uploading', async () => {
  const upload = jest.spyOn(uploader, 'uploadMultipartFile');
  await openDetail('pt');
  pickPdfOnce({ ...PDF, size: 11 * 1024 * 1024 });

  await fireEvent.press(await screen.findByRole('button', { name: 'Chọn tệp PDF chứng chỉ' }));

  expect(await screen.findByText('Tệp quá lớn. Tối đa 10,0 MB.')).toBeTruthy();
  expect(upload).not.toHaveBeenCalled();
});

test('a failed upload shows an error and keeps the field empty so it can be retried', async () => {
  jest.spyOn(uploader, 'uploadMultipartFile').mockRejectedValue(new Error('Upload thất bại'));
  await openDetail('pt');
  pickPdfOnce();

  await fireEvent.press(await screen.findByRole('button', { name: 'Chọn tệp PDF chứng chỉ' }));

  expect(
    await screen.findByText(/^Không tải được tệp lên. Kiểm tra mạng và thử lại./),
  ).toBeTruthy();
  expect(screen.getByRole('button', { name: 'Chọn tệp PDF chứng chỉ' })).toBeTruthy();
});

test('an upload rejected by the server explains the cause by HTTP status', async () => {
  jest
    .spyOn(uploader, 'uploadMultipartFile')
    .mockRejectedValue(new uploader.UploadError('Upload file multipart thất bại (401): {}', 401));
  await openDetail('pt');
  pickPdfOnce();

  await fireEvent.press(await screen.findByRole('button', { name: 'Chọn tệp PDF chứng chỉ' }));

  expect(
    await screen.findByText('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.'),
  ).toBeTruthy();
});

test('a server error on the certificate shows under the field, in the PT tab', async () => {
  jest.spyOn(uploader, 'uploadMultipartFile').mockResolvedValue(uploadResult());
  jest.spyOn(profileApi, 'save').mockRejectedValue(
    new ApiError('Thông tin chưa hợp lệ.', 400, 'VALIDATION_FAILED', {
      chung_chi_tep_tin_id: [
        'Tệp chứng chỉ không hợp lệ hoặc đã hết hạn. Vui lòng chọn lại tệp PDF.',
      ],
    }),
  );
  await openDetail('pt');
  await fillPt();
  pickPdfOnce();
  await fireEvent.press(screen.getByRole('button', { name: 'Chọn tệp PDF chứng chỉ' }));
  await screen.findByText('PT_Level1_FI.pdf');

  await fireEvent.press(screen.getByRole('button', SAVE));

  expect(
    await screen.findByText(
      'Tệp chứng chỉ không hợp lệ hoặc đã hết hạn. Vui lòng chọn lại tệp PDF.',
    ),
  ).toBeTruthy();
  expect(screen).toHavePathname('/profile-setup/chi-tiet');
});

test('after saving a PT profile the done screen says it is waiting for verification', async () => {
  jest.spyOn(profileApi, 'save').mockResolvedValue({ ...saved, loai_nguoi_dung: 'pt' });
  await openDetail('pt');
  await fillPt();

  await fireEvent.press(screen.getByRole('button', SAVE));

  expect(await screen.findByText('Đã gửi hồ sơ PT!')).toBeTruthy();
  expect(screen.getByText('Chờ xác minh')).toBeTruthy();
  expect(screen.getByText(/đang chờ đội ngũ GymBase xác minh/)).toBeTruthy();
  await fireEvent.press(screen.getByRole('button', { name: 'Về Trang chủ' }));
  await waitFor(() => expect(screen).toHavePathname('/home'));
});

test('after saving a learner profile the done screen welcomes the user to GymBase', async () => {
  jest.spyOn(profileApi, 'save').mockResolvedValue(saved);
  await openDetail('hoc_vien');
  await fillValid();

  await fireEvent.press(screen.getByRole('button', SAVE));

  expect(await screen.findByText('Chào mừng Anh đến với GymBase!')).toBeTruthy();
  expect(screen.getByText('Bảng tin cộng đồng')).toBeTruthy();
  expect(screen.queryByText('Chờ xác minh')).toBeNull();
  await fireEvent.press(screen.getByRole('button', { name: 'Bắt đầu khám phá' }));
  await waitFor(() => expect(screen).toHavePathname('/home'));
});
