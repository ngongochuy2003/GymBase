import { useState } from 'react';
import { Image, StyleSheet, View, type StyleProp, type ViewStyle } from 'react-native';
import { theme } from '../theme/tokens';

export interface RemoteImageProps {
  uri?: string;
  accessibilityLabel?: string;
  style?: StyleProp<ViewStyle>;
  /** Mặc định bo góc ảnh theo token; avatar truyền `circle`. */
  circle?: boolean;
}

/** Ảnh từ URL; lỗi tải hoặc không có URI thì hiện nền xám thay vì crash/trống. */
export function RemoteImage({ uri, accessibilityLabel, style, circle }: RemoteImageProps) {
  const [failed, setFailed] = useState(false);
  const radius = circle ? theme.radius.chip : theme.radius.image;
  if (!uri || failed) {
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
      source={{ uri }}
      onError={() => setFailed(true)}
      style={[styles.image, { borderRadius: radius }, style as object]}
    />
  );
}

const styles = StyleSheet.create({
  image: { backgroundColor: theme.colors.placeholder },
  fallback: { backgroundColor: theme.colors.placeholder, overflow: 'hidden' },
});
