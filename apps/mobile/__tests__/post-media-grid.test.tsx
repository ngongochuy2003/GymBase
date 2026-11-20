import { render, screen } from '@testing-library/react-native';
import { Image } from 'react-native';
import { PostCard, PostMediaGrid } from '@/features/feed';

describe('PostMediaGrid & Multi-Image Layouts', () => {
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

  it('render 1 ảnh: dùng PostImage đơn lẻ', async () => {
    await render(<PostMediaGrid images={['https://cdn.gymbase.vn/img1.webp']} />);

    expect(screen.getByTestId('post-image-container')).toBeTruthy();
    expect(screen.queryByTestId('post-media-grid-2')).toBeNull();
    expect(screen.queryByTestId('post-media-grid-3')).toBeNull();
    expect(screen.queryByTestId('post-media-grid-4')).toBeNull();
  });

  it('render 2 ảnh: chia dọc khung làm 2', async () => {
    await render(
      <PostMediaGrid
        images={['https://cdn.gymbase.vn/img1.webp', 'https://cdn.gymbase.vn/img2.webp']}
      />,
    );

    const grid2 = screen.getByTestId('post-media-grid-2');
    expect(grid2).toBeTruthy();
    expect(screen.queryByTestId('post-media-grid-3')).toBeNull();
    expect(screen.queryByTestId('post-media-grid-4')).toBeNull();
  });

  it('render 3 ảnh: ảnh 1 to bên trái, 2 ảnh sau xếp dọc bên phải', async () => {
    await render(
      <PostMediaGrid
        images={[
          'https://cdn.gymbase.vn/img1.webp',
          'https://cdn.gymbase.vn/img2.webp',
          'https://cdn.gymbase.vn/img3.webp',
        ]}
      />,
    );

    const grid3 = screen.getByTestId('post-media-grid-3');
    expect(grid3).toBeTruthy();
    expect(screen.queryByTestId('post-media-grid-2')).toBeNull();
    expect(screen.queryByTestId('post-media-grid-4')).toBeNull();
  });

  it('render 4 ảnh: lưới 2x2 không có overlay cộng thêm', async () => {
    await render(
      <PostMediaGrid
        images={[
          'https://cdn.gymbase.vn/img1.webp',
          'https://cdn.gymbase.vn/img2.webp',
          'https://cdn.gymbase.vn/img3.webp',
          'https://cdn.gymbase.vn/img4.webp',
        ]}
      />,
    );

    const grid4 = screen.getByTestId('post-media-grid-4');
    expect(grid4).toBeTruthy();
    expect(screen.queryByTestId('post-media-remaining-overlay')).toBeNull();
  });

  it('render 6 ảnh: lưới 2x2, ô thứ 4 hiển thị overlay +2', async () => {
    await render(
      <PostMediaGrid
        images={[
          'https://cdn.gymbase.vn/img1.webp',
          'https://cdn.gymbase.vn/img2.webp',
          'https://cdn.gymbase.vn/img3.webp',
          'https://cdn.gymbase.vn/img4.webp',
          'https://cdn.gymbase.vn/img5.webp',
          'https://cdn.gymbase.vn/img6.webp',
        ]}
      />,
    );

    const grid4 = screen.getByTestId('post-media-grid-4');
    expect(grid4).toBeTruthy();
    const overlay = screen.getByTestId('post-media-remaining-overlay');
    expect(overlay).toBeTruthy();
    expect(screen.getByText('+2')).toBeTruthy();
  });

  it('render 7 ảnh: lưới 2x2, ô thứ 4 hiển thị overlay +3', async () => {
    await render(
      <PostMediaGrid
        images={[
          'https://cdn.gymbase.vn/img1.webp',
          'https://cdn.gymbase.vn/img2.webp',
          'https://cdn.gymbase.vn/img3.webp',
          'https://cdn.gymbase.vn/img4.webp',
          'https://cdn.gymbase.vn/img5.webp',
          'https://cdn.gymbase.vn/img6.webp',
          'https://cdn.gymbase.vn/img7.webp',
        ]}
      />,
    );

    expect(screen.getByTestId('post-media-remaining-overlay')).toBeTruthy();
    expect(screen.getByText('+3')).toBeTruthy();
  });

  it('PostCard hiển thị đúng lưới đa ảnh khi truyền mảng imageUris', async () => {
    await render(
      <PostCard
        author={{ name: 'Minh Lê' }}
        timeLabel="Vừa xong"
        content="Album ảnh tập luyện"
        imageUris={[
          'https://cdn.gymbase.vn/img1.webp',
          'https://cdn.gymbase.vn/img2.webp',
          'https://cdn.gymbase.vn/img3.webp',
          'https://cdn.gymbase.vn/img4.webp',
          'https://cdn.gymbase.vn/img5.webp',
          'https://cdn.gymbase.vn/img6.webp',
          'https://cdn.gymbase.vn/img7.webp',
        ]}
        actions={[]}
        imageLayout="grid"
      />,
    );

    expect(screen.getByTestId('post-media-grid-4')).toBeTruthy();
    expect(screen.getByText('+3')).toBeTruthy();
  });

  it('PostCard hiển thị Carousel vuốt ngang theo thiết kế mới khi mặc định', async () => {
    const handlePressImage = jest.fn();
    await render(
      <PostCard
        author={{ name: 'Minh Lê' }}
        timeLabel="Vừa xong"
        content="Album 3 ảnh cuộn ngang"
        imageUris={[
          'https://cdn.gymbase.vn/img1.webp',
          'https://cdn.gymbase.vn/img2.webp',
          'https://cdn.gymbase.vn/img3.webp',
        ]}
        actions={[]}
        onPressImage={handlePressImage}
      />,
    );

    const carousel = screen.getByTestId('post-media-carousel');
    expect(carousel).toBeTruthy();
    expect(screen.getByTestId('post-image-pressable-0')).toBeTruthy();
    expect(screen.getByTestId('post-image-pressable-1')).toBeTruthy();
    expect(screen.getByTestId('post-image-pressable-2')).toBeTruthy();
  });
});
