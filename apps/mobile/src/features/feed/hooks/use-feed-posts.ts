import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { feedRepository, type FeedRepository } from '../api/feed-api';

export const FEED_QUERY_KEY = ['feed', 'posts'] as const;

export function useFeedPosts(tab: string, repository: FeedRepository = feedRepository) {
  return useQuery({ queryKey: [...FEED_QUERY_KEY, tab], queryFn: () => repository.getFeedPosts(tab) });
}

export function useToggleLikeMutation(repository: FeedRepository = feedRepository) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (postId: string) => repository.toggleLike(postId),
    onSuccess: () => { void queryClient.invalidateQueries({ queryKey: FEED_QUERY_KEY }); },
  });
}
