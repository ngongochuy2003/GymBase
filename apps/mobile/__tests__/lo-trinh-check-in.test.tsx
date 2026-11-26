import { fireEvent, render, screen } from '@testing-library/react-native';
import { KhoiHomNay } from '@/features/lo-trinh/components/khoi-hom-nay';
import { datLaiKhoCheckIn, layCheckIn, luuCheckIn } from '@/features/lo-trinh/data/check-in-store';
import { taoDuLieuMau } from '@/features/lo-trinh/data/seed';
import { CheckInBuaAnScreen } from '@/features/lo-trinh/screens/check-in-bua-an-screen';
import { ChiTietCheckInBuaAnScreen } from '@/features/lo-trinh/screens/chi-tiet-check-in-bua-an-screen';
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

function veMan(man: React.ReactElement) {
  return render(<AppProviders>{man}</AppProviders>);
}

beforeEach(() => datLaiKhoCheckIn());

describe('nhiệm vụ hôm nay mở đúng màn check-in bữa ăn', () => {
  async function veKhoi() {
    const lo = taoDuLieuMau(new Date()).cuaToi[0];
    if (!lo) throw new Error('thiếu lộ trình mẫu');
    await veMan(<KhoiHomNay loTrinhId={lo.id} nhiemVu={lo.nhiemVuHomNay} homNay={new Date()} />);
  }

  test('bữa ăn "Check-in ngay" mở form check-in', async () => {
    await veKhoi();
    await fireEvent.press(screen.getByLabelText(/^Bữa tối – 19:00/));
    expect(mockPush).toHaveBeenCalledWith({
      pathname: '/check-in-bua-an',
      params: { loTrinhId: LO_TRINH, nhiemVuId: 'nv-6' },
    });
  });

  test('bữa ăn đã hoàn thành hoặc không hoàn thành mở chi tiết check-in', async () => {
    await veKhoi();
    await fireEvent.press(screen.getByLabelText(/^Bữa sáng – 07:00/));
    await fireEvent.press(screen.getByLabelText(/^Bữa trưa – 12:00/));
    expect(mockPush).toHaveBeenNthCalledWith(1, {
      pathname: '/chi-tiet-check-in',
      params: { loTrinhId: LO_TRINH, nhiemVuId: 'nv-1' },
    });
    expect(mockPush).toHaveBeenNthCalledWith(2, {
      pathname: '/chi-tiet-check-in',
      params: { loTrinhId: LO_TRINH, nhiemVuId: 'nv-2' },
    });
  });

  test('buổi tập "Check-in ngay" mở form, buổi tập đã xong mở chi tiết', async () => {
    await veKhoi();
    await fireEvent.press(screen.getByLabelText(/^Buổi tập – 17:30/));
    await fireEvent.press(screen.getByLabelText(/^Buổi tập – 06:00/));
    expect(mockPush).toHaveBeenNthCalledWith(1, {
      pathname: '/check-in-buoi-tap',
      params: { loTrinhId: LO_TRINH, nhiemVuId: 'nv-3' },
    });
    expect(mockPush).toHaveBeenNthCalledWith(2, {
      pathname: '/chi-tiet-check-in-buoi-tap',
      params: { loTrinhId: LO_TRINH, nhiemVuId: 'nv-7' },
    });
  });

  test('cân nặng và ảnh cơ thể "Ghi ngay" mở màn ghi, không qua duyệt', async () => {
    await veKhoi();
    expect(screen.getAllByText('Ghi ngay')).toHaveLength(2);
    await fireEvent.press(screen.getByLabelText(/^Cân nặng – Tuần 5/));
    await fireEvent.press(screen.getByLabelText(/^Ảnh cơ thể – Tuần 5/));
    expect(mockPush).toHaveBeenNthCalledWith(1, {
      pathname: '/ghi-can-nang',
      params: { loTrinhId: LO_TRINH, nhiemVuId: 'nv-4' },
    });
    expect(mockPush).toHaveBeenNthCalledWith(2, {
      pathname: '/ghi-anh-co-the',
      params: { loTrinhId: LO_TRINH, nhiemVuId: 'nv-5' },
    });
  });
});

