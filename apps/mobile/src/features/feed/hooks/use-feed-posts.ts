import {
  useInfiniteQuery,
  useMutation,
  useQueryClient,
  type InfiniteData,
} from '@tanstack/react-query';
import { feedRepository, type FeedPageResult, type FeedRepository } from '../api/feed-api';
import type { ReportReasonId } from '../types';

export const FEED_QUERY_KEY = ['feed', 'posts'] as const;
export const DEFAULT_FEED_PAGE_SIZE = 10;

export function useFeedPosts(
  tab: string,
  pageSize = DEFAULT_FEED_PAGE_SIZE,
  repository: FeedRepository = feedRepository,
) {
  const query = useInfiniteQuery({
    queryKey: [...FEED_QUERY_KEY, tab, pageSize],
    queryFn: ({ pageParam = 1 }) => repository.getPaginatedFeed(tab, pageParam, pageSize),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => (lastPage.hasNextPage ? lastPage.page + 1 : undefined),
  });

  const posts = query.data?.pages.flatMap((page) => page.posts) ?? [];

  return {
    ...query,
    posts,
    hasNextPage: Boolean(query.hasNextPage),
    isFetchingNextPage: query.isFetchingNextPage,
    fetchNextPage: query.fetchNextPage,
  };
}

export function useToggleLikeMutation(repository: FeedRepository = feedRepository) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (postId: string) => repository.toggleLike(postId),
    onSuccess: (result) => {
      queryClient.setQueriesData<InfiniteData<FeedPageResult>>(
        { queryKey: FEED_QUERY_KEY },
        (old) => {
          if (!old) return old;
          return {
            ...old,
            pages: old.pages.map((page) => ({
              ...page,
              posts: page.posts.map((post) =>
                post.id === result.bai_viet_id
                  ? {
                      ...post,
                      counts: { ...post.counts, like: result.so_luot_thich },
                      initialActive: { ...post.initialActive, like: result.da_thich },
                    }
                  : post,
              ),
            })),
          };
        },
      );
      void queryClient.invalidateQueries({ queryKey: ['post-detail', result.bai_viet_id] });
    },
  });
}

export function useDeletePostMutation(repository: FeedRepository = feedRepository) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (postId: string) => repository.deletePost(postId),
    onSuccess: (_data, postId) => {
      queryClient.setQueriesData<InfiniteData<FeedPageResult>>(
        { queryKey: FEED_QUERY_KEY },
        (old) => {
          if (!old) return old;
          return {
            ...old,
            pages: old.pages.map((page) => ({
              ...page,
              posts: page.posts.filter((post) => post.id !== postId),
            })),
          };
        },
      );
      void queryClient.invalidateQueries({ queryKey: FEED_QUERY_KEY });
    },
  });
}

export function useReportPostMutation(repository: FeedRepository = feedRepository) {
  return useMutation({
    mutationFn: ({
      postId,
      reason,
      description,
    }: {
      postId: string;
      reason: ReportReasonId;
      description?: string;
    }) => repository.reportPost(postId, reason, description),
  });
}
