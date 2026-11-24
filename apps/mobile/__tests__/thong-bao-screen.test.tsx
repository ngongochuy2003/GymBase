import { fireEvent, render, screen, waitFor } from '@testing-library/react-native';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import { ThongBaoScreen } from '@/features/thong-bao';
import { thongBaoApi } from '@/shared/notifications';
import { ThemeProvider } from '@/shared/theme/theme';
import type { HopThuDto, ThongBaoDto } from '@/shared/notifications';

const mockPush = jest.fn();
const mockBack = jest.fn();
jest.mock('expo-router', () => ({
  useRouter: () => ({ push: mockPush, back: mockBack, navigate: jest.fn(), canGoBack: () => true }),
}));

jest.mock('@/shared/notifications/thong-bao-api', () => ({
  thongBaoApi: {
    layHopThu: jest.fn(),
    demChuaDoc: jest.fn(),
    danhDauDaDoc: jest.fn(),
    danhDauTatCaDaDoc: jest.fn(),
    guiThu: jest.fn(),
  },
}));

const api = jest.mocked(thongBaoApi);

function thongBao(id: string, daDoc: boolean, tieuDe = `Tiêu đề ${id}`): ThongBaoDto {
  return {
    id,
    loai_thong_bao: 'tin_nhan',
    tieu_de: tieuDe,
    noi_dung: `Nội dung ${id}`,
    loai_doi_tuong: 'tin_nhan',
    doi_tuong_id: null,
    thoi_diem: '2026-11-24T05:00:00Z',
    da_doc: daDoc,
  };
}

function hopThu(data: ThongBaoDto[], conNua = false): HopThuDto {
  return { data, con_nua: conNua };
}

async function renderScreen() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });
  const wrapper = ({ children }: { children: ReactNode }) => (
    <ThemeProvider>
      <QueryClientProvider client={client}>{children}</QueryClientProvider>
    </ThemeProvider>
  );
  return await render(<ThongBaoScreen />, { wrapper });
}

beforeEach(() => {
  api.danhDauDaDoc.mockResolvedValue(undefined);
  api.danhDauTatCaDaDoc.mockResolvedValue(undefined);
  api.guiThu.mockResolvedValue(undefined);
  api.demChuaDoc.mockResolvedValue(0);
});

describe('ThongBaoScreen', () => {
  it('liệt kê thông báo và đánh dấu dòng chưa đọc', async () => {
    api.layHopThu.mockResolvedValue(hopThu([thongBao('a', false), thongBao('b', true)]));

    await renderScreen();

    expect(await screen.findByText('Tiêu đề a')).toBeTruthy();
    expect(screen.getByText('Tiêu đề b')).toBeTruthy();
    expect(screen.getAllByTestId('cham-chua-doc')).toHaveLength(1);
  });

  it('hộp thư trống thì báo chưa có thông báo', async () => {
    api.layHopThu.mockResolvedValue(hopThu([]));

    await renderScreen();

    expect(await screen.findByText('Chưa có thông báo nào.')).toBeTruthy();
  });

  it('lỗi tải thì hiện lỗi và cho thử lại', async () => {
    api.layHopThu.mockRejectedValueOnce(new Error('mạng')).mockResolvedValueOnce(hopThu([thongBao('a', true)]));

    await renderScreen();

    await fireEvent.press(await screen.findByText('Thử lại'));
    expect(await screen.findByText('Tiêu đề a')).toBeTruthy();
  });

  it('bấm dòng chưa đọc: đánh dấu đã đọc và mở màn lộ trình', async () => {
    api.layHopThu.mockResolvedValue(hopThu([thongBao('a', false)]));
    await renderScreen();

    await fireEvent.press(await screen.findByText('Tiêu đề a'));

    await waitFor(() => expect(api.danhDauDaDoc).toHaveBeenCalledWith('a'));
    expect(mockPush).toHaveBeenCalledWith('/route');
  });

  it('bấm dòng đã đọc: chỉ điều hướng, không gọi đánh dấu', async () => {
    api.layHopThu.mockResolvedValue(hopThu([thongBao('b', true)]));
    await renderScreen();

    await fireEvent.press(await screen.findByText('Tiêu đề b'));

    expect(mockPush).toHaveBeenCalledWith('/route');
    expect(api.danhDauDaDoc).not.toHaveBeenCalled();
  });

  it('"Đọc tất cả" gọi server và bị khóa khi không còn dòng chưa đọc', async () => {
    api.layHopThu.mockResolvedValue(hopThu([thongBao('a', false)]));
    await renderScreen();
    await screen.findByText('Tiêu đề a');

    await fireEvent.press(screen.getByText('Đọc tất cả'));

    await waitFor(() => expect(api.danhDauTatCaDaDoc).toHaveBeenCalledTimes(1));
  });

  it('không có dòng chưa đọc thì "Đọc tất cả" không bấm được', async () => {
    api.layHopThu.mockResolvedValue(hopThu([thongBao('b', true)]));
    await renderScreen();
    await screen.findByText('Tiêu đề b');

    await fireEvent.press(screen.getByText('Đọc tất cả'));

    expect(api.danhDauTatCaDaDoc).not.toHaveBeenCalled();
  });

  it('nút quay lại về màn trước', async () => {
    api.layHopThu.mockResolvedValue(hopThu([]));
    await renderScreen();
    await screen.findByText('Chưa có thông báo nào.');

    await fireEvent.press(screen.getByLabelText('Quay lại'));

    expect(mockBack).toHaveBeenCalled();
  });

  it('nút gửi thử sau 15 giây hẹn giờ phía server để kịp tắt app', async () => {
    api.layHopThu.mockResolvedValue(hopThu([]));
    await renderScreen();
    await screen.findByText('Chưa có thông báo nào.');

    await fireEvent.press(screen.getByLabelText('Gửi thông báo thử sau 15 giây'));

    await waitFor(() => expect(api.guiThu).toHaveBeenCalledWith(15));
  });

  it('nút gửi thử (chỉ bản dev) gọi API thử rồi tải lại hộp thư', async () => {
    api.layHopThu.mockResolvedValue(hopThu([]));
    await renderScreen();
    await screen.findByText('Chưa có thông báo nào.');

    await fireEvent.press(screen.getByLabelText('Gửi thông báo thử'));

    await waitFor(() => expect(api.guiThu).toHaveBeenCalledTimes(1));
    await waitFor(() => expect(api.layHopThu).toHaveBeenCalledTimes(2));
  });
});
