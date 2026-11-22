import { fireEvent, render, screen } from '@testing-library/react-native';
import {
  datLaiKhoCheckIn,
  layAnhCoThe,
  layCanNang,
  luuAnhCoThe,
  luuCanNang,
} from '@/features/lo-trinh/data/check-in-store';
import { GhiAnhCoTheScreen } from '@/features/lo-trinh/screens/ghi-anh-co-the-screen';
import { GhiCanNangScreen } from '@/features/lo-trinh/screens/ghi-can-nang-screen';
import { chinhCanNang, docCanNang } from '@/features/lo-trinh/utils/tien-do';
import { AppProviders } from '@/providers/app-providers';

const mockBack = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ push: jest.fn(), back: mockBack, canGoBack: () => true }),
}));

const mockLaunchLibrary = jest.fn();
jest.mock('expo-image-picker', () => ({
  requestCameraPermissionsAsync: jest.fn(async () => ({ granted: true })),
  requestMediaLibraryPermissionsAsync: jest.fn(async () => ({ granted: true })),
  launchCameraAsync: jest.fn(async () => ({ canceled: true })),
  launchImageLibraryAsync: (...args: unknown[]) => mockLaunchLibrary(...args),
}));

const LO_TRINH = 'ct-1';
/** Nhiệm vụ cân nặng Tuần 5 đến giờ ghi. */
const CAN_NANG = 'nv-4';
/** Nhiệm vụ ảnh cơ thể Tuần 5 đến giờ ghi. */
const ANH_CO_THE = 'nv-5';

function veMan(man: React.ReactElement) {
  return render(<AppProviders>{man}</AppProviders>);
}

beforeEach(() => datLaiKhoCheckIn());

describe('tiện ích cân nặng', () => {
  test('docCanNang chấp nhận dấu phẩy hoặc chấm và chặn giá trị vô lý', () => {
    expect(docCanNang('66,8')).toBe(66.8);
    expect(docCanNang('66.84')).toBe(66.8);
    expect(docCanNang(' 70 ')).toBe(70);
    expect(docCanNang('')).toBeNull();
    expect(docCanNang('abc')).toBeNull();
    expect(docCanNang('19,9')).toBeNull();
    expect(docCanNang('301')).toBeNull();
  });

  test('chinhCanNang tăng giảm từng bước, kẹp trong khoảng và dùng số gốc khi chưa nhập được', () => {
    expect(chinhCanNang('66,5', 0.1, 60)).toBe('66,6');
    expect(chinhCanNang('66,5', -0.1, 60)).toBe('66,4');
    expect(chinhCanNang('', 0.1, 60)).toBe('60,1');
    expect(chinhCanNang('20', -0.1, 60)).toBe('20,0');
    expect(chinhCanNang('300', 0.1, 60)).toBe('300,0');
  });
});