describe('CheckInBuaAnScreen (HV-04)', () => {
  const veForm = () => veMan(<CheckInBuaAnScreen loTrinhId={LO_TRINH} nhiemVuId="nv-6" />);

  test('hiện thông tin bữa và kế hoạch', async () => {
    await veForm();
    expect(screen.getByText('Check-in bữa tối')).toBeTruthy();
    expect(screen.getByText('Lộ trình: Tăng 10 kg trong 3 tháng')).toBeTruthy();
    expect(screen.getByText('Bữa tối')).toBeTruthy();
    expect(screen.getByText('19:00')).toBeTruthy();
    expect(screen.getByText('~ 650 kcal')).toBeTruthy();
    // Không còn các ô dinh dưỡng chi tiết.
    expect(screen.queryByText('Protein')).toBeNull();
    expect(screen.queryByText('Năng lượng')).toBeNull();
  });

  test('gửi khi chưa chọn trạng thái thì báo lỗi và không gửi', async () => {
    await veForm();
    await fireEvent.press(screen.getByLabelText('Gửi PT đánh giá'));
    expect(screen.getByText('Chọn trạng thái bữa ăn')).toBeTruthy();
    expect(mockBack).not.toHaveBeenCalled();
  });

  test('chọn Đã ăn rồi gửi thì lưu check-in chờ PT duyệt và quay lại', async () => {
    await veForm();
    await fireEvent.press(screen.getByRole('radio', { name: 'Đã ăn' }));
    await fireEvent.changeText(screen.getByLabelText('Ghi chú gửi PT'), 'Ăn đủ, thêm quả táo.');
    expect(screen.getByText('20/300')).toBeTruthy();
    await fireEvent.press(screen.getByLabelText('Gửi PT đánh giá'));
    expect(mockBack).toHaveBeenCalledTimes(1);
    expect(layCheckIn('nv-6')).toMatchObject({
      trangThaiBua: 'da_an',
      ghiChu: 'Ăn đủ, thêm quả táo.',
      trangThaiDuyet: 'cho_duyet',
    });
  });

  test('chọn Bỏ bữa rồi gửi thì lưu trạng thái bỏ bữa', async () => {
    await veForm();
    await fireEvent.press(screen.getByRole('radio', { name: 'Bỏ bữa' }));
    await fireEvent.press(screen.getByLabelText('Gửi PT đánh giá'));
    expect(layCheckIn('nv-6')).toMatchObject({ trangThaiBua: 'bo_bua', ghiChu: '' });
  });

  test('chỉ thêm tối đa 3 ảnh và có thể xóa', async () => {
    mockLaunchLibrary.mockResolvedValue({
      canceled: false,
      assets: [{ uri: 'a.jpg' }, { uri: 'b.jpg' }, { uri: 'c.jpg' }, { uri: 'd.jpg' }],
    });
    await veForm();
    await fireEvent.press(screen.getByLabelText('Chọn từ thư viện'));
    expect(await screen.findByText('Đã thêm 3/3 ảnh')).toBeTruthy();
    expect(screen.getAllByLabelText('Ảnh bữa ăn đã thêm')).toHaveLength(3);
    await fireEvent.press(screen.getAllByLabelText('Xóa ảnh')[0] as never);
    expect(screen.getByText('Đã thêm 2/3 ảnh')).toBeTruthy();
    // Còn đúng một chỗ trống thì gom hai ô thêm ảnh thành một ô "Thêm ảnh".
    expect(screen.getByLabelText('Thêm ảnh')).toBeTruthy();
    expect(screen.queryByLabelText('Chụp ảnh')).toBeNull();
  });
});

