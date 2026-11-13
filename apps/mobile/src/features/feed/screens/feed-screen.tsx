import { useCallback, useMemo, useState } from 'react';
import { useRouter } from 'expo-router';
import { DEFAULT_HEADER_ACTIONS, useTabNavigation } from '@/shared/navigation';
import { ScreenRenderer, registerSection, type ScreenConfig } from '@/shared/screen-config';
import { EmptyState, ErrorState, LoadingState, Screen } from '@/shared/components/ui';
import { PostList } from '../components/post-list';
import { useFeedPosts, useToggleLikeMutation } from '../hooks/use-feed-posts';
import { feedFilters, postActionTemplates } from '../config/feed-config';
import { feedTabs } from '../config/tabs';

registerSection('postList', PostList);

const EMPTY_POSTS: import('../types').FeedPostData[] = [];

export function FeedScreen() {
  const router = useRouter();
  const onTabPress = useTabNavigation();
  const [activeFilter, setActiveFilter] = useState(feedFilters[0]?.id ?? 'for-you');
  const feed = useFeedPosts(activeFilter);
  const posts = feed.data ?? EMPTY_POSTS;
  const { mutate: toggleLike } = useToggleLikeMutation();

  const handleActionPress = useCallback(
    (postId: string, actionId: string) => {
      if (actionId === 'like') {
        toggleLike(postId);
      }
    },
    [toggleLike],
  );

  const config = useMemo<ScreenConfig>(
    () => ({
      id: 'feed',
      header: {
        rightActions: DEFAULT_HEADER_ACTIONS,
        onAvatarPress: () => router.navigate('/profile'),
      },
      sections: [
        {
          id: 'filters',
          type: 'filterChips',
          props: { items: feedFilters, activeId: activeFilter, onChange: setActiveFilter },
        },
        {
          id: 'posts',
          type: 'postList',
          props: {
            posts,
            actionTemplates: postActionTemplates,
            onActionPress: handleActionPress,
          },
        },
      ],
      bottomTabs: feedTabs,
      activeTabId: 'home',
    }),
    [activeFilter, handleActionPress, posts, router],
  );

  if (feed.isPending) return <Screen><LoadingState /></Screen>;
  if (feed.isError) return <Screen><ErrorState message="Không tải được bảng tin từ máy chủ." onRetry={() => void feed.refetch()} /></Screen>;
  if (posts.length === 0) return <Screen><EmptyState message="Chưa có bài viết. Hãy là người đầu tiên chia sẻ tiến trình tập luyện." /></Screen>;
  return <ScreenRenderer config={config} onTabPress={onTabPress} />;
}

