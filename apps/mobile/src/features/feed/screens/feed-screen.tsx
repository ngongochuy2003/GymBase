import { useCallback, useMemo, useState } from 'react';
import { View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSession } from '@/core/auth/session';
import { DEFAULT_HEADER_ACTIONS, useTabNavigation } from '@/shared/navigation';
import { ScreenRenderer, registerSection, type ScreenConfig } from '@/shared/screen-config';
import { resolveMediaUrl } from '../api/feed-api';
import { CommentBottomSheet } from '../components/comment-bottom-sheet';
import { PostList } from '../components/post-list';
import { useFeedPosts, useToggleLikeMutation } from '../hooks/use-feed-posts';
import { useSavedPosts } from '../hooks/use-saved-posts';
import { feedFilters, postActionTemplates } from '../config/feed-config';
import { feedTabs } from '../config/tabs';
import type { FeedPostData } from '../types';

registerSection('postList', PostList);

export function FeedScreen() {
  const router = useRouter();
  const onTabPress = useTabNavigation();
  const { user } = useSession();
  const [activeFilter, setActiveFilter] = useState(feedFilters[0]?.id ?? 'for-you');
  const feed = useFeedPosts(activeFilter);
  const posts = feed.posts;
  const { mutate: toggleLike } = useToggleLikeMutation();
  const { isSaved, toggleSave } = useSavedPosts();

  const [activeCommentPost, setActiveCommentPost] = useState<FeedPostData | null>(null);

  const postsWithSaved = useMemo(() => {
    return posts.map((post) => ({
      ...post,
      initialActive: {
        ...post.initialActive,
        save: isSaved(post.id),
      },
    }));
  }, [isSaved, posts]);

  const handleActionPress = useCallback(
    (postId: string, actionId: string) => {
      if (actionId === 'like') {
        toggleLike(postId);
      } else if (actionId === 'save') {
        void toggleSave(postId);
      } else if (actionId === 'comment') {
        const targetPost = postsWithSaved.find((p) => p.id === postId);
        if (targetPost) {
          setActiveCommentPost(targetPost);
        }
      }
    },
    [postsWithSaved, toggleLike, toggleSave],
  );

  const handleEndReached = useCallback(() => {
    if (!feed.isFetchingNextPage && feed.hasNextPage) {
      void feed.fetchNextPage();
    }
  }, [feed]);

  const handleRefresh = useCallback(() => {
    void feed.refetch();
  }, [feed]);

  const handleRetry = useCallback(() => {
    void feed.refetch();
  }, [feed]);

  const handleOpenPostDetail = useCallback(
    (post: FeedPostData) => {
      router.push({
        pathname: '/(protected)/post-detail',
        params: {
          postId: post.id,
          postJson: JSON.stringify(post),
        },
      });
    },
    [router],
  );

  const avatarUri = useMemo(() => {
    if (user) {
      const userPost = posts.find((p) => p.author.name === user.ho_ten);
      if (userPost?.author.avatarUri) {
        return userPost.author.avatarUri;
      }
    }
    if (
      user?.email?.toLowerCase().includes('tuanminh30042005') ||
      user?.id === '01a1d5ef-6706-74df-bfc8-0f150d22cad2'
    ) {
      return resolveMediaUrl('/api/v1/mob/files/f7c8dabf-aa78-45af-833c-6884551708e1/content');
    }
    return undefined;
  }, [posts, user]);

  const isLoading = feed.isPending || (feed.isFetching && !feed.isFetchingNextPage);

  const config = useMemo<ScreenConfig>(
    () => ({
      id: 'feed',
      scrollable: false, // Dùng FlatList ảo hóa trong PostList làm scroll view chính
      header: {
        rightActions: DEFAULT_HEADER_ACTIONS,
      },
      sections: [
        {
          id: 'posts',
          type: 'postList',
          props: {
            posts: postsWithSaved,
            actionTemplates: postActionTemplates,
            onActionPress: handleActionPress,
            filterChipsProps: {
              items: feedFilters,
              activeId: activeFilter,
              onChange: setActiveFilter,
            },
            onEndReached: handleEndReached,
            hasNextPage: feed.hasNextPage,
            isFetchingNextPage: feed.isFetchingNextPage,
            onRefresh: handleRefresh,
            refreshing: feed.isRefetching,
            isLoading,
            isError: feed.isError,
            onRetry: handleRetry,
            onPostPress: handleOpenPostDetail,
            onImagePress: (post: FeedPostData) => handleOpenPostDetail(post),
          },
        },
      ],
      bottomTabs: feedTabs,
      activeTabId: 'home',
    }),
    [
      activeFilter,
      feed.hasNextPage,
      feed.isError,
      feed.isFetchingNextPage,
      feed.isRefetching,
      handleActionPress,
      handleEndReached,
      handleOpenPostDetail,
      handleRefresh,
      handleRetry,
      isLoading,
      postsWithSaved,
    ],
  );

  return (
    <View style={{ flex: 1 }}>
      <ScreenRenderer config={config} onTabPress={onTabPress} />
      <CommentBottomSheet
        visible={Boolean(activeCommentPost)}
        post={activeCommentPost}
        onClose={() => setActiveCommentPost(null)}
        currentUserAvatar={avatarUri}
      />
    </View>
  );
}
