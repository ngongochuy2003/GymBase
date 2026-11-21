import { act, cleanup, fireEvent, render } from '@testing-library/react-native';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import {
  CommentBottomSheet,
  feedRepository,
  PostList,
  postActionTemplates,
  savedPostsStorage,
  type BinhLuanDto,
  type FeedPostData,
} from '@/features/feed';

jest.mock('@/core/auth/session', () => ({
  useSession: () => ({
    user: { id: 'u1', ho_ten: 'Test User', anh_dai_dien_url: null },
    isAuthenticated: true,
  }),
  useOptionalSession: () => ({
    user: { id: 'u1', ho_ten: 'Test User', anh_dai_dien_url: null },
    isAuthenticated: true,
  }),
}));

describe('Feed Interactions (Like, Save, Comment)', () => {
  let queryClient: QueryClient;

  beforeEach(async () => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false, gcTime: 0 },
      },
    });
    await savedPostsStorage.clear();
  });

  afterEach(async () => {
    cleanup();
    queryClient.clear();
    jest.restoreAllMocks();
  });

  describe('savedPostsStorage', () => {
    it('quản lý lưu và bỏ lưu bài viết chính xác', async () => {
      const postId = 'test-post-1';
      expect(savedPostsStorage.isSaved(postId)).toBe(false);

      const saved = await savedPostsStorage.toggleSavePost(postId);
      expect(saved).toBe(true);
      expect(savedPostsStorage.isSaved(postId)).toBe(true);

      const ids = await savedPostsStorage.getSavedPostIds();
      expect(ids).toContain(postId);

      const unsaved = await savedPostsStorage.toggleSavePost(postId);
      expect(unsaved).toBe(false);
      expect(savedPostsStorage.isSaved(postId)).toBe(false);
    });
  });

  describe('PostCard & PostList Actions', () => {
    const mockPost: FeedPostData = {
      id: 'post-card-1',
      author: { name: 'Người tập Gym' },
      timeLabel: '5 phút trước',
      content: 'Hôm nay tập ngực rất tốt!',
      counts: { like: 12, comment: 3 },
      initialActive: { like: false, save: false },
    };

    it('bấm action like và save hoạt động độc lập', async () => {
      const onActionPress = jest.fn();
      const utils = await render(
        <PostList
          posts={[mockPost]}
          actionTemplates={postActionTemplates}
          onActionPress={onActionPress}
        />,
      );

      const likeBtn = utils.getByTestId('post-action-like');
      const saveBtn = utils.getByTestId('post-action-save');

      expect(likeBtn).toBeTruthy();
      expect(saveBtn).toBeTruthy();

      // Bấm like
      await act(async () => {
        fireEvent.press(likeBtn);
      });
      expect(onActionPress).toHaveBeenCalledWith('post-card-1', 'like');

      // Bấm save
      await act(async () => {
        fireEvent.press(saveBtn);
      });
      expect(onActionPress).toHaveBeenCalledWith('post-card-1', 'save');
    });
  });

  describe('CommentBottomSheet', () => {
    const targetPost: FeedPostData = {
      id: 'target-post-id',
      author: { name: 'HLV Minh' },
      timeLabel: '20 phút trước',
      content: 'Lịch tập tuần này đã cập nhật.',
      counts: { like: 50, comment: 2 },
    };

    const mockComments: BinhLuanDto[] = [
      {
        id: 'c1',
        bai_viet_id: 'target-post-id',
        tai_khoan_id: 'acc1',
        ten_nguoi_dung: 'Học viên A',
        anh_dai_dien_url: null,
        noi_dung: 'Em cảm ơn thầy nhiều ạ!',
        tuong_tac_cha_id: null,
        ngay_tao: '2026-11-17T09:00:00Z',
      },
    ];

    it('không hiển thị khi visible = false', async () => {
      const utils = await render(
        <QueryClientProvider client={queryClient}>
          <CommentBottomSheet
            visible={false}
            post={targetPost}
            onClose={jest.fn()}
          />
        </QueryClientProvider>,
      );
      expect(utils.queryByTestId('comment-sheet-container')).toBeNull();
    });

    it('hiển thị modal bình luận, cho phép trả lời, đổi bộ lọc và đóng modal', async () => {
      jest.spyOn(feedRepository, 'getComments').mockResolvedValue(mockComments);
      const onClose = jest.fn();

      const utils = await render(
        <QueryClientProvider client={queryClient}>
          <CommentBottomSheet
            visible={true}
            post={targetPost}
            onClose={onClose}
          />
        </QueryClientProvider>,
      );

      // 1. Kiểm tra hiển thị modal và nội dung bình luận
      expect(await utils.findByTestId('comment-sheet-container')).toBeTruthy();
      expect(await utils.findByText('Bình luận')).toBeTruthy();
      expect(await utils.findByText('Phù hợp nhất')).toBeTruthy();
      expect(await utils.findByPlaceholderText('Viết bình luận...')).toBeTruthy();
      expect(await utils.findByText('Học viên A')).toBeTruthy();
      expect(await utils.findByText('Em cảm ơn thầy nhiều ạ!')).toBeTruthy();

      // 2. Bấm nút "Trả lời"
      const replyBtn = await utils.findByText('Trả lời');
      await act(async () => {
        fireEvent.press(replyBtn);
      });

      // 3. Thấy thanh đang trả lời
      expect(await utils.findByTestId('replying-bar')).toBeTruthy();
      expect(utils.getAllByText(/Học viên A/).length).toBe(2);

      // 4. Bấm bộ lọc để đổi sang "Mới nhất"
      const filterBtn = await utils.findByLabelText('Chọn bộ lọc bình luận');
      await act(async () => {
        fireEvent.press(filterBtn);
      });
      expect(await utils.findByText('Mới nhất')).toBeTruthy();

      // 5. Bấm nút đóng modal
      const closeBtn = await utils.findByTestId('comment-modal-close-button');
      await act(async () => {
        fireEvent.press(closeBtn);
      });
      expect(onClose).toHaveBeenCalled();
    });
  });
});
