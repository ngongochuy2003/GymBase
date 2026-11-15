import { act, fireEvent, render, screen } from '@testing-library/react-native';
import { Image } from 'react-native';
import {
  PostCard,
  PostImage,
  MIN_POST_ASPECT_RATIO,
  MAX_POST_ASPECT_RATIO,
  DEFAULT_POST_ASPECT_RATIO,
} from '@/features/feed';

describe('PostCard Image Aspect Ratio & Fit Behavior', () => {
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

  it('giữ đúng các mốc giới hạn tỷ lệ khung an toàn', () => {
    expect(MIN_POST_ASPECT_RATIO).toBeCloseTo(0.8); // 4 / 5
    expect(MAX_POST_ASPECT_RATIO).toBeCloseTo(1.777, 2); // 16 / 9
    expect(DEFAULT_POST_ASPECT_RATIO).toBeCloseTo(1.333, 2); // 4 / 3
  });

  it('render ảnh bài viết với resizeMode = contain để không phóng to cắt chi tiết', async () => {
    await render(
      <PostCard
        author={{ name: 'Minh Lê' }}
        timeLabel="Vừa xong"
        content="Hôm nay tập ngực"
        imageUri="https://cdn.gymbase.vn/bench-press.webp"
        actions={[]}
      />,
    );

    const container = screen.getByTestId('post-image-container');
    expect(container).toBeTruthy();

    const image = screen.getByLabelText('Ảnh bài viết');
    expect(image).toBeTruthy();
    expect(image.props.resizeMode).toBe('contain');
  });

  it('PostImage tự động điều chỉnh tỷ lệ khung hình khi tải xong ảnh ngang (16:9)', async () => {
    await render(<PostImage uri="https://cdn.gymbase.vn/landscape.webp" />);

    const container = screen.getByTestId('post-image-container');
    const image = screen.getByLabelText('Ảnh bài viết');

    // Mô phỏng sự kiện onLoad trả về ảnh ngang 1920 x 1080 (16:9)
    await act(async () => {
      fireEvent(image, 'onLoad', {
        nativeEvent: { source: { width: 1920, height: 1080 } },
      });
    });

    const flatStyle = Object.assign({}, ...[container.props.style].flat().filter(Boolean));
    expect(flatStyle.aspectRatio).toBeCloseTo(16 / 9);
  });

  it('PostImage thu nhỏ và giới hạn khung ở 4:5 khi ảnh siêu dài (infographic/full-body 9:20)', async () => {
    await render(<PostImage uri="https://cdn.gymbase.vn/long-infographic.webp" />);

    const container = screen.getByTestId('post-image-container');
    const image = screen.getByLabelText('Ảnh bài viết');

    // Mô phỏng ảnh dọc siêu dài 1080 x 2400 (ratio = 0.45 < MIN_POST_ASPECT_RATIO 0.8)
    await act(async () => {
      fireEvent(image, 'onLoad', {
        nativeEvent: { source: { width: 1080, height: 2400 } },
      });
    });

    const flatStyle = Object.assign({}, ...[container.props.style].flat().filter(Boolean));
    // Khung dừng ở mức tối đa an toàn 4:5, ảnh thu nhỏ vừa vặn ở giữa mà không bị phóng to cắt xén
    expect(flatStyle.aspectRatio).toBeCloseTo(MIN_POST_ASPECT_RATIO);
    expect(image.props.resizeMode).toBe('contain');
  });

  it('PostImage thu nhỏ và giới hạn khung ở 16:9 khi ảnh siêu ngang (panorama 21:9)', async () => {
    await render(<PostImage uri="https://cdn.gymbase.vn/panorama.webp" />);

    const container = screen.getByTestId('post-image-container');
    const image = screen.getByLabelText('Ảnh bài viết');

    // Mô phỏng ảnh panorama 2560 x 1080 (ratio = 2.37 > MAX_POST_ASPECT_RATIO 1.777)
    await act(async () => {
      fireEvent(image, 'onLoad', {
        nativeEvent: { source: { width: 2560, height: 1080 } },
      });
    });

    const flatStyle = Object.assign({}, ...[container.props.style].flat().filter(Boolean));
    // Khung dừng ở 16:9, ảnh thu nhỏ vừa vặn ở giữa
    expect(flatStyle.aspectRatio).toBeCloseTo(MAX_POST_ASPECT_RATIO);
    expect(image.props.resizeMode).toBe('contain');
  });
});