describe('GhiCanNangScreen (HV-08)', () => {
  const veForm = () => veMan(<GhiCanNangScreen loTrinhId={LO_TRINH} nhiemVuId={CAN_NANG} />);
  const oNhap = () => screen.getByLabelText('Cân nặng (kg)');

  test('mở với lần cân trước, so sánh và khoảng còn lại tới mục tiêu', async () => {
    await veForm();
    expect(screen.getByText('Ghi cân nặng')).toBeTruthy();
    expect(screen.getByText('Lộ trình: Tăng 10 kg trong 3 tháng')).toBeTruthy();
    expect(screen.getByText('Cân nặng – Tuần 5')).toBeTruthy();
    expect(oNhap().props.value).toBe('66,5');
    expect(screen.getByText('Không đổi so với lần cân trước (66,5 kg, Tuần 8)')).toBeTruthy();
    expect(screen.getByText('Còn 3,5 kg tới mục tiêu 70 kg')).toBeTruthy();
    expect(screen.getByLabelText('Lưu cân nặng')).toBeTruthy();
    expect(screen.getByText('PT xem trong tiến độ')).toBeTruthy();
  });

  test('bấm + và − đổi 0,1 kg và cập nhật so sánh', async () => {
    await veForm();
    await fireEvent.press(screen.getByLabelText('Tăng 0,1 kg'));
    expect(oNhap().props.value).toBe('66,6');
    expect(screen.getByText(/^Tăng 0,1 kg so với lần cân trước/)).toBeTruthy();
    await fireEvent.press(screen.getByLabelText('Giảm 0,1 kg'));
    await fireEvent.press(screen.getByLabelText('Giảm 0,1 kg'));
    expect(oNhap().props.value).toBe('66,4');
    expect(screen.getByText(/^Giảm 0,1 kg so với lần cân trước/)).toBeTruthy();
  });

  test('gõ vượt mục tiêu thì báo đã đạt, ký tự lạ bị bỏ qua', async () => {
    await veForm();
    await fireEvent.changeText(oNhap(), '71');
    expect(screen.getByText('Bạn đã đạt mục tiêu 70 kg')).toBeTruthy();
    await fireEvent.changeText(oNhap(), '7a');
    expect(oNhap().props.value).toBe('71');
  });

  test('số ngoài khoảng thì báo lỗi và không lưu', async () => {
    await veForm();
    await fireEvent.changeText(oNhap(), '5');
    await fireEvent.press(screen.getByLabelText('Lưu cân nặng'));
    expect(screen.getByText('Nhập cân nặng từ 20 đến 300 kg')).toBeTruthy();
    expect(layCanNang(CAN_NANG)).toBeUndefined();
    expect(mockBack).not.toHaveBeenCalled();
  });

  test('lưu thì ghi cân nặng và quay lại, không qua duyệt', async () => {
    await veForm();
    await fireEvent.changeText(oNhap(), '66,8');
    await fireEvent.press(screen.getByLabelText('Lưu cân nặng'));
    expect(layCanNang(CAN_NANG)).toMatchObject({ kg: 66.8 });
    expect(mockBack).toHaveBeenCalledTimes(1);
  });

  test('ảnh cân nặng tùy chọn: thêm ảnh rồi lưu cùng cân nặng', async () => {
    mockLaunchLibrary.mockResolvedValue({ canceled: false, assets: [{ uri: 'mat-can.jpg' }] });
    await veForm();
    expect(screen.getByText(/^Ảnh cân nặng/)).toBeTruthy();
    expect(screen.getByText('Đã thêm 0/1 ảnh')).toBeTruthy();
    await fireEvent.press(screen.getByLabelText('Chọn từ thư viện'));
    expect(await screen.findByLabelText('Ảnh cân nặng đã thêm')).toBeTruthy();
    expect(screen.getByText('Đã thêm 1/1 ảnh')).toBeTruthy();
    await fireEvent.press(screen.getByLabelText('Lưu cân nặng'));
    expect(layCanNang(CAN_NANG)).toMatchObject({ kg: 66.5, anh: 'mat-can.jpg' });
  });

  test('lưu không kèm ảnh vẫn được vì ảnh là tùy chọn', async () => {
    await veForm();
    await fireEvent.press(screen.getByLabelText('Lưu cân nặng'));
    expect(layCanNang(CAN_NANG)).toMatchObject({ kg: 66.5 });
    expect(layCanNang(CAN_NANG)?.anh).toBeUndefined();
  });

  test('mở lại thì hiện ảnh đã ghi và xóa được', async () => {
    luuCanNang(CAN_NANG, { kg: 66.8, anh: 'cu.jpg', gioGhi: '06:50' }, '');
    await veForm();
    expect(screen.getByLabelText('Ảnh cân nặng đã thêm')).toBeTruthy();
    await fireEvent.press(screen.getByLabelText('Xóa ảnh'));
    expect(screen.getByText('Đã thêm 0/1 ảnh')).toBeTruthy();
  });

  test('đã ghi rồi thì mở lại với số đã ghi để cập nhật', async () => {
    luuCanNang(CAN_NANG, { kg: 66.8, gioGhi: '06:50' }, '');
    await veForm();
    expect(screen.getByText('Đã ghi lúc 06:50')).toBeTruthy();
    expect(oNhap().props.value).toBe('66,8');
    expect(screen.getByLabelText('Cập nhật cân nặng')).toBeTruthy();
  });

  test('không phải nhiệm vụ cân nặng thì báo không tìm thấy', async () => {
    await veMan(<GhiCanNangScreen loTrinhId={LO_TRINH} nhiemVuId="nv-1" />);
    expect(screen.getByText('Không tìm thấy nhiệm vụ cân nặng.')).toBeTruthy();
  });
});

