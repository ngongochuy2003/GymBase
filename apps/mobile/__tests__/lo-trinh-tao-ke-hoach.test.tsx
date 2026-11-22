import { fireEvent, render, screen } from '@testing-library/react-native';
import { datLaiBanNhap, layBanNhap } from '@/features/lo-trinh/data/ban-nhap-lo-trinh-store';
import { DatBuaAnScreen } from '@/features/lo-trinh/screens/dat-bua-an-screen';
import { DatBuoiTapScreen } from '@/features/lo-trinh/screens/dat-buoi-tap-screen';
import { TaoLoTrinhScreen } from '@/features/lo-trinh/screens/tao-lo-trinh-screen';
import type { LichTapTheoTuan } from '@/features/lo-trinh/types';
import {
  buoiTapCuaTuan,
  suaBuoiTapCuaTuan,
  xoaBuoiTapCuaTuan,
} from '@/features/lo-trinh/utils/lich-tap';
import { AppProviders } from '@/providers/app-providers';

const mockPush = jest.fn();
const mockBack = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ push: mockPush, back: mockBack, canGoBack: () => true }),
}));
jest.mock('expo-image-picker', () => ({
  requestMediaLibraryPermissionsAsync: jest.fn(),
  requestCameraPermissionsAsync: jest.fn(),
  launchImageLibraryAsync: jest.fn(),
  launchCameraAsync: jest.fn(),
}));

beforeEach(() => {
  datLaiBanNhap();
  mockPush.mockClear();
  mockBack.mockClear();
});

const LICH: LichTapTheoTuan = {
  chung: [{ thu: 1, ten: 'Upper A', gio: '17:30', baiTap: [] }],
  rieng: {},
};

describe('lịch tập theo tuần', () => {
  test('sửa Tuần 1 đổi bản chung; sửa tuần khác tách riêng tuần đó (N29)', () => {
    const tuan1 = suaBuoiTapCuaTuan(LICH, 1, 1, { ten: 'Ngực' });
    expect(tuan1.chung[0]?.ten).toBe('Ngực');
    expect(buoiTapCuaTuan(tuan1, 3)[0]?.ten).toBe('Ngực');

    const tuan3 = suaBuoiTapCuaTuan(LICH, 3, 1, { ten: 'Lưng' });
    expect(tuan3.chung[0]?.ten).toBe('Upper A');
    expect(buoiTapCuaTuan(tuan3, 3)[0]?.ten).toBe('Lưng');
    // Sửa Tuần 1 sau đó không ghi đè tuần đã sửa riêng.
    const sauTuan1 = suaBuoiTapCuaTuan(tuan3, 1, 1, { ten: 'Ngực' });
    expect(buoiTapCuaTuan(sauTuan1, 3)[0]?.ten).toBe('Lưng');
    expect(buoiTapCuaTuan(sauTuan1, 2)[0]?.ten).toBe('Ngực');
  });

  test('xóa buổi chỉ ảnh hưởng tuần đang sửa', () => {
    expect(xoaBuoiTapCuaTuan(LICH, 1, 1).chung).toHaveLength(0);
    const bo = xoaBuoiTapCuaTuan(LICH, 2, 1);
    expect(buoiTapCuaTuan(bo, 2)).toHaveLength(0);
    expect(buoiTapCuaTuan(bo, 1)).toHaveLength(1);
  });
});

describe('PT-02 Tạo lộ trình nối sang chi tiết bữa ăn và buổi tập', () => {
  test('mỗi bữa và buổi tập hiện tóm tắt, mũi tên mở màn con đúng bữa, đúng buổi', async () => {
    await render(
      <AppProviders>
        <TaoLoTrinhScreen />
      </AppProviders>,
    );
    expect(screen.getByText('~550 kcal • Yến mạch với sữa, 3 trứng luộc, 1 chuối')).toBeTruthy();
    expect(screen.getByText('3 bài • 60 phút • Ngực, vai, tay sau')).toBeTruthy();

    await fireEvent.press(screen.getByLabelText('Chi tiết Bữa trưa'));
    expect(mockPush).toHaveBeenLastCalledWith({
      pathname: '/dat-bua-an',
      params: { buaId: 'trua' },
    });

    await fireEvent.press(screen.getByLabelText('Chi tiết buổi Thứ 4'));
    expect(mockPush).toHaveBeenLastCalledWith({
      pathname: '/dat-buoi-tap',
      params: { tuan: '1', thu: '3' },
    });
  });

  test('bữa chưa có năng lượng và buổi chưa có bài tập thì nhắc chưa đặt', async () => {
    await render(
      <AppProviders>
        <TaoLoTrinhScreen />
      </AppProviders>,
    );
    // Bật Thứ 3 (chưa có buổi) để có một buổi trống.
    await fireEvent.press(screen.getByLabelText('Thứ 3'));
    expect(screen.getByText('Chưa đặt bài tập')).toBeTruthy();
  });
});

