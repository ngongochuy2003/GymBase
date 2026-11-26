import { act, render, screen } from '@testing-library/react-native';
import { taoDuLieuMau, taoTienDoMau } from '@/features/lo-trinh/data/seed';
import { ChiTietLoTrinhScreen } from '@/features/lo-trinh/screens/chi-tiet-lo-trinh-screen';
import { khoangTrucY, phanTramMucTieu, soThapPhan } from '@/features/lo-trinh/utils/tien-do';
import { AppProviders } from '@/providers/app-providers';

// Animation chạy trên luồng UI của native; trong test dùng bản giả để giá trị đổi ngay (xem reanimated-mock.js).
// eslint-disable-next-line @typescript-eslint/no-require-imports
jest.mock('react-native-reanimated', () => require('react-native-reanimated/mock'));
jest.mock('expo-router', () => ({
  useRouter: () => ({ push: jest.fn(), back: jest.fn(), canGoBack: () => true }),
}));

describe('tính toán tiến độ lộ trình', () => {
  test('soThapPhan dùng dấu phẩy', () => {
    expect(soThapPhan(6.5)).toBe('6,5');
    expect(soThapPhan(60)).toBe('60,0');
  });

  test('phanTramMucTieu đúng cho tăng cân, giảm cân và kẹp trong 0 đến 100', () => {
    expect(phanTramMucTieu(66.5, 60, 70)).toBe(65);
    expect(phanTramMucTieu(77.5, 80, 75)).toBe(50);
    expect(phanTramMucTieu(58, 60, 70)).toBe(0);
    expect(phanTramMucTieu(72, 60, 70)).toBe(100);
    expect(phanTramMucTieu(60, 60, 60)).toBe(100);
  });

  test('khoangTrucY co giãn theo dữ liệu và bao hết điểm đo', () => {
    expect(khoangTrucY([60, 66.5])).toMatchObject({ min: 58, max: 68, buoc: 2 });
    // Dữ liệu hẹp thì bước nhỏ lại, không bị kéo giãn tới một mốc cố định.
    expect(khoangTrucY([70.1, 70.9])).toMatchObject({ min: 70, max: 71, buoc: 0.2 });
    // Dải lớn thì bước lớn hơn.
    expect(khoangTrucY([50, 90])).toMatchObject({ min: 40, max: 100, buoc: 10 });
    // Mọi điểm bằng nhau vẫn có khoảng để vẽ.
    const bang = khoangTrucY([65, 65]);
    expect(bang.min).toBeLessThan(65);
    expect(bang.max).toBeGreaterThan(65);
  });
});

describe('ChiTietLoTrinhScreen', () => {
  // Đồng hồ giả để chạy hết animation đếm số một cách xác định, không phải chờ thật.
  beforeEach(() => jest.useFakeTimers());
  afterEach(() => jest.useRealTimers());

  test('hiện đủ các khối tiến độ của lộ trình', async () => {
    await render(
      <AppProviders>
        <ChiTietLoTrinhScreen id="ct-1" />
      </AppProviders>,
    );
    expect(screen.getByText('Tăng 10 kg trong 3 tháng')).toBeTruthy();
    expect(screen.getByText('Từ 60 kg → 70 kg')).toBeTruthy();
    // Các số đếm lên từ 0, nên cho animation chạy hết mới thấy giá trị thật.
    await act(async () => {
      await jest.advanceTimersByTimeAsync(3000);
    });
    expect(screen.getByText('65%')).toBeTruthy();
    expect(screen.getByText(/Đã tăng 6,5 kg/)).toBeTruthy();
    expect(screen.getByText('92%')).toBeTruthy();
    expect(screen.getByText('Cân nặng 8 tuần')).toBeTruthy();
    expect(screen.getByText('Ảnh cơ thể theo mốc')).toBeTruthy();
    expect(screen.getByText('Đánh giá từ PT')).toBeTruthy();
    expect(screen.getByLabelText('Quay lại')).toBeTruthy();
  });

  test('dữ liệu mẫu cân nặng có 2 lần cân mỗi tuần và tăng dần tới cân nặng hiện tại', () => {
    const lo = taoDuLieuMau(new Date()).cuaToi[0];
    if (!lo) throw new Error('thiếu lộ trình mẫu');
    const { canNang } = taoTienDoMau(lo);
    expect(canNang).toHaveLength(16);
    expect(canNang.map((d) => d.lan)).toEqual(Array.from({ length: 16 }, (_, i) => (i % 2) + 1));
    expect(canNang[0]).toMatchObject({ tuan: 1, lan: 1, kg: 60 });
    expect(canNang.at(-1)).toMatchObject({ tuan: 8, lan: 2, kg: 66.5 });
  });

  test('id không tồn tại thì báo không tìm thấy', async () => {
    await render(
      <AppProviders>
        <ChiTietLoTrinhScreen id="khong-co" />
      </AppProviders>,
    );
    expect(screen.getByText('Không tìm thấy lộ trình.')).toBeTruthy();
  });
});
