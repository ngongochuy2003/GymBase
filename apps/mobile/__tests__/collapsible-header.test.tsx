import { act, fireEvent, render, renderHook, screen } from '@testing-library/react-native';
import {
  ScreenRenderer,
  useCollapsibleHeader,
  type ScreenConfig,
} from '@/shared/screen-config';
import { pickTabs } from '@/shared/navigation';

jest.mock('expo-router', () => ({
  useRouter: () => ({ navigate: jest.fn(), back: jest.fn(), canGoBack: () => false }),
}));

describe('useCollapsibleHeader Hook', () => {
  it('khởi tạo với trạng thái header hiển thị ban đầu', async () => {
    const { result } = await renderHook(() =>
      useCollapsibleHeader({ initialHeaderHeight: 60 }),
    );

    expect(result.current.isHeaderVisible).toBe(true);
    expect(result.current.headerHeight).toBe(60);
  });

  it('ẩn header khi người dùng vuốt cuộn xuống (scroll down)', async () => {
    const { result } = await renderHook(() =>
      useCollapsibleHeader({ initialHeaderHeight: 60, scrollThreshold: 10 }),
    );

    // Bắt đầu từ 0
    await act(async () => {
      result.current.onScroll({
        nativeEvent: { contentOffset: { y: 0 } },
      } as never);
    });

    // Cuộn xuống 50px (diff = 50 > 10, currentY = 50 > 30) -> phải ẩn
    await act(async () => {
      result.current.onScroll({
        nativeEvent: { contentOffset: { y: 50 } },
      } as never);
    });

    expect(result.current.isHeaderVisible).toBe(false);
  });

  it('từ từ hiện header ra khi người dùng vuốt cuộn lên (scroll up)', async () => {
    const { result } = await renderHook(() =>
      useCollapsibleHeader({ initialHeaderHeight: 60, scrollThreshold: 10 }),
    );

    // Cuộn xuống để ẩn header trước
    await act(async () => {
      result.current.onScroll({
        nativeEvent: { contentOffset: { y: 80 } },
      } as never);
    });
    expect(result.current.isHeaderVisible).toBe(false);

    // Cuộn ngược lên từ 80 về 40 (diff = -40 < -10) -> phải hiện lại
    await act(async () => {
      result.current.onScroll({
        nativeEvent: { contentOffset: { y: 40 } },
      } as never);
    });

    expect(result.current.isHeaderVisible).toBe(true);
  });

  it('luôn hiện header khi chạm đỉnh trang hoặc kéo nảy (contentOffset.y <= 0)', async () => {
    const { result } = await renderHook(() =>
      useCollapsibleHeader({ initialHeaderHeight: 60, scrollThreshold: 10 }),
    );

    // Đang ẩn ở vị trí 100
    await act(async () => {
      result.current.onScroll({
        nativeEvent: { contentOffset: { y: 100 } },
      } as never);
    });
    expect(result.current.isHeaderVisible).toBe(false);

    // Người dùng lướt về đỉnh trang (y = 0 hoặc y = -10 iOS bounce)
    await act(async () => {
      result.current.onScroll({
        nativeEvent: { contentOffset: { y: -5 } },
      } as never);
    });

    expect(result.current.isHeaderVisible).toBe(true);
  });

  it('cập nhật headerHeight chính xác khi gọi onHeaderLayout', async () => {
    const { result } = await renderHook(() =>
      useCollapsibleHeader({ initialHeaderHeight: 60 }),
    );

    await act(async () => {
      result.current.onHeaderLayout({
        nativeEvent: { layout: { height: 95 } },
      } as never);
    });

    expect(result.current.headerHeight).toBe(95);
  });

  it('không thay đổi trạng thái khi enabled = false', async () => {
    const { result } = await renderHook(() =>
      useCollapsibleHeader({ enabled: false, initialHeaderHeight: 60 }),
    );

    await act(async () => {
      result.current.onScroll({
        nativeEvent: { contentOffset: { y: 100 } },
      } as never);
    });

    expect(result.current.isHeaderVisible).toBe(true);
  });
});

describe('ScreenRenderer Collapsible Header UI Integration', () => {
  const dummyConfig: ScreenConfig = {
    id: 'test-feed',
    header: {
      rightActions: [],
      onAvatarPress: jest.fn(),
    },
    sections: [
      { id: 'sec-1', type: 'dummySection' },
    ],
    bottomTabs: pickTabs(['home', 'profile']),
    activeTabId: 'home',
  };

  it('render header có testID screen-header-container và gắn animation collapsible', async () => {
    await render(
      <ScreenRenderer
        config={dummyConfig}
      />,
    );

    const header = screen.getByTestId('screen-header-container');
    const scrollView = screen.getByTestId('screen-scroll-view');

    expect(header).toBeTruthy();
    expect(scrollView).toBeTruthy();

    // Mô phỏng người dùng vuốt xuống: scroll view kích hoạt onScroll
    await act(async () => {
      fireEvent.scroll(scrollView, {
        nativeEvent: {
          contentOffset: { y: 100 },
          contentSize: { height: 1000, width: 400 },
          layoutMeasurement: { height: 600, width: 400 },
        },
      });
    });

    // Pointer events tự động chuyển none khi ẩn để không chặn thao tác chạm
    expect(header.props.pointerEvents).toBe('none');

    // Mô phỏng người dùng vuốt lên: cuộn ngược về 30
    await act(async () => {
      fireEvent.scroll(scrollView, {
        nativeEvent: {
          contentOffset: { y: 30 },
          contentSize: { height: 1000, width: 400 },
          layoutMeasurement: { height: 600, width: 400 },
        },
      });
    });

    expect(header.props.pointerEvents).toBe('auto');
  });

  it('tôn trọng collapsibleHeader: false khi người dùng muốn cố định header', async () => {
    await render(
      <ScreenRenderer
        config={{
          ...dummyConfig,
          collapsibleHeader: false,
        }}
      />,
    );

    const header = screen.getByTestId('screen-header-container');
    const scrollView = screen.getByTestId('screen-scroll-view');

    await act(async () => {
      fireEvent.scroll(scrollView, {
        nativeEvent: {
          contentOffset: { y: 150 },
        },
      });
    });

    // Không chuyển pointerEvents sang none vì collapsible bị tắt
    expect(header.props.pointerEvents).toBe('auto');
  });
});
