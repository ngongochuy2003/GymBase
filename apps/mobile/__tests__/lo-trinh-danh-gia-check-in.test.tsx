import { act, fireEvent, render, screen } from '@testing-library/react-native';
import {
  datLaiDanhGiaCheckIn,
  datPhienBanCheckIn,
  guiDanhGia,
} from '@/features/lo-trinh/data/danh-gia-check-in-store';
import { datLaiDongLoTrinh, dongLoTrinh } from '@/features/lo-trinh/data/dong-lo-trinh-store';
import { DanhGiaCheckInScreen } from '@/features/lo-trinh/screens/danh-gia-check-in-screen';
import { ChiTietLoTrinhHocVienScreen } from '@/features/lo-trinh/screens/chi-tiet-lo-trinh-hoc-vien-screen';
import { AppProviders } from '@/providers/app-providers';

// Animation chạy trên luồng UI của native; trong test dùng bản giả để giá trị đổi ngay (xem reanimated-mock.js).
// eslint-disable-next-line @typescript-eslint/no-require-imports
jest.mock('react-native-reanimated', () => require('react-native-reanimated/mock'));
const mockPush = jest.fn();
const mockBack = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ push: mockPush, back: mockBack, canGoBack: () => true }),
}));

beforeEach(() => {
  jest.useFakeTimers();
  datLaiDanhGiaCheckIn();
  datLaiDongLoTrinh();
  mockPush.mockClear();
  mockBack.mockClear();
});
afterEach(() => jest.useRealTimers());

const BUA_CHO_DUYET = 'ci-ql-1-1';
const BUOI_CHO_DUYET = 'ci-ql-1-2';

async function moDanhGia(loTrinhId: string, checkInId: string) {
  await render(
    <AppProviders>
      <DanhGiaCheckInScreen loTrinhId={loTrinhId} checkInId={checkInId} />
    </AppProviders>,
  );
}

describe('kho đánh giá check-in', () => {
  test('ghi đánh giá một lần; gửi lại bị chặn', () => {
    const checkIn = { id: BUA_CHO_DUYET, phienBan: 1 };
    const danhGia = { loTrinhId: 'ql-1', trangThai: 'da_duyet', nhanXet: '' } as const;
    expect(guiDanhGia(checkIn, danhGia, 1)).toBe('ok');
    expect(guiDanhGia(checkIn, danhGia, 1)).toBe('da_xu_ly');
  });

  test('học viên đã sửa sau khi PT mở xem thì không ghi đánh giá (N13)', () => {
    const checkIn = { id: BUOI_CHO_DUYET, phienBan: 1 };
    const danhGia = { loTrinhId: 'ql-1', trangThai: 'can_bo_sung', nhanXet: 'Thêm ảnh' } as const;
    datPhienBanCheckIn(BUOI_CHO_DUYET, 2);
    expect(guiDanhGia(checkIn, danhGia, 1)).toBe('da_doi');
    expect(guiDanhGia(checkIn, danhGia, 2)).toBe('ok');
  });
});

describe('PT-04 check-in bữa ăn chờ duyệt', () => {
  test('hiện kế hoạch lúc check-in, kết quả, ảnh, ghi chú và ba nút', async () => {
    await moDanhGia('ql-1', BUA_CHO_DUYET);
    expect(screen.getByText('Đánh giá check-in')).toBeTruthy();
    expect(screen.getByText('Nguyễn Văn Minh • Tăng 10 kg trong 3 tháng')).toBeTruthy();
    expect(screen.getByText('Đã ăn')).toBeTruthy();
    expect(screen.getAllByLabelText('Ảnh bữa ăn học viên gửi')).toHaveLength(2);
    expect(screen.getByText('Em đã ăn đúng kế hoạch, có thêm 1 quả trứng.')).toBeTruthy();
    expect(screen.getByLabelText('Nhận xét của PT')).toBeTruthy();
    expect(screen.getByLabelText('Duyệt')).toBeTruthy();
    expect(screen.getByLabelText('Yêu cầu bổ sung')).toBeTruthy();
    expect(screen.getByLabelText('Nhắn tin học viên')).toBeTruthy();
  });

  test('Duyệt không cần nhận xét: quay về và check-in chuyển sang chỉ xem', async () => {
    await moDanhGia('ql-1', BUA_CHO_DUYET);
    await fireEvent.press(screen.getByLabelText('Duyệt'));
    expect(mockBack).toHaveBeenCalled();
    expect(screen.getByText('Chi tiết check-in')).toBeTruthy();
    expect(
      screen.getByText('Check-in này đã được duyệt. Học viên không sửa được nữa.'),
    ).toBeTruthy();
    expect(screen.queryByLabelText('Duyệt')).toBeNull();
  });

  test('Yêu cầu bổ sung bắt buộc có nhận xét', async () => {
    await moDanhGia('ql-1', BUA_CHO_DUYET);
    await fireEvent.press(screen.getByLabelText('Yêu cầu bổ sung'));
    expect(screen.getByText('Nhập nội dung cần học viên bổ sung')).toBeTruthy();
    expect(mockBack).not.toHaveBeenCalled();

    await fireEvent.changeText(screen.getByLabelText('Nhận xét của PT'), 'Chụp rõ món ăn hơn nhé');
    expect(screen.queryByText('Nhập nội dung cần học viên bổ sung')).toBeNull();
    await fireEvent.press(screen.getByLabelText('Yêu cầu bổ sung'));
    expect(mockBack).toHaveBeenCalled();
    expect(screen.getByText('Yêu cầu bổ sung đã gửi')).toBeTruthy();
    expect(screen.getByText('Chụp rõ món ăn hơn nhé')).toBeTruthy();
    expect(screen.getByText(/Đang chờ học viên bổ sung/)).toBeTruthy();
  });

  test('học viên vừa sửa: báo xem lại rồi mới cho đánh giá', async () => {
    await moDanhGia('ql-1', BUA_CHO_DUYET);
    await act(async () => {
      datPhienBanCheckIn(BUA_CHO_DUYET, 2);
    });
    await fireEvent.press(screen.getByLabelText('Duyệt'));
    expect(
      screen.getByText('Học viên vừa sửa check-in này. Hãy xem lại nội dung mới rồi đánh giá.'),
    ).toBeTruthy();
    expect(mockBack).not.toHaveBeenCalled();

    // Sau khi đã xem lại, bấm Duyệt lần nữa thì ghi được.
    await fireEvent.press(screen.getByLabelText('Duyệt'));
    expect(mockBack).toHaveBeenCalled();
  });
});

