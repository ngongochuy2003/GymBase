import { act, fireEvent, render, screen } from '@testing-library/react-native';
import { TheLoiMoi } from '@/features/lo-trinh/components/the-loi-moi';
import { datLaiLoiMoi } from '@/features/lo-trinh/data/loi-moi-store';
import { taoLoiMoiMau } from '@/features/lo-trinh/data/seed';
import { ChiTietLoTrinhScreen } from '@/features/lo-trinh/screens/chi-tiet-lo-trinh-screen';
import { LoiMoiLoTrinhScreen } from '@/features/lo-trinh/screens/loi-moi-lo-trinh-screen';
import { AppProviders } from '@/providers/app-providers';

// Màn chi tiết lộ trình dùng animation native; trong test dùng bản giả.
// eslint-disable-next-line @typescript-eslint/no-require-imports
jest.mock('react-native-reanimated', () => require('react-native-reanimated/mock'));

const mockPush = jest.fn();
const mockBack = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ push: mockPush, back: mockBack }),
}));

const TEN = 'Săn chắc toàn thân 8 tuần';

const ve = (con: React.ReactElement) => render(<AppProviders>{con}</AppProviders>);

beforeEach(async () => {
  jest.clearAllMocks();
  await act(() => datLaiLoiMoi());
});

describe('Lời mời tham gia lộ trình', () => {
  test('thẻ lời mời mở màn chi tiết lời mời', async () => {
    const loiMoi = taoLoiMoiMau(new Date())[0];
    if (!loiMoi) throw new Error('thiếu lời mời mẫu');
    await ve(<TheLoiMoi loiMoi={loiMoi} />);
    await fireEvent.press(screen.getByRole('button', { name: `Xem chi tiết lời mời ${TEN}` }));
    expect(mockPush).toHaveBeenCalledWith({
      pathname: '/loi-moi-lo-trinh',
      params: { id: 'lm-1' },
    });
  });

  test('xem được lịch tập của từng tuần', async () => {
    await ve(<LoiMoiLoTrinhScreen id="lm-1" />);
    expect(screen.getAllByRole('tab')).toHaveLength(8);
    expect(screen.getByText('Thứ 6 • Upper B')).toBeTruthy();
    await fireEvent.press(screen.getByRole('tab', { name: 'Tuần 4' }));
    expect(screen.queryByText('Thứ 6 • Upper B')).toBeNull();
    expect(screen.getByText('Thứ 2 • Upper A')).toBeTruthy();
  });

  test('bấm Tham gia phải xác nhận; hủy thì chưa trả lời', async () => {
    await ve(<LoiMoiLoTrinhScreen id="lm-1" />);
    await fireEvent.press(screen.getByRole('button', { name: 'Tham gia' }));
    expect(screen.getByText('Tham gia lộ trình?')).toBeTruthy();
    await fireEvent.press(screen.getByRole('button', { name: 'Hủy' }));
    expect(screen.queryByText('Tham gia lộ trình?')).toBeNull();
    expect(screen.getByRole('button', { name: 'Tham gia' })).toBeTruthy();
    expect(mockBack).not.toHaveBeenCalled();
  });

  test('xác nhận tham gia: quay lại, lộ trình có trong của tôi và chưa có số liệu', async () => {
    const man = await ve(<LoiMoiLoTrinhScreen id="lm-1" />);
    await fireEvent.press(screen.getByRole('button', { name: 'Tham gia' }));
    await fireEvent.press(screen.getByRole('button', { name: 'Xác nhận tham gia' }));
    expect(mockBack).toHaveBeenCalled();
    expect(screen.getByText('Bạn đã tham gia lộ trình này.')).toBeTruthy();
    await man.unmount();
    await ve(<ChiTietLoTrinhScreen id="lm-1" />);
    expect(screen.getByText('Chưa có số liệu tiến độ.')).toBeTruthy();
  });

  test('xác nhận từ chối: lộ trình không vào của tôi', async () => {
    const man = await ve(<LoiMoiLoTrinhScreen id="lm-1" />);
    await fireEvent.press(screen.getByRole('button', { name: 'Từ chối' }));
    await fireEvent.press(screen.getByRole('button', { name: 'Xác nhận từ chối' }));
    expect(mockBack).toHaveBeenCalled();
    expect(screen.getByText('Bạn đã từ chối lời mời này.')).toBeTruthy();
    await man.unmount();
    await ve(<ChiTietLoTrinhScreen id="lm-1" />);
    expect(screen.getByText('Không tìm thấy lộ trình.')).toBeTruthy();
  });
});