describe('GhiAnhCoTheScreen (HV-08)', () => {
  const veForm = () => veMan(<GhiAnhCoTheScreen loTrinhId={LO_TRINH} nhiemVuId={ANH_CO_THE} />);

  test('hiện ảnh mốc trước, hai ô thêm ảnh và mặc định chỉ mình tôi xem', async () => {
    await veForm();
    expect(screen.getByText('Ghi ảnh cơ thể')).toBeTruthy();
    expect(screen.getByText('Ảnh cơ thể – Tuần 5')).toBeTruthy();
    // Ảnh mốc trước và ảnh mới nằm cạnh nhau.
    expect(screen.getByText('Tuần 1')).toBeTruthy();
    // Nhãn tuần của ảnh mới chỉ hiện khi đã có ảnh.
    expect(screen.queryByText('Tuần 5')).toBeNull();
    expect(screen.getByLabelText('Ảnh cơ thể tuần 1')).toBeTruthy();
    expect(screen.getByText('Đã thêm 0/1 ảnh')).toBeTruthy();
    expect(screen.getByLabelText('Chụp ảnh')).toBeTruthy();
    expect(screen.getByLabelText('Chọn từ thư viện')).toBeTruthy();
    expect(
      screen.getByRole('radio', { name: 'Chỉ mình tôi' }).props.accessibilityState.checked,
    ).toBe(true);
    expect(screen.getByRole('radio', { name: 'Cho PT xem' }).props.accessibilityState.checked).toBe(
      false,
    );
  });

  test('lưu khi chưa có ảnh thì báo lỗi', async () => {
    await veForm();
    await fireEvent.press(screen.getByLabelText('Lưu ảnh cơ thể'));
    expect(screen.getByText('Chọn một ảnh cơ thể')).toBeTruthy();
    expect(layAnhCoThe(ANH_CO_THE)).toBeUndefined();
    expect(mockBack).not.toHaveBeenCalled();
  });

  test('chọn ảnh, cho PT xem rồi lưu', async () => {
    mockLaunchLibrary.mockResolvedValue({ canceled: false, assets: [{ uri: 'co-the.jpg' }] });
    await veForm();
    await fireEvent.press(screen.getByLabelText('Chọn từ thư viện'));
    expect(await screen.findByLabelText('Ảnh cơ thể đã thêm')).toBeTruthy();
    expect(screen.getByText('Đã thêm 1/1 ảnh')).toBeTruthy();
    // Ảnh mới thay chỗ hai ô thêm ảnh, nằm cạnh ảnh mốc trước với nhãn tuần của mình.
    expect(screen.getByText('Tuần 5')).toBeTruthy();
    expect(screen.queryByLabelText('Chụp ảnh')).toBeNull();
    expect(screen.queryByLabelText('Chọn từ thư viện')).toBeNull();
    await fireEvent.changeText(screen.getByLabelText('Ghi chú ảnh cơ thể'), 'Sau khi ngủ dậy.');
    expect(screen.getByText('16/300')).toBeTruthy();
    await fireEvent.press(screen.getByRole('radio', { name: 'Cho PT xem' }));
    await fireEvent.press(screen.getByLabelText('Lưu ảnh cơ thể'));
    expect(layAnhCoThe(ANH_CO_THE)).toMatchObject({
      anh: 'co-the.jpg',
      ghiChu: 'Sau khi ngủ dậy.',
      quyenXem: 'cho_pt',
    });
    expect(mockBack).toHaveBeenCalledTimes(1);
  });

  test('đã ghi rồi thì mở lại với ảnh và chế độ xem đã lưu', async () => {
    luuAnhCoThe(
      ANH_CO_THE,
      { anh: 'da-luu.jpg', ghiChu: 'Ảnh cũ.', quyenXem: 'cho_pt', gioGhi: '07:10' },
      '',
    );
    await veForm();
    expect(screen.getByText('Đã ghi lúc 07:10')).toBeTruthy();
    expect(screen.getByLabelText('Ảnh cơ thể đã thêm')).toBeTruthy();
    expect(screen.getByRole('radio', { name: 'Cho PT xem' }).props.accessibilityState.checked).toBe(
      true,
    );
    expect(screen.getByLabelText('Cập nhật ảnh cơ thể')).toBeTruthy();
    expect(screen.getByLabelText('Ghi chú ảnh cơ thể').props.value).toBe('Ảnh cũ.');
    await fireEvent.press(screen.getByLabelText('Xóa ảnh'));
    expect(screen.getByText('Đã thêm 0/1 ảnh')).toBeTruthy();
  });

  test('không phải nhiệm vụ ảnh cơ thể thì báo không tìm thấy', async () => {
    await veMan(<GhiAnhCoTheScreen loTrinhId={LO_TRINH} nhiemVuId="nv-4" />);
    expect(screen.getByText('Không tìm thấy nhiệm vụ ảnh cơ thể.')).toBeTruthy();
  });
});
