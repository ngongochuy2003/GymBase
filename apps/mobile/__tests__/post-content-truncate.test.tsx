import { fireEvent, render, screen } from '@testing-library/react-native';
import { PostCard } from '@/features/feed';

describe('PostCard Content Truncation & Navigation Triggers', () => {
  const shortText = 'Hôm nay tập ngực và tay sau cực kỳ năng suất!';
  const longText =
    'Hôm nay là một ngày tập luyện vô cùng tuyệt vời tại GymBase. Tôi đã hoàn thành 5 hiệp Bench Press với mức tạ 100kg và 4 hiệp Incline Dumbbell Press. Cảm thấy cơ thể tràn đầy năng lượng và sẵn sàng cho các bài tập tiếp theo!';

  it('bài viết <= 100 ký tự: hiển thị trọn vẹn và không xuất hiện "...đọc thêm"', async () => {
    await render(
      <PostCard
        author={{ name: 'Minh Lê' }}
        timeLabel="Vừa xong"
        content={shortText}
        actions={[]}
      />,
    );

    expect(screen.getByText(shortText)).toBeTruthy();
    expect(screen.queryByTestId('post-read-more')).toBeNull();
  });

  it('bài viết > 100 ký tự: cắt nội dung ở 100 ký tự và hiện "...đọc thêm" được làm mờ', async () => {
    await render(
      <PostCard
        author={{ name: 'Minh Lê' }}
        timeLabel="Vừa xong"
        content={longText}
        actions={[]}
        truncateContent={true}
        maxContentLength={100}
      />,
    );

    const expectedTruncated = longText.slice(0, 100).trimEnd();
    // 100 ký tự đầu tiên được render
    expect(screen.getByText(new RegExp(expectedTruncated.slice(0, 30)))).toBeTruthy();

    // Xuất hiện text "...đọc thêm"
    const readMore = screen.getByTestId('post-read-more');
    expect(readMore).toBeTruthy();
    expect(readMore.props.children).toBe('...đọc thêm');

    // Thuộc tính làm mờ đúng yêu cầu (opacity < 1 và màu muted)
    const flatStyle = Object.assign({}, ...[readMore.props.style].flat().filter(Boolean));
    expect(flatStyle.opacity).toBeLessThan(1);
  });

  it('khi người dùng bấm vào phần nội dung hoặc "...đọc thêm", kích hoạt sự kiện onPressContent', async () => {
    const onPressContentMock = jest.fn();

    await render(
      <PostCard
        author={{ name: 'Minh Lê' }}
        timeLabel="Vừa xong"
        content={longText}
        actions={[]}
        onPressContent={onPressContentMock}
      />,
    );

    const pressable = screen.getByTestId('post-content-pressable');
    expect(pressable).toBeTruthy();

    await fireEvent.press(pressable);
    expect(onPressContentMock).toHaveBeenCalledTimes(1);
  });

  it('khi người dùng bấm vào ảnh trong lưới ảnh, kích hoạt onPressImage với index tương ứng', async () => {
    const onPressImageMock = jest.fn();

    await render(
      <PostCard
        author={{ name: 'Minh Lê' }}
        timeLabel="Vừa xong"
        content="Bài viết có 2 ảnh"
        imageUris={['https://cdn.gymbase.vn/img1.webp', 'https://cdn.gymbase.vn/img2.webp']}
        actions={[]}
        onPressImage={onPressImageMock}
      />,
    );

    const image0 = screen.getByTestId('post-image-pressable-0');
    const image1 = screen.getByTestId('post-image-pressable-1');

    await fireEvent.press(image0);
    expect(onPressImageMock).toHaveBeenCalledWith(0);

    await fireEvent.press(image1);
    expect(onPressImageMock).toHaveBeenCalledWith(1);
  });

  it('khi truncateContent = false (màn hình chi tiết), nội dung hiển thị đầy đủ không bị cắt', async () => {
    await render(
      <PostCard
        author={{ name: 'Minh Lê' }}
        timeLabel="Vừa xong"
        content={longText}
        actions={[]}
        truncateContent={false}
      />,
    );

    expect(screen.getByText(longText)).toBeTruthy();
    expect(screen.queryByTestId('post-read-more')).toBeNull();
  });
});
