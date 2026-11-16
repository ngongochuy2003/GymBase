import { useCallback, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  PanResponder,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { useQuery, useQueryClient, type InfiniteData } from '@tanstack/react-query';
import { SafeAreaView } from 'react-native-safe-area-context';
import { theme } from '@/shared/theme/tokens';
import { SubHeader } from '@/shared/ui-kit';
import { feedRepository, type FeedPageResult } from '../api/feed-api';
import { postActionTemplates } from '../config/feed-config';
import { FEED_QUERY_KEY, useToggleLikeMutation } from '../hooks/use-feed-posts';
import type { FeedPostData } from '../types';
import { PostCard, type PostCardAction } from '../components/post-card';
import { PostImageViewer } from '../components/post-image-viewer';

export interface PostDetailScreenProps {
  initialPost?: FeedPostData;
  postId?: string;
  onBack?: () => void;
}

/**
 * Màn hình xem chi tiết bài viết:
 * - Tuân thủ chính xác token design (fontsize, color, padding) như màn trang chủ.
 * - Nội dung hiển thị đầy đủ (không cắt ngắn 100 ký tự).
 * - Danh sách ảnh xếp dọc lần lượt từ trên xuống cho người dùng cuộn xem giống trang chủ.
 * - Vuốt từ trái sang phải sẽ quay về màn hình trước (trang chủ).
 * - Nhấn vào từng ảnh sẽ mở trình xem ảnh chi tiết toàn màn hình (PostImageViewer).
 */
export function PostDetailScreen({
  initialPost: propInitialPost,
  postId: propPostId,
  onBack: propOnBack,
}: PostDetailScreenProps = {}) {
  const router = useRouter();
  const searchParams = useLocalSearchParams<{ postId?: string; postJson?: string }>();
  const queryClient = useQueryClient();

  const targetPostId = propPostId ?? searchParams.postId;

  // 1. Phục hồi post từ param postJson nếu có
  const parsedPostFromJson = useMemo<FeedPostData | null>(() => {
    if (searchParams.postJson) {
      try {
        return JSON.parse(searchParams.postJson) as FeedPostData;
      } catch {
        return null;
      }
    }
    return null;
  }, [searchParams.postJson]);

  // 2. Tra cứu post từ TanStack Query cache của bảng tin (Feed) để hiển thị tức thì (0ms)
  const cachedPost = useMemo<FeedPostData | null>(() => {
    if (propInitialPost) return propInitialPost;
    if (parsedPostFromJson) return parsedPostFromJson;
    if (!targetPostId) return null;

    const allQueries = queryClient.getQueriesData<InfiniteData<FeedPageResult>>({
      queryKey: FEED_QUERY_KEY,
    });
    for (const [, queryData] of allQueries) {
      if (!queryData?.pages) continue;
      for (const page of queryData.pages) {
        const found = page.posts.find((p) => p.id === targetPostId);
        if (found) return found;
      }
    }
    return null;
  }, [propInitialPost, parsedPostFromJson, targetPostId, queryClient]);

  // 3. Tự động đồng bộ với backend nếu cần
  const { data: fetchedPost, isLoading } = useQuery({
    queryKey: ['post-detail', targetPostId],
    queryFn: () => (targetPostId ? feedRepository.getPostById(targetPostId) : null),
    enabled: Boolean(targetPostId),
    initialData: cachedPost ?? undefined,
  });

  const post = fetchedPost ?? cachedPost ?? propInitialPost ?? parsedPostFromJson;

  // State quản lý tương tác like
  const [isLikedOverride, setIsLikedOverride] = useState<boolean | undefined>(undefined);
  const { mutate: toggleLike } = useToggleLikeMutation();

  // State quản lý trình xem chi tiết ảnh
  const [isViewerVisible, setIsViewerVisible] = useState(false);
  const [selectedImageIndex, setSelectedImageIndex] = useState(0);

  const handleBack = useCallback(() => {
    if (propOnBack) {
      propOnBack();
      return;
    }
    if (router.canGoBack()) {
      router.back();
    } else {
      router.replace('/(protected)/home');
    }
  }, [propOnBack, router]);

  // Nhận diện cử chỉ vuốt từ trái sang phải (swipe right) để quay về màn hình trước
  const panResponder = useMemo(
    () =>
      PanResponder.create({
        onMoveShouldSetPanResponder: (_evt, gestureState) => {
          // Vuốt sang phải dx > 20 và góc vuốt ngang chiếm ưu thế
          return (
            gestureState.dx > 20 &&
            gestureState.dx > Math.abs(gestureState.dy) * 1.3
          );
        },
        onPanResponderRelease: (_evt, gestureState) => {
          if (gestureState.dx > 60 || gestureState.vx > 0.3) {
            handleBack();
          }
        },
      }),
    [handleBack],
  );

  const images = useMemo(() => {
    if (!post) return [];
    if (post.imageUris && post.imageUris.length > 0) {
      return post.imageUris.filter(Boolean);
    }
    if (post.imageUri) {
      return [post.imageUri];
    }
    return [];
  }, [post]);

  const handleImagePress = useCallback((index: number) => {
    setSelectedImageIndex(index);
    setIsViewerVisible(true);
  }, []);

  const handleToggleLike = useCallback(() => {
    if (!post) return;
    const initialLike = post.initialActive?.like ?? false;
    const currentLike = isLikedOverride !== undefined ? isLikedOverride : initialLike;
    setIsLikedOverride(!currentLike);
    toggleLike(post.id);
  }, [isLikedOverride, post, toggleLike]);

  const actions = useMemo<PostCardAction[]>(() => {
    if (!post) return [];
    return postActionTemplates.map((template) => {
      const initial = post.initialActive?.[template.id] ?? false;
      const isLikeAction = template.id === 'like';
      const active = isLikeAction
        ? isLikedOverride !== undefined
          ? isLikedOverride
          : initial
        : initial;

      const base = post.counts?.[template.id];
      const count =
        template.showCount && base !== undefined
          ? base + (active === initial ? 0 : active ? 1 : -1)
          : undefined;

      return {
        id: template.id,
        icon: template.icon,
        activeIcon: template.activeIcon,
        align: template.align,
        active,
        count,
        onPress: () => {
          if (isLikeAction) {
            handleToggleLike();
          }
        },
      };
    });
  }, [handleToggleLike, isLikedOverride, post]);

  return (
    <SafeAreaView style={styles.root} edges={['top', 'left', 'right', 'bottom']}>
      <View
        testID="post-detail-screen-container"
        style={styles.container}
        {...panResponder.panHandlers}
      >
        {/* Header với nút quay lại và tiêu đề giữ nguyên chuẩn giao diện */}
        <SubHeader
          title="Bài viết"
          onBack={handleBack}
          style={styles.subHeader}
        />

        {post ? (
          <ScrollView
            testID="post-detail-scrollview"
            style={styles.scrollArea}
            contentContainerStyle={styles.scrollContent}
            showsVerticalScrollIndicator={false}
          >
            <PostCard
              author={post.author}
              timeLabel={post.timeLabel}
              content={post.content}
              imageUris={images}
              actions={actions}
              truncateContent={false} // Hiển thị toàn bộ nội dung, không cắt 100 ký tự
              imageLayout="stack" // Xếp dọc lần lượt từng ảnh từ trên xuống
              onPressImage={handleImagePress}
            />
          </ScrollView>
        ) : isLoading ? (
          <View testID="post-detail-loading" style={styles.centered}>
            <ActivityIndicator size="large" color={theme.colors.primary} />
            <Text style={styles.loadingText}>Đang tải bài viết...</Text>
          </View>
        ) : (
          <View testID="post-detail-not-found" style={styles.centered}>
            <Text style={styles.notFoundText}>Không tìm thấy bài viết này.</Text>
          </View>
        )}

        {/* Modal xem chi tiết ảnh toàn màn hình có zoom và swipe */}
        <PostImageViewer
          visible={isViewerVisible}
          images={images}
          initialIndex={selectedImageIndex}
          onClose={() => setIsViewerVisible(false)}
        />
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  root: {
    flex: 1,
    backgroundColor: theme.colors.background,
  },
  container: {
    flex: 1,
  },
  subHeader: {
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.border,
    backgroundColor: theme.colors.card,
  },
  scrollArea: {
    flex: 1,
  },
  scrollContent: {
    paddingBottom: 0,
  },
  centered: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    gap: theme.spacing.sm,
  },
  loadingText: {
    color: theme.colors.muted,
    fontSize: theme.font.size.sm,
  },
  notFoundText: {
    color: theme.colors.muted,
    fontSize: theme.font.size.md,
  },
});
