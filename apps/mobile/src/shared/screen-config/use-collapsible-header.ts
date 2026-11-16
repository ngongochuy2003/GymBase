import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
  Animated,
  Easing,
  type LayoutChangeEvent,
  type NativeScrollEvent,
  type NativeSyntheticEvent,
} from 'react-native';

export interface UseCollapsibleHeaderOptions {
  /** Chiều cao ban đầu của header trước khi đo qua onLayout (mặc định: 64) */
  initialHeaderHeight?: number;
  /** Ngưỡng cuộn tối thiểu (pixel) để phát hiện chuyển hướng vuốt (mặc định: 10) */
  scrollThreshold?: number;
  /** Thời gian animation khi ẩn header (ms) (mặc định: 220) */
  hideDuration?: number;
  /** Thời gian animation khi hiện header từ từ (ms) (mặc định: 320) */
  showDuration?: number;
  /** Bật/tắt hiệu ứng ẩn hiện theo chuyển động vuốt (mặc định: true) */
  enabled?: boolean;
}

export interface UseCollapsibleHeaderReturn {
  /** Giá trị biến đổi translateY cho Animated.View của header */
  headerTranslateY: Animated.Value;
  /** Giá trị độ mờ (0 -> 1) cho Animated.View của header */
  headerOpacity: Animated.AnimatedInterpolation<number>;
  /** Trạng thái header đang hiển thị hay đang ẩn */
  isHeaderVisible: boolean;
  /** Chiều cao header đo được thực tế */
  headerHeight: number;
  /** Callback gắn vào onScroll của ScrollView */
  onScroll(event: NativeSyntheticEvent<NativeScrollEvent>): void;
  /** Callback gắn vào onLayout của header view để tự động đo chiều cao */
  onHeaderLayout(event: LayoutChangeEvent): void;
  /** Hàm ép hiện header */
  showHeader(): void;
  /** Hàm ép ẩn header */
  hideHeader(): void;
}

/**
 * Hook điều khiển thanh header ẩn/hiện mượt mà theo chuyển động vuốt của người dùng:
 * - Khi vuốt xuống (cuộn trang để xem nội dung tiếp theo): Header trượt lên và ẩn đi.
 * - Khi vuốt lên (cuộn trang ngược lại để xem nội dung phía trên): Header từ từ trượt xuống và hiện ra.
 * - Khi ở đỉnh trang (contentOffsetY <= 0): Header luôn luôn hiện đầy đủ.
 */
export function useCollapsibleHeader({
  initialHeaderHeight = 64,
  scrollThreshold = 10,
  hideDuration = 220,
  showDuration = 320,
  enabled = true,
}: UseCollapsibleHeaderOptions = {}): UseCollapsibleHeaderReturn {
  const [headerHeight, setHeaderHeight] = useState(initialHeaderHeight);
  const [isHeaderVisible, setIsHeaderVisible] = useState(true);

  // Animated value: 0 là hiển thị hoàn toàn, -headerHeight là ẩn hoàn toàn
  const [headerAnim] = useState(() => new Animated.Value(0));
  const isVisibleRef = useRef(true);
  const prevScrollY = useRef(0);
  const currentHeightRef = useRef(headerHeight);

  useEffect(() => {
    currentHeightRef.current = headerHeight;
  }, [headerHeight]);

  const showHeader = useCallback(() => {
    if (!isVisibleRef.current) {
      isVisibleRef.current = true;
      setIsHeaderVisible(true);
      Animated.timing(headerAnim, {
        toValue: 0,
        duration: showDuration,
        easing: Easing.bezier(0.25, 0.1, 0.25, 1),
        useNativeDriver: true,
      }).start();
    }
  }, [headerAnim, showDuration]);

  const hideHeader = useCallback(() => {
    if (isVisibleRef.current) {
      isVisibleRef.current = false;
      setIsHeaderVisible(false);
      Animated.timing(headerAnim, {
        toValue: -currentHeightRef.current,
        duration: hideDuration,
        easing: Easing.bezier(0.25, 0.1, 0.25, 1),
        useNativeDriver: true,
      }).start();
    }
  }, [headerAnim, hideDuration]);

  const onScroll = useCallback(
    (event: NativeSyntheticEvent<NativeScrollEvent>) => {
      if (!enabled) return;

      const currentY = event.nativeEvent.contentOffset.y;
      const diff = currentY - prevScrollY.current;

      // 1. Khi ở đỉnh trang hoặc cuộn nảy (overscroll trên iOS): Luôn hiện đầy đủ
      if (currentY <= 0) {
        showHeader();
        prevScrollY.current = Math.max(0, currentY);
        return;
      }

      // 2. Vuốt cuộn xuống (scroll down - người dùng đọc tiếp bên dưới)
      // Khi đã cuộn qua một đoạn nhỏ (> 30px) và diff vượt ngưỡng -> Ẩn header
      if (diff > scrollThreshold && currentY > 30) {
        hideHeader();
      }
      // 3. Vuốt cuộn lên (scroll up - người dùng quay lại xem nội dung phía trên)
      // diff âm vượt ngưỡng -> Từ từ hiện header ra
      else if (diff < -scrollThreshold) {
        showHeader();
      }

      prevScrollY.current = currentY;
    },
    [enabled, hideHeader, scrollThreshold, showHeader],
  );

  const onHeaderLayout = useCallback(
    (event: LayoutChangeEvent) => {
      const { height } = event.nativeEvent.layout;
      if (height > 0 && Math.abs(height - headerHeight) > 1) {
        setHeaderHeight(height);
        currentHeightRef.current = height;
        if (!isVisibleRef.current) {
          headerAnim.setValue(-height);
        }
      }
    },
    [headerAnim, headerHeight],
  );

  const headerOpacity = useMemo(
    () =>
      headerAnim.interpolate({
        inputRange: [-Math.max(headerHeight, 1), 0],
        outputRange: [0, 1],
        extrapolate: 'clamp',
      }),
    [headerAnim, headerHeight],
  );

  return {
    headerTranslateY: headerAnim,
    headerOpacity,
    isHeaderVisible,
    headerHeight,
    onScroll,
    onHeaderLayout,
    showHeader,
    hideHeader,
  };
}
