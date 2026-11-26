import { act, fireEvent, render, screen } from '@testing-library/react-native';
import { TheLoTrinhQuanLy } from '@/features/lo-trinh/components/the-lo-trinh-quan-ly';
import { datLaiDongLoTrinh } from '@/features/lo-trinh/data/dong-lo-trinh-store';
import { taoDuLieuMau } from '@/features/lo-trinh/data/seed';
import { ChiTietLoTrinhHocVienScreen } from '@/features/lo-trinh/screens/chi-tiet-lo-trinh-hoc-vien-screen';
import type { NhiemVuNhom, ThanhVienLoTrinh } from '@/features/lo-trinh/types';
import { moTaTienDoNhiemVu, trangThaiNhiemVuNhom } from '@/features/lo-trinh/utils/nhiem-vu-nhom';
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
  datLaiDongLoTrinh();
  mockPush.mockClear();
});
afterEach(() => jest.useRealTimers());

const TV = (id: string, thamGia: ThanhVienLoTrinh['thamGia'] = 'tham_gia'): ThanhVienLoTrinh => ({
  id,
  ten: id,
  thamGia,
});
const NV = (daCheckIn: string[]): NhiemVuNhom => ({
  id: 'n',
  loai: 'bua_an',
  tieuDe: 'Bữa trưa',
  moTa: 'Chờ học viên check-in',
  gio: '12:00',
  moc: 'cho_check_in',
  daCheckIn,
});

describe('trạng thái nhiệm vụ chung của nhóm', () => {
  test('xong khi mọi thành viên đang tham gia đã check-in, chưa thì theo mốc giờ', () => {
    const nhom = [TV('a'), TV('b')];
    expect(trangThaiNhiemVuNhom(NV(['a', 'b']), nhom)).toBe('da_hoan_thanh');
    expect(trangThaiNhiemVuNhom(NV(['a']), nhom)).toBe('cho_check_in');
    expect(trangThaiNhiemVuNhom(NV([]), nhom)).toBe('cho_check_in');
  });

  test('người chưa chấp nhận lời mời hoặc đã rời không chặn việc hoàn thành', () => {
    const nhom = [TV('a'), TV('b', 'cho_chap_nhan'), TV('c', 'da_roi'), TV('d', 'tu_choi')];
    expect(trangThaiNhiemVuNhom(NV(['a']), nhom)).toBe('da_hoan_thanh');
  });

  test('không có ai đang tham gia thì không tính là xong', () => {
    expect(trangThaiNhiemVuNhom(NV([]), [TV('a', 'cho_chap_nhan')])).toBe('cho_check_in');
  });

  test('dòng phụ: mô tả khi chưa ai check-in, n/m với nhóm, xong thì báo cả nhóm', () => {
    const nhom = [TV('a'), TV('b'), TV('c')];
    expect(moTaTienDoNhiemVu(NV([]), nhom, true)).toBe('Chờ học viên check-in');
    expect(moTaTienDoNhiemVu(NV(['a']), nhom, true)).toBe('1/3 đã check-in');
    expect(moTaTienDoNhiemVu(NV(['a', 'b', 'c']), nhom, true)).toBe('Cả nhóm đã check-in');
    expect(moTaTienDoNhiemVu(NV(['a']), [TV('a')], false)).toBe('Đã check-in');
  });
});

async function moManHinh(id: string) {
  await render(
    <AppProviders>
      <ChiTietLoTrinhHocVienScreen id={id} />
    </AppProviders>,
  );
  await act(async () => {
    jest.runAllTimers();
  });
}

