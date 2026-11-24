import { useState } from 'react';
import { Image as ExpoImage } from 'expo-image';
import {
  Image,
  StyleSheet,
  View,
  type ImageLoadEvent,
  type ImageResizeMode,
  type ImageStyle,
  type StyleProp,
} from 'react-native';
import { resolveMediaUrl } from '@/core/config/api-origin';
import { layThongTinAnh, type ImageMeta } from '../file/image-cache';
import { theme } from '../theme/tokens';

export interface RemoteImageProps {
  uri?: string;
  accessibilityLabel?: string;
  style?: StyleProp<ImageStyle>;
  /** Mặc định bo góc ảnh theo token; avatar truyền `circle`. */
  circle?: boolean;
  resizeMode?: ImageResizeMode;
  onLoad?(event: ImageLoadEvent): void;
  /** Thông tin đi kèm ảnh (file_id, blurhash). Bỏ trống thì tra theo URL trong `ghiNhoThongTinAnh`. */
  media?: ImageMeta;
}

/** `resizeMode` của React Native tương ứng `contentFit` của expo-image. */
const CONTENT_FIT = {
  cover: 'cover',
  contain: 'contain',
  stretch: 'fill',
  center: 'none',
  none: 'none',
  repeat: 'cover',
} as const;

/**
 * Ảnh từ URL; lỗi tải hoặc không có URI thì hiện nền xám thay vì crash/trống.
 * Ảnh có `file_id` hoặc blurhash (tệp do người dùng tải lên) đi qua expo-image: cache theo `file_id` và hiện bản mờ khi đang tải.
 * Ảnh còn lại dùng `Image` của React Native như cũ.
 */
export function RemoteImage({
  uri,
  accessibilityLabel,
  style,
  circle,
  resizeMode = 'cover',
  onLoad,
  media,
}: RemoteImageProps) {
  const [failed, setFailed] = useState(false);
  const resolvedUri = resolveMediaUrl(uri);
  const radius = circle ? theme.radius.chip : theme.radius.image;

  if (!resolvedUri || failed) {
    return (
      <View
        testID="image-fallback"
        accessibilityLabel={accessibilityLabel}
        style={[
          styles.fallback,
          { borderRadius: radius },
          style as object,
          circle && styles.circle,
        ]}
      />
    );
  }

  const meta = media ?? layThongTinAnh(resolvedUri);
  if (meta?.fileId || meta?.blurhash) {
    return (
      <ExpoImage
        accessibilityLabel={accessibilityLabel}
        source={{ uri: resolvedUri, cacheKey: meta.fileId }}
        placeholder={meta.blurhash ? { blurhash: meta.blurhash } : undefined}
        placeholderContentFit={CONTENT_FIT[resizeMode]}
        contentFit={CONTENT_FIT[resizeMode]}
        transition={200}
        onError={() => setFailed(true)}
        // Người gọi đang nhận sự kiện kiểu React Native (`nativeEvent.source`), nên đổi lại cho khớp.
        onLoad={(event) =>
          onLoad?.({
            nativeEvent: { source: { width: event.source.width, height: event.source.height, uri: resolvedUri } },
          } as ImageLoadEvent)
        }
        style={[
          styles.image,
          { borderRadius: radius },
          style as object,
          circle && styles.circle,
        ]}
      />
    );
  }

  return (
    <Image
      accessibilityLabel={accessibilityLabel}
      source={{ uri: resolvedUri }}
      resizeMode={resizeMode}
      onError={() => setFailed(true)}
      onLoad={onLoad}
      style={[
        styles.image,
        { borderRadius: radius },
        style as object,
        circle && styles.circle,
      ]}
    />
  );
}

const styles = StyleSheet.create({
  image: { backgroundColor: theme.colors.placeholder, overflow: 'hidden' },
  fallback: { backgroundColor: theme.colors.placeholder, overflow: 'hidden' },
  circle: { borderRadius: theme.radius.chip },
});
