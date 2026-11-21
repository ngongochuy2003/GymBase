import { useState } from 'react';
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
import { theme } from '../theme/tokens';

export interface RemoteImageProps {
  uri?: string;
  accessibilityLabel?: string;
  style?: StyleProp<ImageStyle>;
  /** Mặc định bo góc ảnh theo token; avatar truyền `circle`. */
  circle?: boolean;
  resizeMode?: ImageResizeMode;
  onLoad?(event: ImageLoadEvent): void;
}

/** Ảnh từ URL; lỗi tải hoặc không có URI thì hiện nền xám thay vì crash/trống. */
export function RemoteImage({
  uri,
  accessibilityLabel,
  style,
  circle,
  resizeMode = 'cover',
  onLoad,
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
