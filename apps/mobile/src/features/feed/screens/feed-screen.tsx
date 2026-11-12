import { useCallback, useMemo, useState } from 'react';
import { useRouter } from 'expo-router';
import { currentUser } from '@/shared/mock/current-user';
import { DEFAULT_HEADER_ACTIONS, useTabNavigation } from '@/shared/navigation';
import { ScreenRenderer, registerSection, type ScreenConfig } from '@/shared/screen-config';
import { PostList } from '../components/post-list';
import { useFeedPosts, useToggleLikeMutation } from '../hooks/use-feed-posts';
import { feedFilters, postActionTemplates } from '../mock/feed-config';
import { feedPosts } from '../mock/feed-posts';
import { feedTabs } from '../mock/tabs';

registerSection('postList', PostList);

export function FeedScreen() {
  const router = useRouter();
  const onTabPress = useTabNavigation();
  const [activeFilter, setActiveFilter] = useState(feedFilters[0]?.id ?? 'for-you');
  const { data: posts = feedPosts } = useFeedPosts(activeFilter);
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
        avatarUri: currentUser.avatarUri,
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

  return <ScreenRenderer config={config} onTabPress={onTabPress} />;
}
