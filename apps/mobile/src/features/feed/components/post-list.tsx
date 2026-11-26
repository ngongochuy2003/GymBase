import { memo, useCallback, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
  Platform,
  Pressable,
  RefreshControl,
  StyleSheet,
  Text,
  View,
  type ListRenderItemInfo,
} from 'react-native';
import { useCollapsibleHeaderContext } from '@/shared/screen-config';
import { theme } from '@/shared/theme/tokens';
import { FilterChips, type FilterChipsProps } from '@/shared/ui-kit';
import type { FeedPostData, PostActionTemplate } from '../types';
import { PostCard, type PostCardAction, type PostCardProps } from './post-card';

export interface PostListProps {
  posts: FeedPostData[];
  actionTemplates: PostActionTemplate[];
  onActionPress?(postId: string, actionId: string): void;
  renderAction?: PostCardProps['renderAction'];
  filterChipsProps?: FilterChipsProps;
  onEndReached?(): void;
  isFetchingNextPage?: boolean;
  hasNextPage?: boolean;
  onRefresh?(): void;
  refreshing?: boolean;
  isLoading?: boolean;
  isError?: boolean;
  onRetry?(): void;
  onPostPress?(post: FeedPostData): void;
  onImagePress?(post: FeedPostData, imageIndex: number): void;
  onMorePress?(post: FeedPostData): void;
  onDeletePress?(post: FeedPostData): void;
}

interface ActionOverrideEntry {
  value: boolean;
  baseInitial: boolean;
}

interface PostListItemProps {
  post: FeedPostData;
  actionTemplates: PostActionTemplate[];
  overrides: Record<string, ActionOverrideEntry>;
  onToggleAction: (key: string, nextActive: boolean, baseInitial: boolean) => void;
  onActionPress?(postId: string, actionId: string): void;
  renderAction?: PostCardProps['renderAction'];
  onPostPress?(post: FeedPostData): void;
  onImagePress?(post: FeedPostData, imageIndex: number): void;
  onMorePress?(post: FeedPostData): void;
  onDeletePress?(post: FeedPostData): void;
}

const PostListItem = memo(function PostListItem({
  post,
  actionTemplates,
  overrides,
  onToggleAction,
  onActionPress,
  renderAction,
  onPostPress,
  onImagePress,
  onMorePress,
  onDeletePress,
}: PostListItemProps) {
  const actions: PostCardAction[] = actionTemplates.map((template) => {
    const key = `${post.id}:${template.id}`;
    const initial = post.initialActive?.[template.id] ?? false;
    const override = overrides[key];
    const hasValidOverride = override !== undefined && override.baseInitial === initial;
    const active = template.toggle
      ? hasValidOverride
        ? override.value
        : initial
      : false;
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
        if (template.toggle) {
          onToggleAction(key, !active, initial);
        }
        onActionPress?.(post.id, template.id);
      },
    };
  });

  const handlePressContent = useCallback(() => {
    onPostPress?.(post);
  }, [onPostPress, post]);

  const handlePressImage = useCallback(
    (index: number) => {
      if (onImagePress) {
        onImagePress(post, index);
      } else if (onPostPress) {
        onPostPress(post);
      }
    },
    [onImagePress, onPostPress, post],
  );

  const handleMore = useMemo(
    () => (onMorePress ? () => onMorePress(post) : undefined),
    [onMorePress, post],
  );

  const handleDelete = useMemo(
    () => (onDeletePress ? () => onDeletePress(post) : undefined),
    [onDeletePress, post],
  );

  return (
    <PostCard
      author={post.author}
      timeLabel={post.timeLabel}
      content={post.content}
      imageUri={post.imageUri}
      imageUris={post.imageUris}
      actions={actions}
      renderAction={renderAction}
      onPressContent={handlePressContent}
      onPressImage={handlePressImage}
      onMorePress={handleMore}
      onDeletePress={handleDelete}
    />
  );
});