describe('PT-04 các trạng thái khác', () => {
  test('check-in buổi tập: kết quả, bài đã tick, ảnh', async () => {
    await moDanhGia('ql-1', BUOI_CHO_DUYET);
    expect(screen.getByText('Tập một phần')).toBeTruthy();
    expect(screen.getByText(/Đã hoàn thành 2\/3 bài/)).toBeTruthy();
    expect(screen.getByLabelText('Ảnh buổi tập học viên gửi')).toBeTruthy();
    expect(screen.getByText('Em bị đau vai nên bỏ bài cuối ạ.')).toBeTruthy();
    expect(screen.getByLabelText('Duyệt')).toBeTruthy();
  });

  test('đã duyệt: xem lại nhận xét, không còn nút đánh giá', async () => {
    await moDanhGia('ql-1', 'ci-ql-1-da-1');
    expect(screen.getByText('Chi tiết check-in')).toBeTruthy();
    expect(screen.getByText('Nhận xét của PT')).toBeTruthy();
    expect(
      screen.getByText('Khẩu phần ổn, đủ protein. Bữa sau giữ lượng rau như hôm nay nhé.'),
    ).toBeTruthy();
    expect(screen.queryByLabelText('Duyệt')).toBeNull();
    expect(screen.queryByLabelText('Yêu cầu bổ sung')).toBeNull();
  });

  test('cần bổ sung: báo đang chờ học viên', async () => {
    await moDanhGia('ql-1', 'ci-ql-1-da-2');
    expect(screen.getByText('Yêu cầu bổ sung đã gửi')).toBeTruthy();
    expect(screen.getByText(/Đang chờ học viên bổ sung/)).toBeTruthy();
    expect(screen.getByText('Học viên không gửi ảnh nào.')).toBeTruthy();
    expect(screen.queryByLabelText('Duyệt')).toBeNull();
  });

  test('lộ trình đã đóng thì chỉ xem dù check-in còn chờ duyệt', async () => {
    dongLoTrinh('ql-1', { trangThai: 'that_bai', lyDo: 'Học viên nghỉ tập' });
    await moDanhGia('ql-1', BUA_CHO_DUYET);
    expect(screen.getByText('Lộ trình đã đóng nên chỉ xem được.')).toBeTruthy();
    expect(screen.queryByLabelText('Duyệt')).toBeNull();
  });

  test('lộ trình nhóm ghi tên học viên của check-in', async () => {
    await moDanhGia('ql-3', 'ci-ql-3-2');
    expect(screen.getByText(/^Phạm Thu Hà • /)).toBeTruthy();
  });

  test('không tìm thấy check-in thì báo lỗi', async () => {
    await moDanhGia('ql-1', 'khong-co');
    expect(screen.getByText('Không tìm thấy check-in.')).toBeTruthy();
  });
});

describe('PT-03 nối với PT-04', () => {
  test('bấm "Xem" và dòng check-in mở đúng check-in', async () => {
    await render(
      <AppProviders>
        <ChiTietLoTrinhHocVienScreen id="ql-1" />
      </AppProviders>,
    );
    await act(async () => {
      jest.runAllTimers();
    });
    await fireEvent.press(screen.getByLabelText('Xem Bữa trưa của Nguyễn Văn Minh'));
    expect(mockPush).toHaveBeenLastCalledWith({
      pathname: '/danh-gia-check-in',
      params: { loTrinhId: 'ql-1', checkInId: BUA_CHO_DUYET },
    });

    await fireEvent.press(screen.getByRole('tab', { name: 'Check-in' }));
    await fireEvent.press(screen.getByLabelText('Bữa sáng của Nguyễn Văn Minh'));
    expect(mockPush).toHaveBeenLastCalledWith({
      pathname: '/danh-gia-check-in',
      params: { loTrinhId: 'ql-1', checkInId: 'ci-ql-1-da-1' },
    });
  });

  test('đã đánh giá một check-in thì banner hiện, số chờ duyệt giảm, hoạt động có dòng mới', async () => {
    guiDanhGia(
      { id: BUA_CHO_DUYET, phienBan: 1 },
      { loTrinhId: 'ql-1', trangThai: 'da_duyet', nhanXet: 'Ổn' },
      1,
    );
    await render(
      <AppProviders>
        <ChiTietLoTrinhHocVienScreen id="ql-1" />
      </AppProviders>,
    );
    await act(async () => {
      jest.runAllTimers();
    });
    expect(screen.getByText('Đã gửi đánh giá cho học viên')).toBeTruthy();
    expect(screen.getByText('Check-in chờ duyệt (1)')).toBeTruthy();
    expect(screen.getByText('PT vừa duyệt bữa trưa của Nguyễn Văn Minh')).toBeTruthy();

    await fireEvent.press(screen.getByLabelText('Đóng thông báo'));
    expect(screen.queryByText('Đã gửi đánh giá cho học viên')).toBeNull();
  });
});
