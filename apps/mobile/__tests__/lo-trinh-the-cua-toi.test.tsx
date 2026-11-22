import { fireEvent, render, screen } from '@testing-library/react-native';
import { TheLoTrinhCuaToi } from '@/features/lo-trinh/components/the-lo-trinh-cua-toi';
import { taoDuLieuMau } from '@/features/lo-trinh/data/seed';
import { AppProviders } from '@/providers/app-providers';

const mockPush = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ push: mockPush, back: jest.fn() }),
}));

describe('TheLoTrinhCuaToi', () => {
  test('dưới tên lộ trình có tên PT phụ trách', async () => {
    const lo = taoDuLieuMau(new Date()).cuaToi[0];
    if (!lo) throw new Error('thiếu lộ trình mẫu');
    await render(
      <AppProviders>
        <TheLoTrinhCuaToi loTrinh={lo} />
      </AppProviders>,
    );
    expect(screen.getByText('Tăng 10 kg trong 3 tháng')).toBeTruthy();
    expect(screen.getByText('PT Trần Đức Anh')).toBeTruthy();
    await fireEvent.press(screen.getByRole('button', { name: 'Tăng 10 kg trong 3 tháng' }));
    expect(mockPush).toHaveBeenCalledWith({
      pathname: '/chi-tiet-lo-trinh',
      params: { id: lo.id },
    });
  });
});