describe('DatBuaAnScreen (PT-02a)', () => {
  test('sửa mô tả, năng lượng, giờ ghi thẳng vào bản nháp của form', async () => {
    await render(
      <AppProviders>
        <DatBuaAnScreen buaId="sang" />
      </AppProviders>,
    );
    expect(screen.getByText('Chi tiết bữa ăn')).toBeTruthy();
    expect(screen.getByLabelText('Mô tả món ăn').props.value).toBe(
      'Yến mạch với sữa, 3 trứng luộc, 1 chuối',
    );
    expect(screen.getByLabelText('Năng lượng bữa ăn (kcal)').props.value).toBe('550');

    await fireEvent.changeText(screen.getByLabelText('Mô tả món ăn'), 'Cháo yến mạch');
    await fireEvent.changeText(screen.getByLabelText('Năng lượng bữa ăn (kcal)'), '48x0');
    const sang = layBanNhap().buaAn.find((b) => b.id === 'sang');
    expect(sang).toMatchObject({ moTa: 'Cháo yến mạch', kcal: 480 });
    // Các bữa khác không đổi.
    expect(layBanNhap().buaAn.find((b) => b.id === 'trua')?.kcal).toBe(700);

    await fireEvent.changeText(screen.getByLabelText('Năng lượng bữa ăn (kcal)'), '');
    expect(layBanNhap().buaAn.find((b) => b.id === 'sang')?.kcal).toBeUndefined();

    await fireEvent.press(screen.getByLabelText('Xong'));
    expect(mockBack).toHaveBeenCalled();
  });

  test('không tìm thấy bữa ăn thì báo lỗi', async () => {
    await render(
      <AppProviders>
        <DatBuaAnScreen buaId="khong-co" />
      </AppProviders>,
    );
    expect(screen.getByText('Không tìm thấy bữa ăn.')).toBeTruthy();
  });
});

describe('DatBuoiTapScreen (PT-02b)', () => {
  test('sửa thông tin buổi và bài tập của Tuần 1 ghi vào bản chung', async () => {
    await render(
      <AppProviders>
        <DatBuoiTapScreen tuan="1" thu="1" />
      </AppProviders>,
    );
    expect(screen.getByText('Tuần 1 • Thứ 2')).toBeTruthy();
    expect(screen.getByText('3 bài')).toBeTruthy();
    expect(screen.getByLabelText('Tên bài tập 1').props.value).toBe('Đẩy ngực với tạ đòn');

    await fireEvent.changeText(screen.getByLabelText('Nhóm cơ'), 'Ngực');
    await fireEvent.changeText(screen.getByLabelText('Tên bài tập 1'), 'Bench press');
    await fireEvent.changeText(screen.getByLabelText('Số set bài 1'), '5x');
    await fireEvent.changeText(screen.getByLabelText('Số lần bài 1'), '5 rep');

    const buoi = layBanNhap().lichTap.chung.find((b) => b.thu === 1);
    expect(buoi?.nhomCo).toBe('Ngực');
    expect(buoi?.baiTap?.[0]).toMatchObject({ ten: 'Bench press', soSet: 5, soLan: '5 rep' });
    // Tuần khác chưa sửa riêng vẫn theo Tuần 1.
    expect(layBanNhap().lichTap.rieng).toEqual({});
  });

  test('thêm, đổi thứ tự và xóa bài tập', async () => {
    await render(
      <AppProviders>
        <DatBuoiTapScreen tuan="1" thu="1" />
      </AppProviders>,
    );
    await fireEvent.press(screen.getByLabelText('Thêm bài tập'));
    expect(screen.getByText('4 bài')).toBeTruthy();
    expect(screen.getByLabelText('Tên bài tập 4').props.value).toBe('');

    // Đưa bài 2 lên đầu: bài đầu đổi thành "Đẩy vai với tạ đơn".
    await fireEvent.press(screen.getByLabelText('Đưa bài 2 lên'));
    expect(screen.getByLabelText('Tên bài tập 1').props.value).toBe('Đẩy vai với tạ đơn');
    expect(screen.getByLabelText('Đưa bài 1 lên').props.accessibilityState.disabled).toBe(true);

    await fireEvent.press(screen.getByLabelText('Xóa bài tập 1'));
    expect(screen.getByText('3 bài')).toBeTruthy();
    expect(layBanNhap().lichTap.chung[0]?.baiTap?.map((b) => b.ten)).toEqual([
      'Đẩy ngực với tạ đòn',
      'Đẩy tay sau với dây cáp',
      '',
    ]);
  });

  test('sửa ở Tuần 3 tách riêng tuần đó và báo rõ trên màn', async () => {
    await render(
      <AppProviders>
        <DatBuoiTapScreen tuan="3" thu="1" />
      </AppProviders>,
    );
    expect(
      screen.getByText('Tuần 3 đang theo Tuần 1. Sửa ở đây sẽ tách riêng tuần này.'),
    ).toBeTruthy();
    await fireEvent.changeText(screen.getByLabelText('Tên buổi tập'), 'Upper A nhẹ');
    expect(screen.getByText('Tuần 3 đã sửa riêng, không còn theo Tuần 1.')).toBeTruthy();
    expect(layBanNhap().lichTap.chung[0]?.ten).toBe('Upper A');
    expect(layBanNhap().lichTap.rieng[3]?.find((b) => b.thu === 1)?.ten).toBe('Upper A nhẹ');
  });

  test('xóa buổi tập bỏ buổi khỏi tuần và quay lại form', async () => {
    await render(
      <AppProviders>
        <DatBuoiTapScreen tuan="1" thu="3" />
      </AppProviders>,
    );
    await fireEvent.press(screen.getByLabelText('Xóa buổi tập'));
    expect(layBanNhap().lichTap.chung.map((b) => b.thu)).toEqual([1, 5]);
    expect(mockBack).toHaveBeenCalled();
  });

  test('không tìm thấy buổi tập thì báo lỗi', async () => {
    await render(
      <AppProviders>
        <DatBuoiTapScreen tuan="1" thu="7" />
      </AppProviders>,
    );
    expect(screen.getByText('Không tìm thấy buổi tập.')).toBeTruthy();
  });
});
