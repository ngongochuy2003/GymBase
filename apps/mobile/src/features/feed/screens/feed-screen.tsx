import { useCallback, useMemo, useState } from 'react';
import { Alert, Platform, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSession } from '@/core/auth/session';
import { DEFAULT_HEADER_ACTIONS, useTabNavigation } from '@/shared/navigation';
import { ScreenRenderer, registerSection, type ScreenConfig } from '@/shared/screen-config';
import { useToast } from '@/shared/ui-kit';
import { resolveMediaUrl } from '../api/feed-api';
import { CommentBottomSheet } from '../components/comment-bottom-sheet';
import { PostList } from '../components/post-list';
import { PostOptionsSheet } from '../components/post-options-sheet';
import { feedFilters, postActionTemplates } from '../config/feed-config';
import { feedTabs } from '../config/tabs';
import {
  useDeletePostMutation,
  useFeedPosts,
  useReportPostMutation,
  useToggleLikeMutation,
} from '../hooks/use-feed-posts';
import { useSavedPosts } from '../hooks/use-saved-posts';
import type { FeedPostData, ReportReasonId } from '../types';
import { copyPostLink } from '../utils/post-actions';

registerSection('postList', PostList);

export function FeedScreen() {
  const router = useRouter();
  const onTabPress = useTabNavigation();
  const { user } = useSession();
  const { toast, show: showToast } = useToast();

  const [activeFilter, setActiveFilter] = useState(feedFilters[0]?.id ?? 'for-you');
  const feed = useFeedPosts(activeFilter);
  const posts = feed.posts;
  const { mutate: toggleLike } = useToggleLikeMutation();
  const deletePostMutation = useDeletePostMutation();
  const reportPostMutation = useReportPostMutation();
  const { isSaved, toggleSave } = useSavedPosts();

  const [hiddenPostIds, setHiddenPostIds] = useState<Set<string>>(() => new Set());
  const [activeCommentPost, setActiveCommentPost] = useState<FeedPostData | null>(null);
  const [optionsPost, setOptionsPost] = useState<FeedPostData | null>(null);

  const postsWithSaved = useMemo(() => {
    return posts
      .filter((post) => !hiddenPostIds.has(post.id))
      .map((post) => ({
        ...post,
        initialActive: {
          ...post.initialActive,
          save: isSaved(post.id),
        },
      }));
  }, [hiddenPostIds, isSaved, posts]);

  const handleToggleSave = useCallback(
    (postId: string) => {
      const nextSaved = !isSaved(postId);
      void toggleSave(postId);
      showToast(nextSaved ? 'Đã lưu bài viết' : 'Đã bỏ lưu bài viết');
    },
    [isSaved, showToast, toggleSave],
  );

  const handleCopyLink = useCallback(
    (post: FeedPostData) => {
      void copyPostLink(post.id)
        .then((mode) => {
          if (mode === 'copied') {
            showToast('Đã sao chép liên kết');
          }
        })
        .catch(() => {
          showToast('Không thể sao chép liên kết');
        });
    },
    [showToast],
  );

  const handleActionPress = useCallback(
    (postId: string, actionId: string) => {
      if (actionId === 'like') {
        toggleLike(postId);
      } else if (actionId === 'save') {
        handleToggleSave(postId);
      } else if (actionId === 'comment') {
        const targetPost = postsWithSaved.find((p) => p.id === postId);
        if (targetPost) {
          setActiveCommentPost(targetPost);
        }
      } else if (actionId === 'share') {
        const targetPost = postsWithSaved.find((p) => p.id === postId);
        if (targetPost) {
          handleCopyLink(targetPost);
        }
      }
    },
    [handleCopyLink, handleToggleSave, postsWithSaved, toggleLike],
  );

  const handleInterested = useCallback(
    (_post: FeedPostData) => {
      // TODO(QUAN_TAM_BAI_VIET): Gửi tín hiệu quan tâm lên backend khi có API gợi ý bảng tin.
      showToast('Chúng tôi sẽ hiển thị nhiều bài viết tương tự hơn');
    },
    [showToast],
  );

  const handleNotInterested = useCallback(
    (post: FeedPostData) => {
      setHiddenPostIds((prev) => {
        const next = new Set(prev);
        next.add(post.id);
        return next;
      });
      showToast('Đã ẩn bài viết khỏi bảng tin');
    },
    [showToast],
  );

  const handleReportPost = useCallback(
    (post: FeedPostData, reason: ReportReasonId) => {
      reportPostMutation.mutate(
        { postId: post.id, reason },
        {
          onSuccess: () => {
            setOptionsPost(null);
            showToast('Đã gửi báo cáo bài viết');
          },
          onError: () => {
            showToast('Không thể gửi báo cáo. Vui lòng thử lại.');
          },
        },
      );
    },
    [reportPostMutation, showToast],
  );

  const executeDeletePost = useCallback(
    (post: FeedPostData) => {
      const currentUserId = user?.id;
      const isOwner = Boolean(currentUserId && post.authorId && currentUserId === post.authorId);
      if (!isOwner) {
        setHiddenPostIds((prev) => {
          const next = new Set(prev);
          next.add(post.id);
          return next;
        });
        showToast('Đã ẩn bài viết');
        return;
      }

      deletePostMutation.mutate(post.id, {
        onSuccess: () => {
          showToast('Đã xóa bài viết');
        },
        onError: () => {
          showToast('Không thể xóa bài viết. Vui lòng thử lại.');
        },
      });
    },
    [deletePostMutation, showToast, user],
  );

  const handleDeletePress = useCallback(
    (post: FeedPostData) => {
      const currentUserId = user?.id;
      const isOwner = Boolean(currentUserId && post.authorId && currentUserId === post.authorId);
      const title = isOwner ? 'Xóa bài viết' : 'Ẩn bài viết';
      const message = isOwner
        ? 'Bạn có chắc chắn muốn xóa bài viết này không?'
        : 'Bạn muốn ẩn bài viết này khỏi bảng tin?';

      if (Platform.OS === 'web') {
        const confirmFn = globalThis.confirm;
        if (typeof confirmFn !== 'function' || confirmFn(message)) {
          executeDeletePost(post);
        }
        return;
      }

      Alert.alert(title, message, [
        { text: 'Hủy', style: 'cancel' },
        {
          text: isOwner ? 'Xóa' : 'Ẩn',
          style: 'destructive',
          onPress: () => executeDeletePost(post),
        },
      ]);
    },
    [executeDeletePost, user],
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
            onMorePress: (post: FeedPostData) => setOptionsPost(post),
            onDeletePress: handleDeletePress,
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
      handleDeletePress,
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
      <PostOptionsSheet
        post={optionsPost}
        isSaved={optionsPost ? isSaved(optionsPost.id) : false}
        isReporting={reportPostMutation.isPending}
        onClose={() => setOptionsPost(null)}
        onInterested={handleInterested}
        onNotInterested={handleNotInterested}
        onToggleSave={(post) => handleToggleSave(post.id)}
        onReport={handleReportPost}
        onCopyLink={handleCopyLink}
      />
      {toast}
    </View>
  );
}
