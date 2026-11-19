import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  feedRepository,
  type BinhLuanCreateDto,
  type BinhLuanDto,
  type FeedRepository,
} from '../api/feed-api';
import { FEED_QUERY_KEY } from './use-feed-posts';

export const COMMENTS_QUERY_KEY = ['feed', 'comments'] as const;

export function usePostComments(
  postId: string | undefined,
  repository: FeedRepository = feedRepository,
) {
  return useQuery<BinhLuanDto[]>({
    queryKey: [...COMMENTS_QUERY_KEY, postId],
    queryFn: () => (postId ? repository.getComments(postId) : Promise.resolve([])),
    enabled: Boolean(postId),
  });
}

export function useAddCommentMutation(
  postId: string,
  repository: FeedRepository = feedRepository,
) {
  const queryClient = useQueryClient();

  return useMutation<BinhLuanDto, Error, BinhLuanCreateDto>({
    mutationFn: (dto) => repository.addComment(postId, dto),
    onSuccess: (newComment) => {
      // 1. Cập nhật cache bình luận ngay lập tức
      queryClient.setQueryData<BinhLuanDto[]>(
        [...COMMENTS_QUERY_KEY, postId],
        (old = []) => [...old, newComment],
      );
      // 2. Invalidate để đồng bộ lại dữ liệu chuẩn từ server
      void queryClient.invalidateQueries({
        queryKey: [...COMMENTS_QUERY_KEY, postId],
      });
      // 3. Invalidate feed để cập nhật số lượng bình luận hiển thị trên bảng tin
      void queryClient.invalidateQueries({
        queryKey: FEED_QUERY_KEY,
      });
      // 4. Invalidate post-detail
      void queryClient.invalidateQueries({
        queryKey: ['post-detail', postId],
      });
    },
  });
}
