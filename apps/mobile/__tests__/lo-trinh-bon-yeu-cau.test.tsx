import { act, fireEvent, render, screen } from '@testing-library/react-native';
import { Linking } from 'react-native';
import { DanhSachBaiTap } from '@/features/lo-trinh/components/check-in-buoi-tap';
import {
  datLaiBanNhap,
  layBanNhap,
  themBua,
} from '@/features/lo-trinh/data/ban-nhap-lo-trinh-store';
import {
  datLaiDanhGiaCheckIn,
  datPhienBanCheckIn,
} from '@/features/lo-trinh/data/danh-gia-check-in-store';
import { taoLichSuMau } from '@/features/lo-trinh/data/lich-su-check-in';
import { taoDuLieuMau, taoTienDoMau } from '@/features/lo-trinh/data/seed';
import { DatBuaAnScreen } from '@/features/lo-trinh/screens/dat-bua-an-screen';
import { DuyetNhanhScreen } from '@/features/lo-trinh/screens/duyet-nhanh-screen';
import { LichSuCheckInScreen } from '@/features/lo-trinh/screens/lich-su-check-in-screen';
import { laLinkHopLe } from '@/features/lo-trinh/utils/link-huong-dan';
import { AppProviders } from '@/providers/app-providers';

// Animation chạy trên luồng UI của native; trong test dùng bản giả.
// eslint-disable-next-line @typescript-eslint/no-require-imports
jest.mock('react-native-reanimated', () => require('react-native-reanimated/mock'));

const mockPush = jest.fn();
const mockBack = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ push: mockPush, back: mockBack }),
}));
jest.mock('expo-image-picker', () => ({
  requestMediaLibraryPermissionsAsync: jest.fn(),
  requestCameraPermissionsAsync: jest.fn(),
  launchImageLibraryAsync: jest.fn(),
  launchCameraAsync: jest.fn(),
}));

const ve = (con: React.ReactElement) => render(<AppProviders>{con}</AppProviders>);

beforeEach(async () => {
  jest.clearAllMocks();
  await act(() => {
    datLaiBanNhap();
    datLaiDanhGiaCheckIn();
  });
});

describe('thêm, xóa bữa ăn (N40)', () => {
  test('thêm được tới 6 bữa rồi thôi', () => {
    expect(themBua()).toBeTruthy();
    expect(themBua()).toBeTruthy();
    expect(themBua()).toBeTruthy();
    expect(themBua()).toBeNull();
    expect(layBanNhap().buaAn).toHaveLength(6);
  });

  test('đổi tên và xóa bữa ở màn chi tiết', async () => {
    const id = themBua() ?? '';
    await ve(<DatBuaAnScreen buaId={id} />);
    await fireEvent.changeText(screen.getByLabelText('Tên bữa'), 'Bữa xế');
    expect(layBanNhap().buaAn.find((b) => b.id === id)?.ten).toBe('Bữa xế');
    await fireEvent.press(screen.getByLabelText('Xóa bữa'));
    expect(layBanNhap().buaAn.some((b) => b.id === id)).toBe(false);
    expect(mockBack).toHaveBeenCalled();
  });
});

describe('link hướng dẫn (N41)', () => {
  test('chỉ nhận https có tên miền', () => {
    expect(laLinkHopLe('https://youtu.be/abc')).toBe(true);
    expect(laLinkHopLe('http://youtu.be/abc')).toBe(false);
    expect(laLinkHopLe('https://abc')).toBe(false);
    expect(laLinkHopLe('javascript:alert(1)')).toBe(false);
    expect(laLinkHopLe(undefined)).toBe(false);
  });

  test('bấm Xem hướng dẫn mở link và không tick bài', async () => {
    const mo = jest.spyOn(Linking, 'openURL').mockImplementation(async () => undefined);
    const onDoi = jest.fn();
    await ve(
      <DanhSachBaiTap
        baiTap={[{ id: 'a', ten: 'Squat', soSet: 3, soLan: '8', link: 'https://example.com/x' }]}
        daXong={[]}
        onDoi={onDoi}
      />,
    );
    await fireEvent.press(screen.getByLabelText('Xem hướng dẫn Squat'));
    expect(mo).toHaveBeenCalledWith('https://example.com/x');
    expect(onDoi).not.toHaveBeenCalled();
  });
});

describe('lịch sử check-in (N42)', () => {
  test('số mục khớp số x/y ở khối chỉ số', () => {
    const homNay = new Date();
    const lo = taoDuLieuMau(homNay).cuaToi[0];
    if (!lo) throw new Error('thiếu lộ trình mẫu');
    const tienDo = taoTienDoMau(lo);
    const muc = taoLichSuMau(lo, tienDo, homNay);
    for (const loai of ['bua_an', 'buoi_tap'] as const) {
      const so = loai === 'bua_an' ? tienDo.buaAn : tienDo.buoiTap;
      const cua = muc.filter((m) => m.nhiemVu.loai === loai);
      expect(cua).toHaveLength(so.tong);
      expect(cua.filter((m) => m.nhiemVu.trangThai === 'da_check')).toHaveLength(so.xong);
    }
  });

  test('mở lọc sẵn theo loại; lọc Bỏ lỡ; bấm mục có check-in mở chi tiết', async () => {
    await ve(<LichSuCheckInScreen id="ct-1" loai="buoi_tap" />);
    expect(screen.getByText('Đã check 14/16')).toBeTruthy();
    await fireEvent.press(screen.getByLabelText('Lọc kết quả Bỏ lỡ'));
    expect(screen.getAllByText('Bỏ lỡ').length).toBeGreaterThan(1);
    await fireEvent.press(screen.getByLabelText('Lọc kết quả Đã check'));
    const dong = screen.getAllByRole('button', { name: /^Buổi tập – .* ngày / })[0];
    if (!dong) throw new Error('thiếu dòng lịch sử');
    await fireEvent.press(dong);
    expect(mockPush).toHaveBeenCalledWith(
      expect.objectContaining({ pathname: '/chi-tiet-check-in-buoi-tap' }),
    );
  });
});

describe('duyệt nhanh (N43)', () => {
  const soCho = () => Number(screen.getByText(/check-in chờ duyệt$/).props.children.split(' ')[0]);

  test('duyệt một bản, và cần bổ sung phải có nhận xét', async () => {
    await ve(<DuyetNhanhScreen loTrinhId="ql-1" />);
    expect(soCho()).toBe(2);
    await fireEvent.press(screen.getAllByRole('button', { name: /^Cần bổ sung / })[0]!);
    await fireEvent.press(screen.getByRole('button', { name: /^Gửi yêu cầu bổ sung / }));
    expect(screen.getByText('Nhập nội dung cần học viên bổ sung')).toBeTruthy();
    expect(soCho()).toBe(2);
    await fireEvent.press(screen.getAllByRole('button', { name: /^Duyệt / })[0]!);
    expect(soCho()).toBe(1);
  });

  test('chọn tất cả rồi duyệt phải xác nhận; bản học viên vừa sửa không ghi', async () => {
    await ve(<DuyetNhanhScreen loTrinhId="ql-1" />);
    await act(() => datPhienBanCheckIn('ci-ql-1-1', 2));
    await fireEvent.press(screen.getByLabelText('Chọn tất cả'));
    await fireEvent.press(screen.getByLabelText('Duyệt 2 check-in đã chọn'));
    expect(soCho()).toBe(2);
    await fireEvent.press(screen.getByLabelText('Xác nhận duyệt'));
    expect(soCho()).toBe(1);
    expect(screen.getByText(/1 check-in học viên vừa sửa/)).toBeTruthy();
  });
});
