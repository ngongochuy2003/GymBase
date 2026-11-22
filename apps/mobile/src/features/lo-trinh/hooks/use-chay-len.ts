import { useEffect } from 'react';
import { Easing, useSharedValue, withDelay, withTiming } from 'react-native-reanimated';
import { systemReduceMotion } from '@/shared/motion';

/**
 * Giá trị chia sẻ chạy 0 → 1 (chậm dần về cuối) trên luồng UI, nên không phụ thuộc luồng JS bận hay rảnh. Dùng cho thanh
 * tiến độ và biểu đồ; tuân theo "giảm chuyển động" của hệ thống như các animation khác trong app. `tre` là thời gian chờ (ms).
 */
export function useChayLen(thoiLuong = 1000, tre = 0) {
  const tienTrinh = useSharedValue(0);
  useEffect(() => {
    tienTrinh.value = withDelay(
      tre,
      withTiming(1, {
        duration: thoiLuong,
        easing: Easing.out(Easing.cubic),
        reduceMotion: systemReduceMotion(),
      }),
    );
  }, [thoiLuong, tre, tienTrinh]);
  return tienTrinh;
}