describe('PT-03 lộ trình đơn', () => {
  test('hiện học viên, PT, tuần, công việc hôm nay với nhãn trạng thái, check-in chờ duyệt', async () => {
    await moManHinh('ql-1');
    expect(screen.getByText('Chi tiết lộ trình học viên')).toBeTruthy();
    expect(screen.getByText('Tăng 10 kg trong 3 tháng')).toBeTruthy();
    expect(screen.getByText('Nguyễn Văn Minh')).toBeTruthy();
    expect(screen.getByText('PT phụ trách')).toBeTruthy();
    expect(screen.getByText(/^Tuần \d+ \/ 12$/)).toBeTruthy();
    // Lộ trình đơn không có danh sách thành viên để mở.
    expect(screen.queryByLabelText(/^Thành viên/)).toBeNull();

    expect(screen.getByText('Công việc hôm nay')).toBeTruthy();
    expect(screen.getByText('Đã hoàn thành')).toBeTruthy();
    expect(screen.getByText('Chờ check-in')).toBeTruthy();
    expect(screen.getByText('Sắp tới 17:30')).toBeTruthy();
    expect(screen.getByText('Ngày mai')).toBeTruthy();

    expect(screen.getByText('Check-in chờ duyệt (2)')).toBeTruthy();
    expect(screen.getByLabelText('Xem Bữa trưa của Nguyễn Văn Minh')).toBeTruthy();
    expect(screen.getByText('Hoạt động gần đây')).toBeTruthy();
    expect(screen.getByText('Check-in gần đây')).toBeTruthy();
  });

  test('bốn tab chuyển nội dung; "Xem tiến độ" nhảy tới tab Tiến độ', async () => {
    await moManHinh('ql-1');
    await fireEvent.press(screen.getByRole('tab', { name: 'Kế hoạch' }));
    expect(screen.getByText('Kế hoạch dinh dưỡng')).toBeTruthy();
    expect(screen.getByText('3 bữa mỗi ngày')).toBeTruthy();
    expect(screen.getByText('Thứ 2 • Upper A')).toBeTruthy();
    expect(screen.getByText('• Đẩy ngực với tạ đòn — 4 set × 8–10 rep')).toBeTruthy();
    expect(screen.queryByText('Công việc hôm nay')).toBeNull();

    await fireEvent.press(screen.getByRole('tab', { name: 'Check-in' }));
    expect(screen.getByText('4 bản')).toBeTruthy();
    await fireEvent.press(screen.getByRole('radio', { name: 'Lọc Cần bổ sung' }));
    expect(screen.getByText('1 bản')).toBeTruthy();
    expect(screen.getByText('Cần bổ sung')).toBeTruthy();

    await fireEvent.press(screen.getByLabelText('Xem tiến độ'));
    await act(async () => {
      jest.runAllTimers();
    });
    expect(screen.getByRole('tab', { name: 'Tiến độ', selected: true })).toBeTruthy();
    expect(screen.getByText('Ảnh cơ thể theo mốc')).toBeTruthy();
  });
});

describe('PT-03 lộ trình nhóm', () => {
  test('trạng thái là của cả nhóm, thành viên đã check-in hiện bằng ảnh đại diện nhỏ', async () => {
    await moManHinh('ql-3');
    expect(screen.getByText('Nhóm 5 người')).toBeTruthy();
    expect(screen.getByText('4 đang tham gia')).toBeTruthy();

    // Bữa sáng: cả 4 người đang tham gia đã check-in nên cả nhóm hoàn thành.
    expect(screen.getByText('Cả nhóm đã check-in')).toBeTruthy();
    expect(screen.getByText('Đã hoàn thành')).toBeTruthy();
    expect(screen.getByLabelText('4 người đã check-in')).toBeTruthy();
    // Bữa trưa: mới 2/4 người nên cả nhóm vẫn "Chờ check-in".
    expect(screen.getByText('2/4 đã check-in')).toBeTruthy();
    expect(screen.getByText('Chờ check-in')).toBeTruthy();
    expect(screen.getByLabelText('2 người đã check-in')).toBeTruthy();
  });

  test('check-in chờ duyệt ghi tên học viên; mở danh sách thành viên thấy trạng thái tham gia', async () => {
    await moManHinh('ql-3');
    expect(screen.getByText('Check-in chờ duyệt (5)')).toBeTruthy();
    // 5 check-in chia cho 4 người nên Lê Hoàng Nam có hai bản.
    expect(screen.getAllByLabelText('Xem Bữa trưa của Lê Hoàng Nam')).toHaveLength(2);
    expect(screen.getByLabelText('Xem Buổi tập Upper A của Phạm Thu Hà')).toBeTruthy();

    expect(screen.queryByText('Chờ chấp nhận')).toBeNull();
    await fireEvent.press(screen.getByLabelText('Thành viên, nhóm 5 người'));
    expect(screen.getByText('Ngô Tuấn Kiệt')).toBeTruthy();
    expect(screen.getByText('Chờ chấp nhận')).toBeTruthy();
    expect(screen.getAllByText('Đã tham gia')).toHaveLength(4);
  });

  test('tab Tiến độ chọn từng thành viên và chỉ hiện ảnh học viên cho PT xem (N33)', async () => {
    await moManHinh('ql-3');
    await fireEvent.press(screen.getByRole('tab', { name: 'Tiến độ' }));
    await act(async () => {
      jest.runAllTimers();
    });
    expect(
      screen.getByRole('radio', { name: 'Tiến độ của Lê Hoàng Nam', checked: true }),
    ).toBeTruthy();
    // Người chưa chấp nhận lời mời không có tiến độ để chọn.
    expect(screen.queryByRole('radio', { name: 'Tiến độ của Ngô Tuấn Kiệt' })).toBeNull();
    await fireEvent.press(screen.getByRole('radio', { name: 'Tiến độ của Phạm Thu Hà' }));
    expect(
      screen.getByRole('radio', { name: 'Tiến độ của Phạm Thu Hà', checked: true }),
    ).toBeTruthy();
    expect(screen.getByText(/ảnh để chế độ Chỉ mình tôi nên không hiện ở đây/)).toBeTruthy();
  });
});

