import { useCallback, useEffect, useRef, useState, type ReactElement } from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { theme } from '../theme/tokens';

/** Toast đơn giản: `show(message)` hiện thông báo ngắn rồi tự ẩn. Đặt `toast` vào cuối màn hình. */
export function useToast(durationMs = 2200): {
  toast: ReactElement | null;
  show(message: string): void;
} {
  const [message, setMessage] = useState<string | null>(null);
  const timer = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(
    () => () => {
      if (timer.current) clearTimeout(timer.current);
    },
    [],
  );

  const show = useCallback(
    (next: string) => {
      if (timer.current) clearTimeout(timer.current);
      setMessage(next);
      timer.current = setTimeout(() => setMessage(null), durationMs);
    },
    [durationMs],
  );

  const toast = message ? (
    <View pointerEvents="none" style={styles.wrap}>
      <View accessibilityRole="alert" style={styles.toast}>
        <Text style={styles.text}>{message}</Text>
      </View>
    </View>
  ) : null;
  return { toast, show };
}

const styles = StyleSheet.create({
  wrap: { position: 'absolute', left: 0, right: 0, bottom: 120, alignItems: 'center' },
  toast: {
    backgroundColor: theme.colors.overlay,
    paddingHorizontal: theme.spacing.xl,
    paddingVertical: theme.spacing.md,
    borderRadius: theme.radius.chip,
  },
  text: {
    color: theme.colors.onPrimary,
    fontSize: theme.font.size.sm,
    fontWeight: theme.font.weight.medium,
  },
});
