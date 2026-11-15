import { useEffect, type PropsWithChildren } from 'react';
import type { StyleProp, ViewStyle } from 'react-native';
import Animated, {
  ReduceMotion,
  useAnimatedStyle,
  useSharedValue,
  withDelay,
  withSpring,
  withTiming,
} from 'react-native-reanimated';

/**
 * Animation vào màn dựng bằng giá trị chia sẻ (không dùng layout animation `entering`), vì cách này chạy giống nhau trên native
 * lẫn web. Tất cả theo cài đặt "giảm chuyển động" của hệ thống: bật thì hiện ngay, không chuyển động.
 */
type MotionProps = PropsWithChildren<{
  /** Chờ bao lâu (ms) rồi mới chạy. */
  delay?: number;
  /** Chỉ nhận `style`, không nhận `className`: NativeWind không áp class lên component của Reanimated một cách ổn định. */
  style?: StyleProp<ViewStyle>;
}>;

const reduceMotion = () => ReduceMotion.System;

/** Hiện dần. */
export function Appear({ delay = 0, style, children }: MotionProps) {
  const progress = useSharedValue(0);
  useEffect(() => {
    progress.value = withDelay(
      delay,
      withTiming(1, { duration: 400, reduceMotion: reduceMotion() }),
    );
  }, [delay, progress]);
  const animated = useAnimatedStyle(() => ({ opacity: progress.value }));
  return <Animated.View style={[style, animated]}>{children}</Animated.View>;
}

/** Trồi lên 16px và hiện dần. */
export function Rise({ delay = 0, style, children }: MotionProps) {
  const progress = useSharedValue(0);
  useEffect(() => {
    progress.value = withDelay(
      delay,
      withTiming(1, { duration: 380, reduceMotion: reduceMotion() }),
    );
  }, [delay, progress]);
  const animated = useAnimatedStyle(() => ({
    opacity: progress.value,
    transform: [{ translateY: (1 - progress.value) * 16 }],
  }));
  return <Animated.View style={[style, animated]}>{children}</Animated.View>;
}

/** Bật ra từ 60% lên 100% có nảy nhẹ, kèm hiện dần. */
export function Pop({ delay = 0, style, children }: MotionProps) {
  const progress = useSharedValue(0);
  useEffect(() => {
    progress.value = withDelay(
      delay,
      withSpring(1, { damping: 11, stiffness: 160, reduceMotion: reduceMotion() }),
    );
  }, [delay, progress]);
  const animated = useAnimatedStyle(() => ({
    opacity: Math.min(progress.value * 2, 1),
    transform: [{ scale: 0.6 + 0.4 * progress.value }],
  }));
  return <Animated.View style={[style, animated]}>{children}</Animated.View>;
}

/** Dùng cho phần animation khác (carousel) cần cùng quy ước giảm chuyển động. */
export function systemReduceMotion() {
  return reduceMotion();
}
