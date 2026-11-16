import { act, fireEvent, render, renderHook, screen } from '@testing-library/react-native';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import type { AxiosInstance } from 'axios';
import type { ReactNode } from 'react';
import {
  createFeedRepository,
  useFeedPosts,
  PostList,
  type BaiVietDto,
  type FeedPostData,
  type PagingResultDto,
} from '@/features/feed';

jest.mock('expo-router', () => ({
  useRouter: () => ({ navigate: jest.fn(), back: jest.fn(), canGoBack: () => false }),
}));

function createMockPostDto(id: string, authorName: string): BaiVietDto {
  return {
    id,
    tac_gia_id: `author-${id}`,
    tac_gia: {
      id: `author-${id}`,
      ten_hien_thi: authorName,
      anh_dai_dien_url: null,
    },
    noi_dung: `Nội dung bài viết số ${id}`,
    loai_bai_viet: 'hon_hop',
    che_do_hien_thi: 'cong_khai',
    trang_thai: 'da_dang',
    chu_de: 'workout',
    hinh_anh_urls: [],
    so_luot_thich: 5,
    so_binh_luan: 2,
    da_thich: false,
    ngay_dang: '2026-11-15T08:00:00Z',
    ngay_tao: '2026-11-15T08:00:00Z',
    ngay_chinh_sua: null,
  };
}

describe('Feed Pagination & Lazy Loading', () => {
  test('createFeedRepository.getPaginatedFeed gửi đúng params và tính hasNextPage', async () => {
    const page1Data: PagingResultDto<BaiVietDto> = {
      data: [createMockPostDto('1', 'Minh Lê'), createMockPostDto('2', 'Văn Nam')],
      meta: {
        page: 1,
        page_size: 2,
        total: 3,
        total_pages: 2,
      },
    };

    const mockClient = {
      get: jest.fn().mockResolvedValue({ data: page1Data }),
      post: jest.fn(),
    } as unknown as AxiosInstance;

    const repo = createFeedRepository(mockClient);
    const result = await repo.getPaginatedFeed('for-you', 1, 2);

    expect(mockClient.get).toHaveBeenCalledWith('/baiviet/feed', {
      params: { tab: 'for-you', page: 1, page_size: 2 },
    });
    expect(result.posts.length).toBe(2);
    expect(result.posts[0]?.author.name).toBe('Minh Lê');
    expect(result.page).toBe(1);
    expect(result.pageSize).toBe(2);
    expect(result.total).toBe(3);
    expect(result.totalPages).toBe(2);
    expect(result.hasNextPage).toBe(true);
  });

  test('createFeedRepository.getPaginatedFeed đặt hasNextPage = false khi ở trang cuối', async () => {
    const page2Data: PagingResultDto<BaiVietDto> = {
      data: [createMockPostDto('3', 'Tuấn Minh')],
      meta: {
        page: 2,
        page_size: 2,
        total: 3,
        total_pages: 2,
      },
    };

    const mockClient = {
      get: jest.fn().mockResolvedValue({ data: page2Data }),
      post: jest.fn(),
    } as unknown as AxiosInstance;

    const repo = createFeedRepository(mockClient);
    const result = await repo.getPaginatedFeed('for-you', 2, 2);

    expect(result.posts.length).toBe(1);
    expect(result.page).toBe(2);
    expect(result.hasNextPage).toBe(false);
  });

  test('useFeedPosts hook hỗ trợ infinite scrolling với fetchNextPage', async () => {
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false, gcTime: 0 },
      },
    });

    const wrapper = ({ children }: { children: ReactNode }) => (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    );

    const mockRepo = {
      getFeedPosts: jest.fn(),
      getPostById: jest.fn(),
      toggleLike: jest.fn(),
      getPaginatedFeed: jest.fn().mockImplementation((_tab, page) => {
        if (page === 1) {
          return Promise.resolve({
            posts: [
              {
                id: 'p1',
                author: { name: 'Người dùng 1' },
                timeLabel: '1 giờ trước',
                content: 'Bài viết 1',
              },
            ],
            page: 1,
            pageSize: 1,
            total: 2,
            totalPages: 2,
            hasNextPage: true,
          });
        }
        return Promise.resolve({
          posts: [
            {
              id: 'p2',
              author: { name: 'Người dùng 2' },
              timeLabel: 'Vừa xong',
              content: 'Bài viết 2',
            },
          ],
          page: 2,
          pageSize: 1,
          total: 2,
          totalPages: 2,
          hasNextPage: false,
        });
      }),
    };

    const { result } = await renderHook(() => useFeedPosts('for-you', 1, mockRepo), { wrapper });

    // Đợi query trang 1 hoàn tất
    await act(async () => {
      await new Promise((resolve) => setTimeout(resolve, 50));
    });

    expect(result.current.posts.length).toBe(1);
    expect(result.current.posts[0]?.content).toBe('Bài viết 1');
    expect(result.current.hasNextPage).toBe(true);

    // Kích hoạt fetchNextPage
    await act(async () => {
      await result.current.fetchNextPage();
      await new Promise((resolve) => setTimeout(resolve, 50));
    });

    expect(result.current.posts.length).toBe(2);
    expect(result.current.posts[1]?.content).toBe('Bài viết 2');
    expect(result.current.hasNextPage).toBe(false);
  });

  test('PostList render danh sách với loading indicator và end-of-list message', async () => {
    const samplePosts: FeedPostData[] = [
      {
        id: 'post-1',
        author: { name: 'HLV Minh' },
        timeLabel: '10 phút trước',
        content: 'Lịch tập ngực hôm nay',
      },
    ];

    // Case 1: Đang tải trang tiếp theo -> hiển thị spinner
    const { rerender } = await render(
      <PostList
        posts={samplePosts}
        actionTemplates={[]}
        isFetchingNextPage={true}
        hasNextPage={true}
      />,
    );

    expect(screen.getByTestId('feed-loading-more')).toBeTruthy();
    expect(screen.getByText('Đang tải thêm bài viết...')).toBeTruthy();

    // Case 2: Đã xem hết bài viết -> hiển thị thông báo kết thúc
    await rerender(
      <PostList
        posts={samplePosts}
        actionTemplates={[]}
        isFetchingNextPage={false}
        hasNextPage={false}
      />,
    );

    expect(screen.getByTestId('feed-end-of-list')).toBeTruthy();
    expect(screen.getByText('Bạn đã xem hết bài viết mới nhất')).toBeTruthy();
  });

  test('PostList kích hoạt onEndReached khi cuộn đến ngưỡng', async () => {
    const handleEndReached = jest.fn();
    const samplePosts: FeedPostData[] = [
      {
        id: 'post-1',
        author: { name: 'HLV Minh' },
        timeLabel: '10 phút trước',
        content: 'Bài viết 1',
      },
    ];

    await render(
      <PostList
        posts={samplePosts}
        actionTemplates={[]}
        onEndReached={handleEndReached}
      />,
    );

    const flatList = screen.getByTestId('screen-scroll-view');
    fireEvent(flatList, 'onEndReached');

    expect(handleEndReached).toHaveBeenCalled();
  });
});
