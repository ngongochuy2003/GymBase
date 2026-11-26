import { fireEvent, render, screen } from '@testing-library/react-native';
import { PostCard, PostOptionsSheet, type FeedPostData } from '@/features/feed';
import { ScreenRenderer, type ScreenConfig } from '@/shared/screen-config';

jest.mock('expo-router', () => ({
  useRouter: () => ({ navigate: jest.fn(), back: jest.fn(), canGoBack: () => false }),
}));

const samplePost: FeedPostData = {
  id: 'post-1',
  authorId: 'user-1',
  author: { name: 'Minh Lê' },
  timeLabel: '5 phút trước',
  content: 'Hôm nay tập ngực và vai nhẹ nhàng.',
  counts: { like: 3, comment: 2 },
  initialActive: { like: false, save: false },
};

describe('Feed interactions and header divider', () => {
  test('ScreenRenderer không vẽ đường line dưới header khi collapsibleHeader bật', async () => {
    const config: ScreenConfig = {
      id: 'feed-no-line',
      header: {},
      sections: [],
      bottomTabs: [],
    };
    await render(<ScreenRenderer config={config} />);

    const headerContainer = screen.getByTestId('screen-header-container');
    const flatStyle = Object.assign(
      {},
      ...[headerContainer.props.style].flat().filter(Boolean),
    );
    expect(flatStyle.borderBottomWidth).toBeUndefined();
  });

  test('PostCard hiển thị nút xóa bài viết bên phải dấu 3 chấm và gọi callback', async () => {
    const onMorePress = jest.fn();
    const onDeletePress = jest.fn();

    await render(
      <PostCard
        author={samplePost.author}
        timeLabel={samplePost.timeLabel}
        content={samplePost.content}
        actions={[]}
        onMorePress={onMorePress}
        onDeletePress={onDeletePress}
      />,
    );

    await fireEvent.press(screen.getByTestId('post-more-button'));
    expect(onMorePress).toHaveBeenCalledTimes(1);

    await fireEvent.press(screen.getByTestId('post-delete-button'));
    expect(onDeletePress).toHaveBeenCalledTimes(1);
  });

  test('PostOptionsSheet hiển thị đủ action và chuyển sang Lựa chọn khác -> Sao chép liên kết', async () => {
    const onInterested = jest.fn();
    const onNotInterested = jest.fn();
    const onToggleSave = jest.fn();
    const onReport = jest.fn();
    const onCopyLink = jest.fn();
    const onClose = jest.fn();

    await render(
      <PostOptionsSheet
        post={samplePost}
        isSaved={false}
        onClose={onClose}
        onInterested={onInterested}
        onNotInterested={onNotInterested}
        onToggleSave={onToggleSave}
        onReport={onReport}
        onCopyLink={onCopyLink}
      />,
    );

    expect(screen.getByText('Quan tâm')).toBeTruthy();
    expect(screen.getByText('Không quan tâm')).toBeTruthy();
    expect(screen.getByText('Lưu bài viết')).toBeTruthy();
    expect(screen.getByText('Báo cáo bài viết')).toBeTruthy();
    expect(screen.getByText('Lựa chọn khác')).toBeTruthy();

    await fireEvent.press(screen.getByTestId('option-more-choices'));
    expect(screen.getByText('Sao chép liên kết')).toBeTruthy();

    await fireEvent.press(screen.getByTestId('option-copy-link'));
    expect(onCopyLink).toHaveBeenCalledWith(samplePost);
  });
});
