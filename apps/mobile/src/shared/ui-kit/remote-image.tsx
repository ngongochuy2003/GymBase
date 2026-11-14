import { useState } from 'react';
import { Image, StyleSheet, View, type StyleProp, type ViewStyle } from 'react-native';
import { DEFAULT_LOCAL_API_URL } from '@/core/api/mobile-client';
import { env } from '@/core/config/env';
import { theme } from '../theme/tokens';

export interface RemoteImageProps {
  uri?: string;
  accessibilityLabel?: string;
  style?: StyleProp<ViewStyle>;
  /** Mặc định bo góc ảnh theo token; avatar truyền `circle`. */
  circle?: boolean;
}

/**
 * Nối API Base URL của backend với đường dẫn tương đối (bắt đầu bằng /)
 * để thẻ Image của React Native hiển thị được ảnh.
 */
export function resolveRemoteImageUri(uri?: string | null): string | undefined {
  if (!uri) return undefined;
  const trimmed = uri.trim();
  if (!trimmed) return undefined;
  if (
    trimmed.startsWith('http://') ||
    trimmed.startsWith('https://') ||
    trimmed.startsWith('data:') ||
    trimmed.startsWith('file://') ||
    trimmed.startsWith('blob:')
  ) {
    return trimmed;
  }
  const apiHostBase = (env.apiUrl || DEFAULT_LOCAL_API_URL)
    .replace(/\/api\/v1\/mob\/?$/, '')
    .replace(/\/+$/, '');
  return `${apiHostBase}${trimmed.startsWith('/') ? '' : '/'}${trimmed}`;
}

/** Ảnh từ URL; lỗi tải hoặc không có URI thì hiện nền xám thay vì crash/trống. */
export function RemoteImage({ uri, accessibilityLabel, style, circle }: RemoteImageProps) {
  const [failed, setFailed] = useState(false);
  const resolvedUri = resolveRemoteImageUri(uri);
  const radius = circle ? theme.radius.chip : theme.radius.image;

  if (!resolvedUri || failed) {
    return (
      <View
        testID="image-fallback"
        accessibilityLabel={accessibilityLabel}
        style={[styles.fallback, { borderRadius: radius }, style]}
      />
    );
  }

  return (
    <Image
      accessibilityLabel={accessibilityLabel}
      source={{ uri: resolvedUri }}
      onError={() => setFailed(true)}
      style={[styles.image, { borderRadius: radius }, style as object]}
    />
  );
}

const styles = StyleSheet.create({
  image: { backgroundColor: theme.colors.placeholder },
  fallback: { backgroundColor: theme.colors.placeholder, overflow: 'hidden' },
});
