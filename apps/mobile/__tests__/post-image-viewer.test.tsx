import { fireEvent, render, screen } from '@testing-library/react-native';
import { PostImageViewer } from '@/features/feed';

describe('PostImageViewer Fullscreen Lightbox & Gestures', () => {
  const sampleImages = [
    'https://cdn.gymbase.vn/img1.webp',
    'https://cdn.gymbase.vn/img2.webp',
    'https://cdn.gymbase.vn/img3.webp',
  ];

  it('không render khi visible = false', async () => {
    await render(
      <PostImageViewer visible={false} images={sampleImages} onClose={jest.fn()} />,
    );

    expect(screen.queryByTestId('post-image-viewer-container')).toBeNull();
  });

  it('hiển thị đúng số trang và ảnh ban đầu khi visible = true', async () => {
    await render(
      <PostImageViewer visible={true} images={sampleImages} initialIndex={0} onClose={jest.fn()} />,
    );

    expect(screen.getByTestId('post-image-viewer-container')).toBeTruthy();
    expect(screen.getByTestId('post-image-viewer-counter').props.children).toBe('1 / 3');
  });

  it('nhấn nút phóng to / thu nhỏ thay đổi tỷ lệ zoom', async () => {
    await render(
      <PostImageViewer visible={true} images={sampleImages} initialIndex={0} onClose={jest.fn()} />,
    );

    const zoomInBtn = screen.getByTestId('post-image-viewer-zoom-in');
    const transformContainer = screen.getByTestId('post-image-viewer-transform');

    // Mặc định scale = 1
    const initialStyle = Object.assign(
      {},
      ...[transformContainer.props.style].flat().filter(Boolean),
    );
    expect(initialStyle.transform).toEqual([{ scale: 1 }]);

    // Bấm Zoom In
    await fireEvent.press(zoomInBtn);
    const zoomedStyle = Object.assign(
      {},
      ...[transformContainer.props.style].flat().filter(Boolean),
    );
    expect(zoomedStyle.transform).toEqual([{ scale: 1.5 }]);

    // Bấm Zoom Out
    const zoomOutBtn = screen.getByTestId('post-image-viewer-zoom-out');
    await fireEvent.press(zoomOutBtn);
    const resetStyle = Object.assign(
      {},
      ...[transformContainer.props.style].flat().filter(Boolean),
    );
    expect(resetStyle.transform).toEqual([{ scale: 1 }]);
  });

  it('vuốt/chuyển sang phải (Next): chuyển sang ảnh kế tiếp; nếu hết ảnh -> thoát về bài viết', async () => {
    const onCloseMock = jest.fn();
    await render(
      <PostImageViewer visible={true} images={sampleImages} initialIndex={1} onClose={onCloseMock} />,
    );

    const counter = screen.getByTestId('post-image-viewer-counter');
    expect(counter.props.children).toBe('2 / 3');

    // Chuyển sang ảnh 3
    const nextBtn = screen.getByTestId('post-image-viewer-next');
    await fireEvent.press(nextBtn);
    expect(counter.props.children).toBe('3 / 3');

    // Đang ở ảnh cuối cùng (3 / 3), tiếp tục vuốt sang ảnh kế tiếp -> thoát xem ảnh
    await fireEvent.press(nextBtn);
    expect(onCloseMock).toHaveBeenCalledTimes(1);
  });

  it('ở ảnh đầu tiên (1 / 3), vuốt lùi về trước (Prev) -> thoát về xem bài viết', async () => {
    const onCloseMock = jest.fn();
    await render(
      <PostImageViewer visible={true} images={sampleImages} initialIndex={0} onClose={onCloseMock} />,
    );

    const prevBtn = screen.getByTestId('post-image-viewer-prev');
    // Ở ảnh đầu tiên bấm prev -> thoát về bài viết
    await fireEvent.press(prevBtn);
    expect(onCloseMock).toHaveBeenCalledTimes(1);
  });

  it('nhấn nút đóng (X): gọi onClose ngay lập tức', async () => {
    const onCloseMock = jest.fn();
    await render(
      <PostImageViewer visible={true} images={sampleImages} initialIndex={0} onClose={onCloseMock} />,
    );

    const closeBtn = screen.getByTestId('post-image-viewer-close');
    await fireEvent.press(closeBtn);
    expect(onCloseMock).toHaveBeenCalledTimes(1);
  });
});
