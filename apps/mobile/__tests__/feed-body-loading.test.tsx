import { fireEvent, render, screen } from '@testing-library/react-native';
import { PostList, type FeedPostData } from '@/features/feed';

describe('PostList Body-only Loading and States', () => {
  const sampleFilterChips = {
    items: [
      { id: 'for-you', label: 'Dành cho bạn' },
      { id: 'workout', label: 'Tập luyện' },
    ],
    activeId: 'workout',
    onChange: jest.fn(),
  };

  it('khi chuyển tab lần đầu (isLoading = true, posts rỗng), chỉ body hiện icon loading mà FilterChips vẫn hiển thị', async () => {
    await render(
      <PostList
        posts={[]}
        actionTemplates={[]}
        filterChipsProps={sampleFilterChips}
        isLoading={true}
      />,
    );

    // FilterChips vẫn tồn tại và không bị che mất
    expect(screen.getByText('Dành cho bạn')).toBeTruthy();
    expect(screen.getByText('Tập luyện')).toBeTruthy();

    // Body hiển thị loading spinner và thông báo đang tải
    expect(screen.getByTestId('feed-body-loading')).toBeTruthy();
    expect(screen.getByText('Đang tải bài viết...')).toBeTruthy();
  });

  it('khi đang cập nhật tab đã có bài (isLoading = true, posts > 0), hiển thị overlay loading và làm mờ bài viết', async () => {
    const samplePosts: FeedPostData[] = [
      {
        id: 'p1',
        author: { name: 'HLV Nam' },
        timeLabel: 'Vừa xong',
        content: 'Bài viết 1',
      },
    ];

    await render(
      <PostList
        posts={samplePosts}
        actionTemplates={[]}
        filterChipsProps={sampleFilterChips}
        isLoading={true}
      />,
    );

    // Vẫn hiển thị bài viết
    expect(screen.getByText('Bài viết 1')).toBeTruthy();

    // Hiển thị overlay loading cập nhật ở giữa body
    expect(screen.getByTestId('feed-body-loading-overlay')).toBeTruthy();
    expect(screen.getByText('Đang cập nhật...')).toBeTruthy();
  });

  it('khi lỗi tải dữ liệu (isError = true), hiển thị lỗi trong body và nút Thử lại', async () => {
    const handleRetry = jest.fn();

    await render(
      <PostList
        posts={[]}
        actionTemplates={[]}
        filterChipsProps={sampleFilterChips}
        isLoading={false}
        isError={true}
        onRetry={handleRetry}
      />,
    );

    expect(screen.getByTestId('feed-body-error')).toBeTruthy();
    expect(screen.getByText('Không tải được bảng tin')).toBeTruthy();

    const retryBtn = screen.getByRole('button', { name: 'Thử lại' });
    fireEvent.press(retryBtn);
    expect(handleRetry).toHaveBeenCalledTimes(1);
  });

  it('khi không có bài viết (empty state), hiển thị thông báo rỗng trong body', async () => {
    await render(
      <PostList
        posts={[]}
        actionTemplates={[]}
        filterChipsProps={sampleFilterChips}
        isLoading={false}
        isError={false}
      />,
    );

    expect(screen.getByTestId('feed-body-empty')).toBeTruthy();
    expect(screen.getByText('Chưa có bài viết')).toBeTruthy();
  });
});
