import { fireEvent, render, screen } from '@testing-library/react-native';
import {
  datLaiKhoCheckIn,
  layCheckInBuoiTap,
  luuCheckInBuoiTap,
} from '@/features/lo-trinh/data/check-in-store';
import { CheckInBuoiTapScreen } from '@/features/lo-trinh/screens/check-in-buoi-tap-screen';
import { ChiTietCheckInBuoiTapScreen } from '@/features/lo-trinh/screens/chi-tiet-check-in-buoi-tap-screen';
import { AppProviders } from '@/providers/app-providers';

// Animation chạy trên luồng UI của native; trong test dùng bản giả (xem reanimated-mock.js).
// eslint-disable-next-line @typescript-eslint/no-require-imports
jest.mock('react-native-reanimated', () => require('react-native-reanimated/mock'));

const mockPush = jest.fn();
const mockBack = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ push: mockPush, back: mockBack, canGoBack: () => true }),
}));

const mockLaunchLibrary = jest.fn();
jest.mock('expo-image-picker', () => ({
  requestCameraPermissionsAsync: jest.fn(async () => ({ granted: true })),
  requestMediaLibraryPermissionsAsync: jest.fn(async () => ({ granted: true })),
  launchCameraAsync: jest.fn(async () => ({ canceled: true })),
  launchImageLibraryAsync: (...args: unknown[]) => mockLaunchLibrary(...args),
}));

const LO_TRINH = 'ct-1';
/** Buổi tập Upper A 17:30 chưa check-in. */
const CHUA_CHECK_IN = 'nv-3';
/** Buổi tập Cardio & Core 06:00 đã hoàn thành và được duyệt. */
const DA_DUYET = 'nv-7';

function veMan(man: React.ReactElement) {
  return render(<AppProviders>{man}</AppProviders>);
}

beforeEach(() => datLaiKhoCheckIn());

