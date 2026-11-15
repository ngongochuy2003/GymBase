import { useCallback, useMemo, useState } from 'react';
import { useRouter } from 'expo-router';
import { useSession } from '@/core/auth/session';
import { DEFAULT_HEADER_ACTIONS, useTabNavigation } from '@/shared/navigation';
import { ScreenRenderer, registerSection, type ScreenConfig } from '@/shared/screen-config';
import { BottomTabBar } from '@/shared/ui-kit';
import { EmptyState, ErrorState, LoadingState, Screen } from '@/shared/components/ui';
import { resolveMediaUrl } from '../api/feed-api';
import { PostList } from '../components/post-list';
import { useFeedPosts, useToggleLikeMutation } from '../hooks/use-feed-posts';
import { feedFilters, postActionTemplates } from '../config/feed-config';
import { feedTabs } from '../config/tabs';

registerSection('postList', PostList);

const EMPTY_POSTS: import('../types').FeedPostData[] = [];

export function FeedScreen() {
  const router = useRouter();
  const onTabPress = useTabNavigation();
  const { user } = useSession();
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

  const avatarUri = useMemo(() => {
    if (user) {
      const userPost = posts.find(
        (p) => p.author.name === user.ho_ten || p.author.name === user.ten_dang_nhap,
      );
      if (userPost?.author.avatarUri) {
        return userPost.author.avatarUri;
      }
    }
    if (
      user?.ten_dang_nhap === 'minhle' ||
      user?.email?.toLowerCase().includes('tuanminh30042005') ||
      user?.id === '01a1d5ef-6706-74df-bfc8-0f150d22cad2'
    ) {
      return resolveMediaUrl('/api/v1/mob/files/f7c8dabf-aa78-45af-833c-6884551708e1/content');
    }
    return undefined;
  }, [posts, user]);

  const config = useMemo<ScreenConfig>(
    () => ({
      id: 'feed',
      header: {
        rightActions: DEFAULT_HEADER_ACTIONS,
        avatarUri,
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
    [activeFilter, avatarUri, handleActionPress, posts, router],
  );

  const footer = <BottomTabBar tabs={feedTabs} activeId="home" onChange={onTabPress} />;

  if (feed.isPending) return <Screen footer={footer}><LoadingState /></Screen>;
  if (feed.isError) return <Screen footer={footer}><ErrorState message="Không tải được bảng tin từ máy chủ." onRetry={() => void feed.refetch()} /></Screen>;
  if (posts.length === 0) return <Screen footer={footer}><EmptyState message="Chưa có bài viết. Hãy là người đầu tiên chia sẻ tiến trình tập luyện." /></Screen>;
  return <ScreenRenderer config={config} onTabPress={onTabPress} />;
}

