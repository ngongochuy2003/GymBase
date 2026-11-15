import { useInfiniteQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { feedRepository, type FeedRepository } from '../api/feed-api';

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
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: FEED_QUERY_KEY });
    },
  });
}
