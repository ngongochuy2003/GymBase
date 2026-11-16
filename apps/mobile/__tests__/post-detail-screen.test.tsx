import type { ReactNode } from 'react';
import { Image } from 'react-native';
import { fireEvent, render, screen } from '@testing-library/react-native';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { PostDetailScreen, type FeedPostData } from '@/features/feed';

describe('PostDetailScreen Behavior & Layout', () => {
  beforeEach(() => {
    jest.spyOn(Image, 'getSize').mockImplementation((_uri, success) => {
      if (typeof success === 'function') {
        success(400, 300);
      }
      return Promise.resolve() as unknown as void;
    });
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: 0 },
    },
  });

  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );

  const samplePost: FeedPostData = {
    id: 'post-detail-123',
    author: {
      name: 'Minh Lê',
      avatarUri: 'https://cdn.gymbase.vn/avatar.webp',
    },
    timeLabel: '10 phút trước',
    content:
      'Bài viết chi tiết với nội dung dài hơn 100 ký tự để kiểm tra việc không bị cắt xén khi hiển thị trên màn hình xem chi tiết bài viết của ứng dụng GymBase.',
    imageUris: [
      'https://cdn.gymbase.vn/img1.webp',
      'https://cdn.gymbase.vn/img2.webp',
      'https://cdn.gymbase.vn/img3.webp',
    ],
    counts: { like: 10, comment: 2 },
    initialActive: { like: false },
  };

  it('hiển thị đầy đủ nội dung bài viết và danh sách ảnh xếp dọc từ trên xuống', async () => {
    await render(<PostDetailScreen initialPost={samplePost} />, { wrapper });

    // Hiển thị tên tác giả và thời gian
    expect(screen.getByText('Minh Lê')).toBeTruthy();
    expect(screen.getByText('10 phút trước')).toBeTruthy();

    // Nội dung hiển thị trọn vẹn, không có "...đọc thêm"
    expect(screen.getByText(samplePost.content)).toBeTruthy();
    expect(screen.queryByTestId('post-read-more')).toBeNull();

    // Bố cục ảnh xếp dọc lần lượt từ trên xuống
    expect(screen.getByTestId('post-media-stack')).toBeTruthy();
    expect(screen.getByTestId('post-stack-image-0')).toBeTruthy();
    expect(screen.getByTestId('post-stack-image-1')).toBeTruthy();
    expect(screen.getByTestId('post-stack-image-2')).toBeTruthy();
  });

  it('khi ấn vào một bức ảnh trong danh sách dọc, mở trình xem chi tiết ảnh (PostImageViewer)', async () => {
    await render(<PostDetailScreen initialPost={samplePost} />, { wrapper });

    // Ban đầu trình xem ảnh chưa mở
    expect(screen.queryByTestId('post-image-viewer-container')).toBeNull();

    // Nhấn vào bức ảnh thứ 2
    const image1 = screen.getByTestId('post-stack-image-1');
    await fireEvent.press(image1);

    // Mở viewer và hiển thị trang 2 / 3
    expect(screen.getByTestId('post-image-viewer-container')).toBeTruthy();
    expect(screen.getByTestId('post-image-viewer-counter').props.children).toBe('2 / 3');
  });

  it('nhấn nút quay lại trên SubHeader kích hoạt onBack', async () => {
    const onBackMock = jest.fn();
    await render(<PostDetailScreen initialPost={samplePost} onBack={onBackMock} />, {
      wrapper,
    });

    const backButton = screen.getByLabelText('Quay lại');
    await fireEvent.press(backButton);

    expect(onBackMock).toHaveBeenCalledTimes(1);
  });

  it('chữ "Bài viết" và icon quay lại được thu nhỏ 2px, không chứa padding viền ngoài dư thừa', async () => {
    await render(<PostDetailScreen initialPost={samplePost} />, { wrapper });

    const titleElement = screen.getByRole('header');
    expect(titleElement.props.children).toBe('Bài viết');
    const flatTitleStyle = Object.assign(
      {},
      ...[titleElement.props.style].flat().filter(Boolean),
    );
    // fontSize giảm 2px từ 22px (xl) xuống 20px (xl - 2)
    expect(flatTitleStyle.fontSize).toBe(20);

    // Không tồn tại container với class padding 24px/40px (p-6 pb-10)
    const container = screen.getByTestId('post-detail-screen-container');
    expect(container).toBeTruthy();
    const flatContainerStyle = Object.assign(
      {},
      ...[container.props.style].flat().filter(Boolean),
    );
    expect(flatContainerStyle.paddingHorizontal).toBeUndefined();
    expect(flatContainerStyle.padding).toBeUndefined();
  });
});