describe('đóng lộ trình (N28)', () => {
  test('bắt buộc nhập lý do; đóng xong thì lộ trình chỉ xem', async () => {
    await moManHinh('ql-1');
    expect(screen.getByLabelText('Chỉnh sửa kế hoạch').props.accessibilityState.disabled).toBe(
      false,
    );

    await fireEvent.press(screen.getByLabelText('Tùy chọn lộ trình'));
    await fireEvent.press(screen.getByLabelText('Đóng lộ trình'));
    expect(screen.getByText('Đóng lộ trình?')).toBeTruthy();

    // Thiếu lý do thì không đóng.
    await fireEvent.press(screen.getByLabelText('Xác nhận đóng lộ trình'));
    expect(screen.getByText('Nhập lý do đóng lộ trình')).toBeTruthy();
    expect(screen.queryByText('Lộ trình đã đóng: thất bại')).toBeNull();

    await fireEvent.press(screen.getByRole('radio', { name: 'Thất bại' }));
    await fireEvent.changeText(screen.getByLabelText('Lý do đóng lộ trình'), 'Học viên nghỉ tập');
    await fireEvent.press(screen.getByLabelText('Xác nhận đóng lộ trình'));

    expect(screen.getByText('Lộ trình đã đóng: thất bại')).toBeTruthy();
    expect(screen.getByText(/Lý do: Học viên nghỉ tập/)).toBeTruthy();
    expect(screen.getByText('Thất bại')).toBeTruthy();
    expect(screen.getByText('Lộ trình không còn nhiệm vụ hôm nay.')).toBeTruthy();
    expect(screen.getByLabelText('Chỉnh sửa kế hoạch').props.accessibilityState.disabled).toBe(
      true,
    );
    expect(screen.queryByLabelText('Tùy chọn lộ trình')).toBeNull();
  });

  test('lộ trình đã đóng sẵn: báo chỉ xem, không có menu đóng', async () => {
    await moManHinh('ql-5');
    expect(screen.getByText('Lộ trình đã đóng: thất bại')).toBeTruthy();
    expect(screen.getByText(/Lý do: Học viên nghỉ tập do bận công việc/)).toBeTruthy();
    expect(screen.queryByLabelText('Tùy chọn lộ trình')).toBeNull();

    await act(async () => {
      jest.runAllTimers();
    });
  });

  test('lộ trình hoàn thành cũng chỉ xem', async () => {
    await moManHinh('ql-4');
    expect(screen.getByText('Lộ trình đã hoàn thành')).toBeTruthy();
    expect(screen.getByLabelText('Chỉnh sửa kế hoạch').props.accessibilityState.disabled).toBe(
      true,
    );
  });

  test('không tìm thấy lộ trình thì báo lỗi', async () => {
    await moManHinh('khong-co');
    expect(screen.getByText('Không tìm thấy lộ trình.')).toBeTruthy();
  });
});

describe('PT-01 nối sang PT-03', () => {
  test('"Xem chi tiết" trên thẻ mở chi tiết đúng lộ trình', async () => {
    const lo = taoDuLieuMau(new Date()).quanLy[2];
    if (!lo) throw new Error('thiếu lộ trình mẫu');
    await render(
      <AppProviders>
        <TheLoTrinhQuanLy loTrinh={lo} homNay={new Date()} />
      </AppProviders>,
    );
    await fireEvent.press(screen.getByLabelText(`Xem chi tiết ${lo.ten}`));
    expect(mockPush).toHaveBeenCalledWith({
      pathname: '/chi-tiet-hoc-vien',
      params: { id: lo.id },
    });
  });
});
