import { memo, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import {
  Image,
  Pressable,
  StyleSheet,
  Text,
  View,
  type ImageLoadEvent,
} from 'react-native';
import { Icon } from '@/shared/icons';
import { theme } from '@/shared/theme/tokens';
import { RemoteImage, Surface, resolveRemoteImageUri } from '@/shared/ui-kit';

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
    const resolvedUri = resolveRemoteImageUri(uri);
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

export interface PostMediaGridProps {
  images: string[];
  onImagePress?(index: number): void;
}

/** Hiển thị lưới ảnh/video bài viết: 1 ảnh, 2 ảnh chia đôi dọc, 3 ảnh, từ 4 ảnh chia lưới 2x2 (+n). */
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

  // 3 ảnh: ảnh đầu to hơn 2 ảnh sau (ảnh 1 chiếm nửa trái, nửa phải chia 2 ảnh xếp dọc)
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

  // Từ 4 ảnh trở lên: chia khung làm 4 phần (lưới 2x2: top-left, top-right, bottom-left, bottom-right)
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
  /** Bố cục ảnh: 'grid' (lưới chia ô như feed) hoặc 'stack' (xếp dọc từ trên xuống như detail). Mặc định 'grid'. */
  imageLayout?: 'grid' | 'stack';
}

function formatCount(value: number): string {
  return value.toLocaleString('en-US');
}

function DefaultAction({ action }: { action: PostCardAction }) {
  const name = action.active && action.activeIcon ? action.activeIcon : action.icon;
  const color = action.active && action.id === 'like' ? theme.colors.like : theme.colors.text;
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={action.label ?? action.id}
      accessibilityState={{ selected: action.active }}
      onPress={action.onPress}
      hitSlop={8}
      style={styles.action}
    >
      {/* Giảm kích thước action đi 2px theo yêu cầu (từ 26px xuống 24px) */}
      <Icon name={name} size={theme.iconSize.lg - 4} color={color} />
      {action.count !== undefined ? (
        <Text style={styles.count}>{formatCount(action.count)}</Text>
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
  imageLayout = 'grid',
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

  return (
    <Surface style={styles.card}>
      {/* 1. Header (Avatar, Tên, Thời gian, Nút tùy chọn): căn padding riêng 4-8px */}
      <View style={styles.head}>
        <RemoteImage uri={author.avatarUri} circle style={styles.avatar} />
        <View style={styles.meta}>
          <Text style={styles.name}>{author.name}</Text>
          <Text style={styles.time}>{timeLabel}</Text>
        </View>
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Tùy chọn"
          onPress={onMorePress}
          hitSlop={8}
        >
          <Icon name="more" size={theme.iconSize.md} />
        </Pressable>
      </View>

      {/* 2. Nội dung Text bài viết: căn padding riêng 4-8px, rút gọn >100 ký tự và hiện ...đọc thêm làm mờ */}
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

      {/* 3. Ảnh bài viết: dạng grid (trang chủ) hoặc stack xếp dọc từ trên xuống (chi tiết) */}
      {images.length > 0 ? (
        imageLayout === 'stack' ? (
          <View testID="post-media-stack" style={styles.mediaStack}>
            {images.map((uri, index) => (
              <Pressable
                key={`${uri}-${index}`}
                testID={`post-stack-image-${index}`}
                accessibilityRole="button"
                accessibilityLabel={`Xem ảnh ${index + 1}`}
                onPress={() => onPressImage?.(index)}
                disabled={!onPressImage}
                style={styles.stackImageWrapper}
              >
                <PostImage uri={uri} />
              </Pressable>
            ))}
          </View>
        ) : (
          <PostMediaGrid images={images} onImagePress={onPressImage} />
        )
      ) : null}

      {/* 4. Hàng nút Action (tim, comment, share, lưu): căn padding riêng 4-8px */}
      <View style={styles.actions}>
        <View style={styles.group}>{actions.filter((a) => a.align !== 'end').map(draw)}</View>
        <View style={styles.group}>{actions.filter((a) => a.align === 'end').map(draw)}</View>
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
    // Dòng gạch ngang ngăn cách giữa các card bài viết 2px
    borderBottomWidth: 2,
    borderBottomColor: theme.colors.border,
    paddingHorizontal: 0,
    paddingTop: theme.spacing.xs,
    paddingBottom: theme.spacing.sm - 3,
    gap: theme.spacing.xs,
    borderRadius: 0,
    shadowOpacity: 0,
    elevation: 0,
  },
  head: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.md,
    paddingHorizontal: theme.spacing.sm, // Căn lề riêng 8px
    paddingTop: theme.spacing.xs,
  },
  avatar: { width: 36, height: 36, borderRadius: 18, overflow: 'hidden' },
  meta: { flex: 1 },
  name: {
    fontSize: theme.font.size.md,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.text,
  },
  time: { fontSize: theme.font.size.xs, color: theme.colors.muted },
  content: {
    fontSize: theme.font.size.md,
    lineHeight: 22,
    color: theme.colors.text,
    paddingHorizontal: theme.spacing.sm, // Căn lề riêng 8px
    paddingVertical: 2,
  },
  contentWrapper: {
    width: '100%',
  },
  readMoreText: {
    fontSize: theme.font.size.sm,
    color: theme.colors.muted,
    opacity: 0.65,
    fontWeight: '400',
  },
  singleImagePressable: {
    width: '100%',
  },
  mediaStack: {
    width: '100%',
    gap: theme.spacing.xs - 2,
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
  },
  image: {
    width: '100%',
    height: '100%',
    borderRadius: 0,
  },
  // Grid 2 ảnh: chia dọc khung làm 2
  grid2Container: {
    width: '100%',
    aspectRatio: 1,
    flexDirection: 'row',
    gap: 2,
    backgroundColor: '#0a0a0a',
    overflow: 'hidden',
  },
  grid2Col: {
    flex: 1,
    height: '100%',
    overflow: 'hidden',
  },
  // Grid 3 ảnh: ảnh 1 to bên trái, 2 ảnh xếp dọc bên phải
  grid3Container: {
    width: '100%',
    aspectRatio: 1,
    flexDirection: 'row',
    gap: 2,
    backgroundColor: '#0a0a0a',
    overflow: 'hidden',
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
  // Grid từ 4 ảnh trở lên: lưới 2x2
  grid4Container: {
    width: '100%',
    aspectRatio: 1,
    flexDirection: 'column',
    gap: 2,
    backgroundColor: '#0a0a0a',
    overflow: 'hidden',
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
    paddingHorizontal: theme.spacing.sm + 3, // Căn lề riêng 8px
    paddingTop: theme.spacing.xs,
    paddingBottom: 1,
  },
  group: { flexDirection: 'row', alignItems: 'center', gap: theme.spacing.lg },
  action: { flexDirection: 'row', alignItems: 'center', gap: theme.spacing.xs + 4 },
  count: { fontSize: theme.font.size.sm, color: theme.colors.text },
});