/** Section `postList`: hiển thị danh sách bài viết dạng Virtualized FlatList tối ưu 60fps kèm phân trang lười (lazy loading). */
export function PostList({
  posts,
  actionTemplates,
  onActionPress,
  renderAction,
  filterChipsProps,
  onEndReached,
  isFetchingNextPage,
  hasNextPage,
  onRefresh,
  refreshing,
  isLoading,
  isError,
  onRetry,
  onPostPress,
  onImagePress,
  onMorePress,
  onDeletePress,
}: PostListProps) {
  const [overrides, setOverrides] = useState<Record<string, ActionOverrideEntry>>({});
  const { headerHeight, onScroll, isCollapsible } = useCollapsibleHeaderContext();

  const handleToggleAction = useCallback(
    (key: string, nextActive: boolean, baseInitial: boolean) => {
      setOverrides((prev) => ({ ...prev, [key]: { value: nextActive, baseInitial } }));
    },
    [],
  );

  const isFaded = Boolean(isLoading && posts.length > 0);

  const renderItem = useCallback(
    ({ item }: ListRenderItemInfo<FeedPostData>) => {
      return (
        <View style={isFaded && styles.fadedItem}>
          <PostListItem
            post={item}
            actionTemplates={actionTemplates}
            overrides={overrides}
            onToggleAction={handleToggleAction}
            onActionPress={onActionPress}
            renderAction={renderAction}
            onPostPress={onPostPress}
            onImagePress={onImagePress}
            onMorePress={onMorePress}
            onDeletePress={onDeletePress}
          />
        </View>
      );
    },
    [
      actionTemplates,
      handleToggleAction,
      isFaded,
      onActionPress,
      onDeletePress,
      onImagePress,
      onMorePress,
      onPostPress,
      overrides,
      renderAction,
    ],
  );

  const keyExtractor = useCallback((item: FeedPostData) => item.id, []);

  const ListHeader = useMemo(() => {
    if (!filterChipsProps) return undefined;
    return (
      <View style={styles.header}>
        <FilterChips {...filterChipsProps} />
      </View>
    );
  }, [filterChipsProps]);

  const ListEmpty = useMemo(() => {
    if (isLoading) {
      return (
        <View testID="feed-body-loading" style={styles.bodyCenter}>
          <ActivityIndicator size="large" color={theme.colors.primary} />
          <Text style={styles.loadingText}>Đang tải bài viết...</Text>
        </View>
      );
    }
    if (isError) {
      return (
        <View testID="feed-body-error" style={styles.bodyCenter}>
          <Text style={styles.errorTitle}>Không tải được bảng tin</Text>
          <Text style={styles.errorSubtitle}>Vui lòng kiểm tra kết nối mạng và thử lại.</Text>
          {onRetry ? (
            <Pressable
              accessibilityRole="button"
              accessibilityLabel="Thử lại"
              style={styles.retryButton}
              onPress={onRetry}
            >
              <Text style={styles.retryButtonText}>Thử lại</Text>
            </Pressable>
          ) : null}
        </View>
      );
    }
    return (
      <View testID="feed-body-empty" style={styles.bodyCenter}>
        <Text style={styles.emptyTitle}>Chưa có bài viết</Text>
        <Text style={styles.emptySubtitle}>
          Hãy là người đầu tiên chia sẻ tiến trình tập luyện trong chủ đề này.
        </Text>
      </View>
    );
  }, [isError, isLoading, onRetry]);

  const ListFooter = useMemo(() => {
    if (isFetchingNextPage) {
      return (
        <View testID="feed-loading-more" style={styles.footerLoading}>
          <ActivityIndicator size="small" color={theme.colors.primary} />
          <Text style={styles.footerText}>Đang tải thêm bài viết...</Text>
        </View>
      );
    }
    if (hasNextPage === false && posts.length > 0) {
      return (
        <View testID="feed-end-of-list" style={styles.footerEnd}>
          <View style={styles.divider} />
          <Text style={styles.endText}>Bạn đã xem hết bài viết mới nhất</Text>
        </View>
      );
    }
    return <View style={styles.footerSpacer} />;
  }, [hasNextPage, isFetchingNextPage, posts.length]);

  return (
    <View style={styles.wrapper}>
      <FlatList
        testID="screen-scroll-view"
        data={posts}
        keyExtractor={keyExtractor}
        renderItem={renderItem}
        ListHeaderComponent={ListHeader}
        ListEmptyComponent={ListEmpty}
        ListFooterComponent={ListFooter}
        onEndReached={onEndReached}
        onEndReachedThreshold={0.5}
        onScroll={isCollapsible ? onScroll : undefined}
        scrollEventThrottle={16}
        contentContainerStyle={[
          styles.content,
          isCollapsible && { paddingTop: headerHeight },
          posts.length === 0 && styles.emptyContent,
        ]}
        style={styles.list}
        refreshControl={
          onRefresh ? (
            <RefreshControl
              refreshing={Boolean(refreshing)}
              onRefresh={onRefresh}
              tintColor={theme.colors.primary}
              colors={[theme.colors.primary]}
            />
          ) : undefined
        }
        initialNumToRender={4}
        maxToRenderPerBatch={4}
        windowSize={5}
        removeClippedSubviews={Platform.OS === 'android'}
      />
      {isLoading && posts.length > 0 ? (
        <View
          testID="feed-body-loading-overlay"
          style={[styles.floatingLoading, { top: headerHeight + 56 }]}
          pointerEvents="none"
        >
          <View style={styles.floatingBubble}>
            <ActivityIndicator size="small" color={theme.colors.primary} />
            <Text style={styles.floatingText}>Đang cập nhật...</Text>
          </View>
        </View>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  wrapper: {
    flex: 1,
    position: 'relative',
    width: '100%',
  },
  list: {
    flex: 1,
    width: '100%',
  },
  content: {
    paddingHorizontal: 0,
    paddingBottom: theme.spacing.xl,
    width: '100%',
    maxWidth: 680,
    alignSelf: 'center',
  },
  emptyContent: {
    flexGrow: 1,
  },
  fadedItem: {
    opacity: 0.35,
  },
  bodyCenter: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: theme.spacing.xl * 2,
    paddingHorizontal: theme.spacing.lg,
    gap: theme.spacing.md,
    minHeight: 280,
  },
  loadingText: {
    fontSize: theme.font.size.sm,
    color: theme.colors.muted,
  },
  floatingLoading: {
    position: 'absolute',
    left: 0,
    right: 0,
    alignItems: 'center',
    justifyContent: 'center',
    zIndex: 20,
  },
  floatingBubble: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.sm,
    backgroundColor: theme.colors.card,
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.sm,
    borderRadius: theme.radius.chip,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  floatingText: {
    fontSize: theme.font.size.xs,
    color: theme.colors.text,
    fontWeight: theme.font.weight.medium,
  },
  emptyTitle: {
    fontSize: theme.font.size.lg,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.text,
  },
  emptySubtitle: {
    fontSize: theme.font.size.sm,
    color: theme.colors.muted,
    textAlign: 'center',
    maxWidth: 300,
  },
  errorTitle: {
    fontSize: theme.font.size.lg,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.text,
  },
  errorSubtitle: {
    fontSize: theme.font.size.sm,
    color: theme.colors.muted,
    textAlign: 'center',
    maxWidth: 300,
  },
  retryButton: {
    backgroundColor: theme.colors.primary,
    paddingHorizontal: theme.spacing.lg,
    paddingVertical: theme.spacing.sm,
    borderRadius: theme.radius.chip,
    marginTop: theme.spacing.xs,
  },
  retryButtonText: {
    color: theme.colors.onPrimary,
    fontWeight: theme.font.weight.bold,
    fontSize: theme.font.size.sm,
  },
  header: {
    width: '100%',
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.border,
  },
  footerLoading: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: theme.spacing.sm,
    paddingVertical: theme.spacing.md,
  },
  footerText: {
    fontSize: theme.font.size.sm,
    color: theme.colors.muted,
  },
  footerEnd: {
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: theme.spacing.lg,
    gap: theme.spacing.sm,
  },
  divider: {
    width: 60,
    height: 2,
    backgroundColor: theme.colors.border,
    borderRadius: 1,
  },
  endText: {
    fontSize: theme.font.size.xs,
    color: theme.colors.muted,
  },
  footerSpacer: {
    height: theme.spacing.md,
  },
});
