import { useMemo } from 'react';
import { View } from 'react-native';
import { useTheme } from '@/shared/theme/theme';

/** Độ cao (px) của sóng ở các điểm cách đều nhau theo chiều ngang, từ mép trái sang mép phải; 0 là đỉnh, lớn là thấp. */
const WAVE_POINTS = [48, 25, 17, 27, 22, 0] as const;
export const WAVE_HEIGHT = 48;

/** Bề rộng mỗi cột (px); 1px để mép sóng mịn như đường vẽ không khử răng cưa. */
const COLUMN = 1;

/** Nội suy Catmull-Rom qua `WAVE_POINTS`: đường cong trơn, đi đúng qua từng điểm. */
function waveHeight(fraction: number) {
  const last = WAVE_POINTS.length - 1;
  const position = Math.min(Math.max(fraction, 0), 1) * last;
  const index = Math.min(Math.floor(position), last - 1);
  const t = position - index;
  const p0 = WAVE_POINTS[Math.max(index - 1, 0)]!;
  const p1 = WAVE_POINTS[index]!;
  const p2 = WAVE_POINTS[index + 1]!;
  const p3 = WAVE_POINTS[Math.min(index + 2, last)]!;
  return (
    0.5 *
    (2 * p1 +
      (-p0 + p2) * t +
      (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t +
      (-p0 + 3 * p1 - 3 * p2 + p3) * t * t * t)
  );
}

/**
 * Mép sóng ở chân hero, tô màu nền trang để "cắt" ảnh thành đường lượn: thấp bên trái, gợn một nhịp, rồi vút lên ở mép phải.
 * React Native không có clip-path nên sóng được dựng từ các cột mảnh cao thấp khác nhau; không cần thư viện thêm.
 */
export function WaveEdge({ width }: { width: number }) {
  const { colors } = useTheme();
  const columns = useMemo(() => {
    const count = Math.ceil(width / COLUMN);
    return Array.from({ length: count }, (_, column) => {
      const height = waveHeight((column * COLUMN + COLUMN / 2) / width);
      return { key: column, left: column * COLUMN, top: Math.max(height, 0) };
    });
  }, [width]);

  if (width === 0) return null;
  return (
    <View
      pointerEvents="none"
      style={{ position: 'absolute', left: 0, right: 0, bottom: -1, height: WAVE_HEIGHT + 1 }}
    >
      {columns.map((column) => (
        <View
          key={column.key}
          style={{
            position: 'absolute',
            left: column.left,
            width: COLUMN + 1,
            top: column.top,
            bottom: 0,
            backgroundColor: colors.background,
          }}
        />
      ))}
    </View>
  );
}