describe('CheckInBuoiTapScreen (HV-06)', () => {
  const veForm = () =>
    veMan(<CheckInBuoiTapScreen loTrinhId={LO_TRINH} nhiemVuId={CHUA_CHECK_IN} />);

  test('hiện thông tin buổi, bài tập và mặc định chọn "Đã tập"', async () => {
    await veForm();
    expect(screen.getByText('Check-in buổi tập')).toBeTruthy();
    expect(screen.getByText('Lộ trình: Tăng 10 kg trong 3 tháng')).toBeTruthy();
    expect(screen.getByText('Upper A')).toBeTruthy();
    expect(screen.getByText('17:30')).toBeTruthy();
    expect(screen.getByText('Ngực – Vai – Tay sau')).toBeTruthy();
    expect(screen.getByText('60 phút')).toBeTruthy();
    expect(screen.getByText('Bài tập hôm nay')).toBeTruthy();
    expect(screen.getByText('Bench Press')).toBeTruthy();
    expect(screen.getByText('4 set × 8–10 rep')).toBeTruthy();
    expect(screen.getAllByRole('checkbox')).toHaveLength(4);
    expect(screen.getByRole('radio', { name: 'Đã tập' }).props.accessibilityState.checked).toBe(
      true,
    );
    expect(screen.getByRole('radio', { name: 'Nghỉ tập' }).props.accessibilityState.checked).toBe(
      false,
    );
  });

  test('bấm một bài để tick rồi bấm lại để bỏ tick', async () => {
    await veForm();
    const bai = () => screen.getByRole('checkbox', { name: 'Bench Press 4 set × 8–10 rep' });
    expect(bai().props.accessibilityState.checked).toBe(false);
    await fireEvent.press(bai());
    expect(bai().props.accessibilityState.checked).toBe(true);
    await fireEvent.press(bai());
    expect(bai().props.accessibilityState.checked).toBe(false);
  });

  test('hoàn thành buổi tập thì lưu các bài đã tick và ghi chú, chờ PT duyệt', async () => {
    await veForm();
    await fireEvent.press(screen.getByRole('checkbox', { name: 'Bench Press 4 set × 8–10 rep' }));
    await fireEvent.press(screen.getByRole('checkbox', { name: 'Shoulder Press 3 set × 10 rep' }));
    await fireEvent.changeText(screen.getByLabelText('Ghi chú gửi PT'), 'Cảm giác ổn.');
    await fireEvent.press(screen.getByLabelText('Hoàn thành buổi tập'));
    expect(mockBack).toHaveBeenCalledTimes(1);
    expect(layCheckInBuoiTap(CHUA_CHECK_IN)).toMatchObject({
      trangThaiBuoi: 'da_tap',
      baiTapXong: ['bt-1', 'bt-3'],
      ghiChu: 'Cảm giác ổn.',
      trangThaiDuyet: 'cho_duyet',
    });
  });

  test('chọn Tập một phần được lưu cùng các bài đã tick', async () => {
    await veForm();
    await fireEvent.press(screen.getByRole('checkbox', { name: 'Bench Press 4 set × 8–10 rep' }));
    await fireEvent.press(screen.getByRole('radio', { name: 'Tập một phần' }));
    await fireEvent.press(screen.getByLabelText('Hoàn thành buổi tập'));
    expect(layCheckInBuoiTap(CHUA_CHECK_IN)).toMatchObject({
      trangThaiBuoi: 'tap_mot_phan',
      baiTapXong: ['bt-1'],
    });
  });

  test('chọn Nghỉ tập thì không tính bài nào dù đã tick', async () => {
    await veForm();
    await fireEvent.press(screen.getByRole('checkbox', { name: 'Bench Press 4 set × 8–10 rep' }));
    await fireEvent.press(screen.getByRole('radio', { name: 'Nghỉ tập' }));
    await fireEvent.press(screen.getByLabelText('Hoàn thành buổi tập'));
    expect(layCheckInBuoiTap(CHUA_CHECK_IN)).toMatchObject({
      trangThaiBuoi: 'nghi_tap',
      baiTapXong: [],
    });
  });

  test('ảnh buổi tập: hai ô chụp ảnh và chọn từ thư viện, tối đa 1 ảnh, xóa được', async () => {
    mockLaunchLibrary.mockResolvedValue({ canceled: false, assets: [{ uri: 'buoi-tap.jpg' }] });
    await veForm();
    expect(screen.getByText('Đã thêm 0/1 ảnh')).toBeTruthy();
    expect(screen.getByLabelText('Chụp ảnh')).toBeTruthy();
    await fireEvent.press(screen.getByLabelText('Chọn từ thư viện'));
    expect(await screen.findByLabelText('Ảnh buổi tập đã thêm')).toBeTruthy();
    expect(screen.getByText('Đã thêm 1/1 ảnh')).toBeTruthy();
    // Đủ 1 ảnh thì hai ô thêm ảnh biến mất.
    expect(screen.queryByLabelText('Chụp ảnh')).toBeNull();
    expect(screen.queryByLabelText('Chọn từ thư viện')).toBeNull();
    await fireEvent.press(screen.getByLabelText('Xóa ảnh'));
    expect(screen.getByText('Đã thêm 0/1 ảnh')).toBeTruthy();
    expect(screen.getByLabelText('Chụp ảnh')).toBeTruthy();
  });

  test('ghi chú có bộ đếm 0/300 như check-in bữa ăn', async () => {
    await veForm();
    expect(screen.getByText('0/300')).toBeTruthy();
    await fireEvent.changeText(screen.getByLabelText('Ghi chú gửi PT'), 'Ổn.');
    expect(screen.getByText('3/300')).toBeTruthy();
  });

  test('buổi tập không tồn tại thì báo không tìm thấy', async () => {
    await veMan(<CheckInBuoiTapScreen loTrinhId={LO_TRINH} nhiemVuId="khong-co" />);
    expect(screen.getByText('Không tìm thấy buổi tập cần check-in.')).toBeTruthy();
  });
});