describe('ChiTietCheckInBuaAnScreen (HV-05)', () => {
  test('đã ăn và đã duyệt: có đánh giá của PT, ẩn nút chỉnh sửa (N13)', async () => {
    await veMan(<ChiTietCheckInBuaAnScreen loTrinhId={LO_TRINH} nhiemVuId="nv-1" />);
    // Đã duyệt thì ô kết quả ghi "Bữa ăn đã được duyệt", dòng phụ vẫn nói rõ kết quả.
    expect(screen.getByText('Bữa ăn đã được duyệt')).toBeTruthy();
    expect(screen.getByText(/Bạn đã ăn bữa này/)).toBeTruthy();
    expect(screen.queryByText('Đã ăn')).toBeNull();
    // Không còn nhãn trạng thái trên thẻ bữa; đã duyệt thì ô kết quả không có dòng chờ duyệt.
    expect(screen.queryByText('Đã duyệt')).toBeNull();
    expect(screen.queryByText('Đang chờ PT duyệt')).toBeNull();
    expect(screen.getByText('Đánh giá từ PT')).toBeTruthy();
    expect(screen.queryByLabelText('Chỉnh sửa check-in')).toBeNull();
    expect(screen.getByLabelText('Nhắn PT')).toBeTruthy();
  });

  test('bỏ bữa và chờ PT duyệt: chưa có nhận xét của PT, cho chỉnh sửa', async () => {
    await veMan(<ChiTietCheckInBuaAnScreen loTrinhId={LO_TRINH} nhiemVuId="nv-2" />);
    // Chưa duyệt thì ô kết quả giữ nguyên "Bỏ bữa".
    expect(screen.getByText('Bỏ bữa')).toBeTruthy();
    expect(screen.queryByText('Bữa ăn đã được duyệt')).toBeNull();
    expect(screen.getByText('Đang chờ PT duyệt')).toBeTruthy();
    expect(screen.queryByText('Cần bổ sung')).toBeNull();
    expect(screen.queryByText('Đánh giá từ PT')).toBeNull();
    expect(screen.queryByText('Yêu cầu bổ sung từ PT')).toBeNull();
    await fireEvent.press(screen.getByLabelText('Chỉnh sửa check-in'));
    expect(mockPush).toHaveBeenCalledWith({
      pathname: '/check-in-bua-an',
      params: { loTrinhId: LO_TRINH, nhiemVuId: 'nv-2' },
    });
  });

  test('PT yêu cầu bổ sung: hiện yêu cầu của PT và cho chỉnh sửa', async () => {
    luuCheckIn(
      'nv-6',
      {
        trangThaiBua: 'bo_bua',
        anh: [],
        ghiChu: '',
        trangThaiDuyet: 'can_bo_sung',
        gioGui: '19:20',
        danhGia: {
          pt: { id: 'pt-1', ten: 'Trần Đức Anh' },
          noiDung: 'Bạn cho mình biết lý do bỏ bữa nhé.',
          thoiDiem: 'Hôm nay, 20:00',
        },
      },
      '',
    );
    await veMan(<ChiTietCheckInBuaAnScreen loTrinhId={LO_TRINH} nhiemVuId="nv-6" />);
    expect(screen.getByText('PT yêu cầu bổ sung')).toBeTruthy();
    expect(screen.getByText('Yêu cầu bổ sung từ PT')).toBeTruthy();
    expect(screen.getByText('Bạn cho mình biết lý do bỏ bữa nhé.')).toBeTruthy();
    expect(screen.getByLabelText('Chỉnh sửa check-in')).toBeTruthy();
  });

  test('check-in vừa gửi (Đã ăn) hiện chờ PT duyệt và cho chỉnh sửa', async () => {
    luuCheckIn(
      'nv-6',
      {
        trangThaiBua: 'da_an',
        anh: [],
        ghiChu: 'Ăn đủ, thêm quả táo.',
        trangThaiDuyet: 'cho_duyet',
        gioGui: '19:20',
      },
      '',
    );
    await veMan(<ChiTietCheckInBuaAnScreen loTrinhId={LO_TRINH} nhiemVuId="nv-6" />);
    expect(screen.getByText('Đã ăn')).toBeTruthy();
    expect(screen.getByText('Đang chờ PT duyệt')).toBeTruthy();
    expect(screen.getByText('Ăn đủ, thêm quả táo.')).toBeTruthy();
    expect(screen.getByText('Bạn không gửi ảnh nào.')).toBeTruthy();
    expect(screen.getByLabelText('Chỉnh sửa check-in')).toBeTruthy();
  });

  test('check-in vừa gửi (Bỏ bữa) hiện kết quả bỏ bữa', async () => {
    luuCheckIn(
      'nv-6',
      { trangThaiBua: 'bo_bua', anh: [], ghiChu: '', trangThaiDuyet: 'cho_duyet', gioGui: '19:20' },
      '',
    );
    await veMan(<ChiTietCheckInBuaAnScreen loTrinhId={LO_TRINH} nhiemVuId="nv-6" />);
    expect(screen.getByText('Bỏ bữa')).toBeTruthy();
    expect(screen.getByText('Bạn không ghi chú gì.')).toBeTruthy();
  });

  test('bữa chưa check-in thì báo chưa có check-in', async () => {
    await veMan(<ChiTietCheckInBuaAnScreen loTrinhId={LO_TRINH} nhiemVuId="nv-6" />);
    expect(screen.getByText('Chưa có check-in cho bữa ăn này.')).toBeTruthy();
  });
});
