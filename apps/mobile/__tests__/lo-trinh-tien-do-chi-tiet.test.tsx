import { act, fireEvent, render, screen } from '@testing-library/react-native';
import { AnhTheoMocScreen } from '@/features/lo-trinh/screens/anh-theo-moc-screen';
import { ChiTietCanNangScreen } from '@/features/lo-trinh/screens/chi-tiet-can-nang-screen';
import { ChiTietLoTrinhScreen } from '@/features/lo-trinh/screens/chi-tiet-lo-trinh-screen';
import { AppProviders } from '@/providers/app-providers';

// Animation chạy trên luồng UI của native; trong test dùng bản giả để giá trị đổi ngay (xem reanimated-mock.js).
// eslint-disable-next-line @typescript-eslint/no-require-imports
jest.mock('react-native-reanimated', () => require('react-native-reanimated/mock'));
const mockPush = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ push: mockPush, back: jest.fn(), canGoBack: () => true }),
}));

beforeEach(() => {
  jest.useFakeTimers();
  mockPush.mockClear();
});
afterEach(() => jest.useRealTimers());

describe('nối từ Chi tiết lộ trình sang hai màn chi tiết', () => {
  test('"Chi tiết" cạnh biểu đồ mở màn theo dõi cân nặng', async () => {
    await render(
      <AppProviders>
        <ChiTietLoTrinhScreen id="ct-1" />
      </AppProviders>,
    );
    await act(async () => {
      jest.runAllTimers();
    });
    await fireEvent.press(screen.getByLabelText('Chi tiết Cân nặng 8 tuần'));
    expect(mockPush).toHaveBeenCalledWith({
      pathname: '/chi-tiet-can-nang',
      params: { id: 'ct-1' },
    });
  });

  test('"Xem tất cả" cạnh ảnh theo mốc mở màn ảnh theo mốc', async () => {
    await render(
      <AppProviders>
        <ChiTietLoTrinhScreen id="ct-1" />
      </AppProviders>,
    );
    await act(async () => {
      jest.runAllTimers();
    });
    await fireEvent.press(screen.getByLabelText('Xem tất cả Ảnh cơ thể theo mốc'));
    expect(mockPush).toHaveBeenCalledWith({ pathname: '/anh-theo-moc', params: { id: 'ct-1' } });
  });
});

describe('ChiTietCanNangScreen', () => {
  test('hiện cân nặng hiện tại, mức đổi so với lúc bắt đầu và còn lại tới mục tiêu', async () => {
    await render(
      <AppProviders>
        <ChiTietCanNangScreen id="ct-1" />
      </AppProviders>,
    );
    await act(async () => {
      jest.runAllTimers();
    });
    expect(screen.getByText('Theo dõi cân nặng')).toBeTruthy();
    expect(screen.getByText('Cân nặng hiện tại')).toBeTruthy();
    expect(screen.getByText('+6,5 kg so với lúc bắt đầu')).toBeTruthy();
    // Bắt đầu 60,0 → hiện tại 66,5 → mục tiêu 70,0: còn 3,5.
    expect(screen.getByText('3,5 kg')).toBeTruthy();
    expect(screen.getByText('70,0 kg')).toBeTruthy();
    expect(screen.getByText('Mỗi tuần')).toBeTruthy();
    expect(screen.getByText('+0,9 kg')).toBeTruthy();
  });

  test('lịch sử liệt kê từng lần cân, mới nhất ở trên, kèm mức đổi so với lần trước', async () => {
    await render(
      <AppProviders>
        <ChiTietCanNangScreen id="ct-1" />
      </AppProviders>,
    );
    await act(async () => {
      jest.runAllTimers();
    });
    expect(screen.getByText('16 lần')).toBeTruthy();
    expect(screen.getByText('Tuần 8 • Lần 2')).toBeTruthy();
    expect(screen.getByText('Mới nhất')).toBeTruthy();
    // Lần đầu không có lần trước để so.
    expect(screen.getByText('Tuần 1 • Lần 1')).toBeTruthy();
    expect(screen.getByText('Mốc đầu')).toBeTruthy();
    // Tuần 2 lần 1 (61,5... ) tăng so với lần 2 tuần 1: có ít nhất một mức đổi dương.
    expect(screen.getAllByText(/^\+/).length).toBeGreaterThan(3);
  });

  test('không tìm thấy lộ trình thì báo lỗi, không vỡ màn', async () => {
    await render(
      <AppProviders>
        <ChiTietCanNangScreen id="khong-co" />
      </AppProviders>,
    );
    expect(screen.getAllByText('Không tìm thấy lộ trình.').length).toBeGreaterThan(0);
  });
});

describe('AnhTheoMocScreen', () => {
  test('mặc định so sánh mốc đầu với mốc mới nhất kèm cân nặng và mức đổi', async () => {
    await render(
      <AppProviders>
        <AnhTheoMocScreen id="ct-1" />
      </AppProviders>,
    );
    await act(async () => {
      jest.runAllTimers();
    });
    expect(screen.getByText('Ảnh cơ thể theo mốc')).toBeTruthy();
    expect(screen.getByLabelText('Ảnh cơ thể tuần 1')).toBeTruthy();
    expect(screen.getByLabelText('Ảnh cơ thể tuần 5')).toBeTruthy();
    // Tuần 1 nặng 60,0, tuần 5 nặng 64,0.
    expect(screen.getByText('Cách nhau 4 tuần • +4,0 kg')).toBeTruthy();
    expect(screen.getByRole('radio', { name: 'Mốc trước Tuần 1', checked: true })).toBeTruthy();
    expect(screen.getByRole('radio', { name: 'Mốc sau Tuần 5', checked: true })).toBeTruthy();
  });

  test('đổi mốc trước thì so sánh và khoảng cách đổi theo', async () => {
    await render(
      <AppProviders>
        <AnhTheoMocScreen id="ct-1" />
      </AppProviders>,
    );
    await act(async () => {
      jest.runAllTimers();
    });
    await fireEvent.press(screen.getByRole('radio', { name: 'Mốc trước Tuần 3' }));
    expect(screen.getByLabelText('Ảnh cơ thể tuần 3')).toBeTruthy();
    expect(screen.queryByLabelText('Ảnh cơ thể tuần 1')).toBeNull();
    expect(screen.getByText('Cách nhau 2 tuần • +2,3 kg')).toBeTruthy();
    await fireEvent.press(screen.getByRole('radio', { name: 'Mốc sau Tuần 3' }));
    expect(screen.getByText('Cùng một mốc')).toBeTruthy();
  });

  test('danh sách mốc hiện chế độ xem của từng ảnh (N33)', async () => {
    await render(
      <AppProviders>
        <AnhTheoMocScreen id="ct-1" />
      </AppProviders>,
    );
    await act(async () => {
      jest.runAllTimers();
    });
    expect(screen.getByText('3 ảnh')).toBeTruthy();
    expect(screen.getByLabelText('Ảnh mốc tuần 5')).toBeTruthy();
    expect(screen.getAllByText('Chỉ mình tôi')).toHaveLength(2);
    expect(screen.getAllByText('Cho PT xem')).toHaveLength(1);
  });

  test('không tìm thấy lộ trình thì báo lỗi', async () => {
    await render(
      <AppProviders>
        <AnhTheoMocScreen id="khong-co" />
      </AppProviders>,
    );
    expect(screen.getAllByText('Không tìm thấy lộ trình.').length).toBeGreaterThan(0);
  });
});