describe('ChiTietCheckInBuoiTapScreen', () => {
  test('đã tập và đã duyệt: ô kết quả ghi "Buổi tập đã được duyệt", ẩn nút chỉnh sửa', async () => {
    await veMan(<ChiTietCheckInBuoiTapScreen loTrinhId={LO_TRINH} nhiemVuId={DA_DUYET} />);
    expect(screen.getByText('Buổi tập đã được duyệt')).toBeTruthy();
    expect(screen.getByText(/Bạn đã tập buổi này/)).toBeTruthy();
    expect(screen.queryByText('Đang chờ PT duyệt')).toBeNull();
    expect(screen.getByText('Đã hoàn thành 3/3 bài')).toBeTruthy();
    expect(screen.getByText('Đánh giá từ PT')).toBeTruthy();
    expect(screen.queryByLabelText('Chỉnh sửa check-in')).toBeNull();
    expect(screen.getByLabelText('Nhắn PT')).toBeTruthy();
  });

  test('nghỉ tập chờ PT duyệt: báo thất bại, cho chỉnh sửa', async () => {
    luuCheckInBuoiTap(
      CHUA_CHECK_IN,
      {
        trangThaiBuoi: 'nghi_tap',
        baiTapXong: [],
        ghiChu: 'Đau vai nên nghỉ.',
        trangThaiDuyet: 'cho_duyet',
        gioGui: '17:00',
      },
      '',
    );
    await veMan(<ChiTietCheckInBuoiTapScreen loTrinhId={LO_TRINH} nhiemVuId={CHUA_CHECK_IN} />);
    expect(screen.getByText('Nghỉ tập')).toBeTruthy();
    expect(screen.getByText('Đang chờ PT duyệt')).toBeTruthy();
    expect(screen.getByText('Đã hoàn thành 0/4 bài')).toBeTruthy();
    expect(screen.getByText('Đau vai nên nghỉ.')).toBeTruthy();
    expect(screen.getByText('Bạn không gửi ảnh nào.')).toBeTruthy();
    await fireEvent.press(screen.getByLabelText('Chỉnh sửa check-in'));
    expect(mockPush).toHaveBeenCalledWith({
      pathname: '/check-in-buoi-tap',
      params: { loTrinhId: LO_TRINH, nhiemVuId: CHUA_CHECK_IN },
    });
  });

  test('tập một phần và PT yêu cầu bổ sung: hiện yêu cầu của PT', async () => {
    luuCheckInBuoiTap(
      CHUA_CHECK_IN,
      {
        trangThaiBuoi: 'tap_mot_phan',
        baiTapXong: ['bt-1', 'bt-2'],
        ghiChu: '',
        trangThaiDuyet: 'can_bo_sung',
        gioGui: '18:30',
        danhGia: {
          pt: { id: 'pt-1', ten: 'Trần Đức Anh' },
          noiDung: 'Hai bài còn lại bạn bỏ vì lý do gì?',
          thoiDiem: 'Hôm nay, 19:00',
        },
      },
      '',
    );
    await veMan(<ChiTietCheckInBuoiTapScreen loTrinhId={LO_TRINH} nhiemVuId={CHUA_CHECK_IN} />);
    expect(screen.getByText('Tập một phần')).toBeTruthy();
    expect(screen.getByText('PT yêu cầu bổ sung')).toBeTruthy();
    expect(screen.getByText('Yêu cầu bổ sung từ PT')).toBeTruthy();
    expect(screen.getByText('Hai bài còn lại bạn bỏ vì lý do gì?')).toBeTruthy();
    expect(screen.getByText('Đã hoàn thành 2/4 bài')).toBeTruthy();
    expect(screen.getByLabelText('Chỉnh sửa check-in')).toBeTruthy();
  });

  test('buổi tập chưa check-in thì báo chưa có check-in', async () => {
    await veMan(<ChiTietCheckInBuoiTapScreen loTrinhId={LO_TRINH} nhiemVuId={CHUA_CHECK_IN} />);
    expect(screen.getByText('Chưa có check-in cho buổi tập này.')).toBeTruthy();
  });
});
