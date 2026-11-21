import { memo, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import {
  Image,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  useWindowDimensions,
  View,
  type ImageLoadEvent,
} from 'react-native';
import { Icon } from '@/shared/icons';
import { isVideoUrl } from '@/shared/file';
import { theme } from '@/shared/theme/tokens';
import { resolveMediaUrl } from '@/core/config/api-origin';
import { RemoteImage, Surface } from '@/shared/ui-kit';
import { PostVideoPlayer } from './post-video-player';

export const MIN_POST_ASPECT_RATIO = 4 / 5; // Tối đa độ dọc (0.8) để ảnh không choán hết màn hình
export const MAX_POST_ASPECT_RATIO = 16 / 9; // Tối đa độ ngang (1.777) để ảnh không quá dẹt
export const DEFAULT_POST_ASPECT_RATIO = 4 / 3; // Tỷ lệ mặc định khi chưa load

export interface PostImageProps {
  uri: string;
}

/** Khung hiển thị ảnh bài viết tự thích ứng tỷ lệ, thu nhỏ vừa vặn không phóng to cắt xén. */
export function PostImage({ uri }: PostImageProps) {
  const [aspectRatio, setAspectRatio] = useState<number | undefined>(undefined);

  const applyDimensions = useCallback((width: number, height: number) => {
    if (width > 0 && height > 0) {
      const rawRatio = width / height;
      const clampedRatio = Math.min(
        MAX_POST_ASPECT_RATIO,
        Math.max(MIN_POST_ASPECT_RATIO, rawRatio),
      );
      setAspectRatio(clampedRatio);
    }
  }, []);

  useEffect(() => {
    let active = true;
    const resolvedUri = resolveMediaUrl(uri);
    if (!resolvedUri) return;

    try {
      if (typeof Image.getSize === 'function') {
        Image.getSize(
          resolvedUri,
          (width, height) => {
            if (active) {
              applyDimensions(width, height);
            }
          },
          () => {
            // Giữ tỷ lệ mặc định nếu không đo được kích thước trước
          },
        );
      }
    } catch {
      // Bỏ qua lỗi trong môi trường Jest / không có NativeModules
    }

    return () => {
      active = false;
    };
  }, [applyDimensions, uri]);

  const handleLoad = useCallback(
    (e: ImageLoadEvent) => {
      const source = e.nativeEvent?.source;
      const width = source?.width;
      const height = source?.height;

      if (width && height) {
        applyDimensions(width, height);
      }
    },
    [applyDimensions],
  );

  return (
    <View
      testID="post-image-container"
      style={[
        styles.imageContainer,
        { aspectRatio: aspectRatio ?? DEFAULT_POST_ASPECT_RATIO },
      ]}
    >
      <RemoteImage
        uri={uri}
        accessibilityLabel="Ảnh bài viết"
        style={styles.image}
        resizeMode="contain"
        onLoad={handleLoad}
      />
    </View>
  );
}

export interface PostMediaCarouselProps {
  images: string[];
  onImagePress?(index: number): void;
}

/**
 * Hiển thị ảnh bài viết dạng Carousel cuộn ngang theo thiết kế mới:
 * - 1 ảnh: Chiếm trọn khung bài viết với tỷ lệ dọc 4:5 và bo góc tròn mềm mại.
 * - Từ 2 ảnh trở lên: Cho phép người dùng vuốt ngang lướt xem các ảnh tiếp theo,
 *   ảnh kế tiếp nhô sang một phần để trực quan nhận biết được.
 */
export function PostMediaCarousel({ images, onImagePress }: PostMediaCarouselProps) {
  const { width: windowWidth } = useWindowDimensions();
  if (!images || images.length === 0) return null;

  // 1 media: Chiếm 100% chiều ngang cột nội dung
  if (images.length === 1) {
    const singleUri = images[0]!;
    if (isVideoUrl(singleUri)) {
      return (
        <View testID="post-video-container-0" style={styles.singleCarouselWrapper}>
          <PostVideoPlayer
            uri={singleUri}
            onPress={() => onImagePress?.(0)}
            accessibilityLabel="Video bài viết"
          />
        </View>
      );
    }
    return (
      <Pressable
        testID="post-image-pressable-0"
        accessibilityRole="button"
        accessibilityLabel="Xem ảnh bài viết"
        onPress={() => onImagePress?.(0)}
        disabled={!onImagePress}
        style={styles.singleCarouselWrapper}
      >
        <PostImage uri={singleUri} />
      </Pressable>
    );
  }

  // Từ 2 media trở lên: Vuốt ngang (horizontal ScrollView)
  // Mỗi media rộng khoảng 76% chiều rộng cột để media sau lấp ló bên phải
  const safeWidth = windowWidth > 0 ? windowWidth : 375;
  const itemWidth = Math.max(210, Math.min(290, Math.round((safeWidth - 72) * 0.76)));

  return (
    <ScrollView
      testID="post-media-carousel"
      horizontal
      showsHorizontalScrollIndicator={false}
      nestedScrollEnabled
      contentContainerStyle={styles.carouselScrollContent}
    >
      {images.map((uri, index) => {
        const isVideo = isVideoUrl(uri);
        if (isVideo) {
          return (
            <View
              key={`${uri}-${index}`}
              testID={`post-video-container-${index}`}
              style={[styles.carouselItemWrapper, { width: itemWidth }]}
            >
              <PostVideoPlayer
                uri={uri}
                onPress={() => onImagePress?.(index)}
                accessibilityLabel={`Video bài viết ${index + 1}`}
              />
            </View>
          );
        }
        return (
          <Pressable
            key={`${uri}-${index}`}
            testID={`post-image-pressable-${index}`}
            accessibilityRole="button"
            accessibilityLabel={`Xem ảnh bài viết ${index + 1}`}
            onPress={() => onImagePress?.(index)}
            disabled={!onImagePress}
            style={[styles.carouselItemWrapper, { width: itemWidth }]}
          >
            <RemoteImage
              uri={uri}
              accessibilityLabel={`Ảnh bài viết ${index + 1}`}
              style={styles.carouselImage}
              resizeMode="cover"
            />
          </Pressable>
        );
      })}
    </ScrollView>
  );
}

export interface PostMediaGridProps {
  images: string[];
  onImagePress?(index: number): void;
}

/** Hiển thị lưới ảnh/video bài viết (chế độ grid). */
export function PostMediaGrid({ images, onImagePress }: PostMediaGridProps) {
  if (!images || images.length === 0) return null;

  // 1 ảnh: dùng PostImage tự điều chỉnh tỷ lệ
  if (images.length === 1) {
    return (
      <Pressable
        testID="post-image-pressable-0"
        onPress={() => onImagePress?.(0)}
        disabled={!onImagePress}
        style={styles.singleImagePressable}
      >
        <PostImage uri={images[0]!} />
      </Pressable>
    );
  }

  // 2 ảnh: chia dọc khung làm 2, mỗi ảnh chiếm 50% chiều ngang
  if (images.length === 2) {
    return (
      <View testID="post-media-grid-2" style={styles.grid2Container}>
        <Pressable
          testID="post-image-pressable-0"
          onPress={() => onImagePress?.(0)}
          disabled={!onImagePress}
          style={styles.grid2Col}
        >
          <RemoteImage uri={images[0]} style={styles.gridImage} resizeMode="cover" />
        </Pressable>
        <Pressable
          testID="post-image-pressable-1"
          onPress={() => onImagePress?.(1)}
          disabled={!onImagePress}
          style={styles.grid2Col}
        >
          <RemoteImage uri={images[1]} style={styles.gridImage} resizeMode="cover" />
        </Pressable>
      </View>
    );
  }

  // 3 ảnh: ảnh đầu to hơn 2 ảnh sau
  if (images.length === 3) {
    return (
      <View testID="post-media-grid-3" style={styles.grid3Container}>
        <Pressable
          testID="post-image-pressable-0"
          onPress={() => onImagePress?.(0)}
          disabled={!onImagePress}
          style={styles.grid3LeftCol}
        >
          <RemoteImage uri={images[0]} style={styles.gridImage} resizeMode="cover" />
        </Pressable>
        <View style={styles.grid3RightCol}>
          <Pressable
            testID="post-image-pressable-1"
            onPress={() => onImagePress?.(1)}
            disabled={!onImagePress}
            style={styles.grid3RightCell}
          >
            <RemoteImage uri={images[1]} style={styles.gridImage} resizeMode="cover" />
          </Pressable>
          <Pressable
            testID="post-image-pressable-2"
            onPress={() => onImagePress?.(2)}
            disabled={!onImagePress}
            style={styles.grid3RightCell}
          >
            <RemoteImage uri={images[2]} style={styles.gridImage} resizeMode="cover" />
          </Pressable>
        </View>
      </View>
    );
  }

  // Từ 4 ảnh trở lên: chia khung làm 4 phần (lưới 2x2)
  const remainingCount = images.length - 4;
  return (
    <View testID="post-media-grid-4" style={styles.grid4Container}>
      <View style={styles.grid4Row}>
        <Pressable
          testID="post-image-pressable-0"
          onPress={() => onImagePress?.(0)}
          disabled={!onImagePress}
          style={styles.grid4Cell}
        >
          <RemoteImage uri={images[0]} style={styles.gridImage} resizeMode="cover" />
        </Pressable>
        <Pressable
          testID="post-image-pressable-1"
          onPress={() => onImagePress?.(1)}
          disabled={!onImagePress}
          style={styles.grid4Cell}
        >
          <RemoteImage uri={images[1]} style={styles.gridImage} resizeMode="cover" />
        </Pressable>
      </View>
      <View style={styles.grid4Row}>
        <Pressable
          testID="post-image-pressable-2"
          onPress={() => onImagePress?.(2)}
          disabled={!onImagePress}
          style={styles.grid4Cell}
        >
          <RemoteImage uri={images[2]} style={styles.gridImage} resizeMode="cover" />
        </Pressable>
        <Pressable
          testID="post-image-pressable-3"
          onPress={() => onImagePress?.(3)}
          disabled={!onImagePress}
          style={styles.grid4Cell}
        >
          <RemoteImage uri={images[3]} style={styles.gridImage} resizeMode="cover" />
          {remainingCount > 0 ? (
            <View testID="post-media-remaining-overlay" style={styles.remainingOverlay}>
              <Text style={styles.remainingText}>{`+${remainingCount}`}</Text>
            </View>
          ) : null}
        </Pressable>
      </View>
    </View>
  );
}

export interface PostCardAction {
  id: string;
  icon: string;
  activeIcon?: string;
  count?: number;
  active?: boolean;
  align?: 'start' | 'end';
  label?: string;
  onPress(): void;
}

export interface PostCardProps {
  author: { name: string; avatarUri?: string };
  timeLabel: string;
  content: string;
  imageUri?: string;
  imageUris?: string[];
  actions: PostCardAction[];
  onMorePress?(): void;
  /** Ghi đè cách vẽ từng action. */
  renderAction?(action: PostCardAction): ReactNode;
  /** Có thu gọn nội dung dài hơn 100 ký tự hay không (mặc định: true). */
  truncateContent?: boolean;
  /** Giới hạn ký tự thu gọn (mặc định: 100). */
  maxContentLength?: number;
  /** Bấm vào nội dung bài viết hoặc chữ đọc thêm */
  onPressContent?(): void;
  /** Bấm vào ảnh bài viết */
  onPressImage?(index: number): void;
  /** Bố cục ảnh: 'carousel' (vuốt ngang theo mockup), 'grid' (lưới chia ô), 'stack' (xếp dọc). Mặc định 'carousel'. */
  imageLayout?: 'carousel' | 'grid' | 'stack';
  /** Có hiển thị badge dấu cộng trên avatar không (mặc định: true). */
  showAvatarBadge?: boolean;
  onAvatarBadgePress?(): void;
}

function formatCount(value: number): string {
  return value.toLocaleString('en-US');
}

function DefaultAction({ action }: { action: PostCardAction }) {
  const isLike = action.id === 'like';
  const isSave = action.id === 'save';
  const name = action.active && action.activeIcon ? action.activeIcon : action.icon;
  const color = action.active
    ? isLike
      ? theme.colors.like
      : isSave
      ? theme.colors.primary
      : theme.colors.text
    : theme.colors.text;
  return (
    <Pressable
      testID={`post-action-${action.id}`}
      accessibilityRole="button"
      accessibilityLabel={action.label ?? action.id}
      accessibilityState={{ selected: action.active }}
      onPress={action.onPress}
      hitSlop={8}
      style={styles.action}
    >
      <Icon name={name} size={theme.iconSize.sm + 2} color={color} />
      {action.count !== undefined ? (
        <Text
          style={[
            styles.count,
            action.active && isLike ? { color: theme.colors.like } : undefined,
          ]}
        >
          {formatCount(action.count)}
        </Text>
      ) : null}
    </Pressable>
  );
}

export const PostCard = memo(function PostCard({
  author,
  timeLabel,
  content,
  imageUri,
  imageUris,
  actions,
  onMorePress,
  renderAction,
  truncateContent = true,
  maxContentLength = 100,
  onPressContent,
  onPressImage,
  imageLayout = 'carousel',
  showAvatarBadge = true,
  onAvatarBadgePress,
}: PostCardProps) {
  const images = useMemo(() => {
    if (imageUris && imageUris.length > 0) {
      return imageUris.filter(Boolean);
    }
    if (imageUri) {
      return [imageUri];
    }
    return [];
  }, [imageUri, imageUris]);

  const draw = (action: PostCardAction) => (
    <View key={action.id}>
      {renderAction ? renderAction(action) : <DefaultAction action={action} />}
    </View>
  );

  const shouldTruncate = Boolean(truncateContent && content && content.length > maxContentLength);
  const displayContent = shouldTruncate
    ? content.slice(0, maxContentLength).trimEnd()
    : content;

  const handleMediaPress = useCallback(
    (index: number) => {
      if (onPressImage) {
        onPressImage(index);
      } else if (onPressContent) {
        onPressContent();
      }
    },
    [onPressContent, onPressImage],
  );

  return (
    <Surface style={styles.card}>
      <View style={styles.cardRow}>
        {/* Cột trái: Avatar tròn + Badge dấu cộng + */}
        <View style={styles.avatarColumn}>
          <View style={styles.avatarWrapper}>
            <RemoteImage uri={author.avatarUri} circle style={styles.avatar} />
            {showAvatarBadge ? (
              <Pressable
                testID="avatar-plus-badge"
                accessibilityRole="button"
                accessibilityLabel="Theo dõi tác giả"
                onPress={onAvatarBadgePress}
                hitSlop={4}
                style={styles.avatarBadge}
              >
                <Icon name="plus" size={9} color="#FFFFFF" />
              </Pressable>
            ) : null}
          </View>
        </View>

        {/* Cột phải: Toàn bộ nội dung bài viết theo thiết kế */}
        <View style={styles.contentColumn}>
          {/* 1. Header: Tên tác giả + Thời gian đăng + Nút tùy chọn (...) */}
          <View style={styles.authorRow}>
            <View style={styles.authorMeta}>
              <Text style={styles.name}>{author.name}</Text>
              <Text style={styles.time}>{timeLabel}</Text>
            </View>
            <Pressable
              accessibilityRole="button"
              accessibilityLabel="Tùy chọn"
              onPress={onMorePress}
              hitSlop={8}
              style={styles.moreButton}
            >
              <Icon name="more" size={theme.iconSize.sm + 2} color={theme.colors.muted} />
            </Pressable>
          </View>

          {/* 2. Nội dung Text: Nhấn vào văn bản chuyển sang xem chi tiết */}
          {content ? (
            <Pressable
              testID="post-content-pressable"
              accessibilityRole="button"
              accessibilityLabel="Nội dung bài viết"
              onPress={onPressContent}
              disabled={!onPressContent}
              style={styles.contentWrapper}
            >
              <Text style={styles.content}>
                {displayContent}
                {shouldTruncate ? (
                  <Text testID="post-read-more" style={styles.readMoreText}>
                    ...đọc thêm
                  </Text>
                ) : null}
              </Text>
            </Pressable>
          ) : null}

          {/* 3. Ảnh bài viết: Carousel vuốt ngang / Grid / Stack */}
          {images.length > 0 ? (
            <View style={styles.mediaContainer}>
              {imageLayout === 'stack' ? (
                <View testID="post-media-stack" style={styles.mediaStack}>
                  {images.map((uri, index) => {
                    const isVideo = isVideoUrl(uri);
                    if (isVideo) {
                      return (
                        <View
                          key={`${uri}-${index}`}
                          testID={`post-stack-video-${index}`}
                          style={styles.stackImageWrapper}
                        >
                          <PostVideoPlayer
                            uri={uri}
                            onPress={() => handleMediaPress(index)}
                            accessibilityLabel={`Video ${index + 1}`}
                          />
                        </View>
                      );
                    }
                    return (
                      <Pressable
                        key={`${uri}-${index}`}
                        testID={`post-stack-image-${index}`}
                        accessibilityRole="button"
                        accessibilityLabel={`Xem ảnh ${index + 1}`}
                        onPress={() => handleMediaPress(index)}
                        style={styles.stackImageWrapper}
                      >
                        <PostImage uri={uri} />
                      </Pressable>
                    );
                  })}
                </View>
              ) : imageLayout === 'grid' ? (
                <PostMediaGrid images={images} onImagePress={handleMediaPress} />
              ) : (
                <PostMediaCarousel images={images} onImagePress={handleMediaPress} />
              )}
            </View>
          ) : null}

          {/* 4. Hàng nút Action: Like, Comment, Share, Repost, Save */}
          <View style={styles.actions}>
            <View style={styles.group}>{actions.filter((a) => a.align !== 'end').map(draw)}</View>
            <View style={styles.group}>{actions.filter((a) => a.align === 'end').map(draw)}</View>
          </View>
        </View>
      </View>
    </Surface>
  );
});

const styles = StyleSheet.create({
  card: {
    backgroundColor: theme.colors.card,
    borderTopWidth: 0,
    borderLeftWidth: 0,
    borderRightWidth: 0,
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.border,
    paddingHorizontal: 16,
    paddingTop: 14,
    paddingBottom: 12,
    borderRadius: 0,
    shadowOpacity: 0,
    elevation: 0,
  },
  cardRow: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    width: '100%',
  },
  avatarColumn: {
    width: 40,
    alignItems: 'center',
    paddingTop: 2,
  },
  avatarWrapper: {
    position: 'relative',
    width: 40,
    height: 40,
  },
  avatar: {
    width: 40,
    height: 40,
    borderRadius: 20,
    overflow: 'hidden',
  },
  avatarBadge: {
    position: 'absolute',
    bottom: -1,
    right: -1,
    width: 16,
    height: 16,
    borderRadius: 8,
    backgroundColor: '#1E1E1E',
    borderWidth: 1.5,
    borderColor: '#FFFFFF',
    justifyContent: 'center',
    alignItems: 'center',
  },
  contentColumn: {
    flex: 1,
    marginLeft: 12,
  },
  authorRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    minHeight: 24,
  },
  authorMeta: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    flex: 1,
  },
  name: {
    fontSize: 15,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.text,
  },
  time: {
    fontSize: 13,
    color: theme.colors.muted,
  },
  moreButton: {
    padding: 2,
    marginLeft: 'auto',
  },
  contentWrapper: {
    width: '100%',
    marginTop: 4,
    marginBottom: 6,
  },
  content: {
    fontSize: 15,
    lineHeight: 21,
    color: theme.colors.text,
  },
  readMoreText: {
    fontSize: 14,
    color: theme.colors.muted,
    opacity: 0.7,
    fontWeight: '400',
  },
  mediaContainer: {
    width: '100%',
    marginTop: 4,
    marginBottom: 8,
  },
  // Carousel cuộn ngang tỷ lệ 4:5 bo góc tròn 16px
  carouselScrollContent: {
    gap: 10,
    paddingRight: 16,
  },
  singleCarouselWrapper: {
    width: '100%',
  },
  singleCarouselImageContainer: {
    width: '100%',
    aspectRatio: 4 / 5,
    borderRadius: 16,
    overflow: 'hidden',
    backgroundColor: theme.colors.surfaceMuted,
  },
  carouselItemWrapper: {
    aspectRatio: 4 / 5,
    borderRadius: 16,
    overflow: 'hidden',
    backgroundColor: theme.colors.surfaceMuted,
  },
  carouselImage: {
    width: '100%',
    height: '100%',
    borderRadius: 16,
  },
  // Grid truyền thống
  singleImagePressable: {
    width: '100%',
  },
  mediaStack: {
    width: '100%',
    gap: theme.spacing.xs,
  },
  stackImageWrapper: {
    width: '100%',
  },
  imageContainer: {
    width: '100%',
    backgroundColor: '#0a0a0a',
    justifyContent: 'center',
    alignItems: 'center',
    overflow: 'hidden',
    borderRadius: 16,
  },
  image: {
    width: '100%',
    height: '100%',
    borderRadius: 16,
  },
  grid2Container: {
    width: '100%',
    aspectRatio: 1,
    flexDirection: 'row',
    gap: 2,
    backgroundColor: '#0a0a0a',
    overflow: 'hidden',
    borderRadius: 16,
  },
  grid2Col: {
    flex: 1,
    height: '100%',
    overflow: 'hidden',
  },
  grid3Container: {
    width: '100%',
    aspectRatio: 1,
    flexDirection: 'row',
    gap: 2,
    backgroundColor: '#0a0a0a',
    overflow: 'hidden',
    borderRadius: 16,
  },
  grid3LeftCol: {
    flex: 1,
    height: '100%',
    overflow: 'hidden',
  },
  grid3RightCol: {
    flex: 1,
    height: '100%',
    flexDirection: 'column',
    gap: 2,
    overflow: 'hidden',
  },
  grid3RightCell: {
    flex: 1,
    width: '100%',
    overflow: 'hidden',
  },
  grid4Container: {
    width: '100%',
    aspectRatio: 1,
    flexDirection: 'column',
    gap: 2,
    backgroundColor: '#0a0a0a',
    overflow: 'hidden',
    borderRadius: 16,
  },
  grid4Row: {
    flex: 1,
    flexDirection: 'row',
    gap: 2,
    overflow: 'hidden',
  },
  grid4Cell: {
    flex: 1,
    height: '100%',
    position: 'relative',
    overflow: 'hidden',
  },
  gridImage: {
    width: '100%',
    height: '100%',
    borderRadius: 0,
  },
  remainingOverlay: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: 'rgba(0, 0, 0, 0.55)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  remainingText: {
    color: '#FFFFFF',
    fontSize: 28,
    fontWeight: 'bold',
    textAlign: 'center',
  },
  actions: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingTop: 4,
    paddingBottom: 2,
  },
  group: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 20,
  },
  action: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
  },
  count: {
    fontSize: 13,
    color: theme.colors.text,
  },
});
