import { View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { Appear, Pop } from '@/shared/motion';
import { useTheme } from '@/shared/theme/theme';

/** Tia lấp lánh quanh vòng tròn: góc quay (độ) và vị trí so với tâm. */
const SPARKLES = [
  { rotate: '-35deg', left: 28, top: 24 },
  { rotate: '0deg', left: 103, top: 4 },
  { rotate: '35deg', left: 178, top: 24 },
] as const;

/**
 * Vòng tròn xanh nhạt có dấu tick lớn và các tia lấp lánh phía trên. Vòng tròn bật ra có nảy nhẹ, ba tia hiện lần lượt sau đó;
 * tắt khi hệ thống bật giảm chuyển động. Phần bố cục nằm trong style của khối animation, phần trang trí nằm ở View con.
 */
export function SuccessBadge() {
  const { colors } = useTheme();
  return (
    <View accessibilityElementsHidden importantForAccessibility="no" className="h-32 w-52">
      {SPARKLES.map((sparkle, index) => (
        <Appear
          key={sparkle.rotate}
          delay={380 + index * 90}
          style={{
            position: 'absolute',
            left: sparkle.left,
            top: sparkle.top,
            transform: [{ rotate: sparkle.rotate }],
          }}
        >
          <View className="h-3.5 w-0.5 rounded-full bg-primary opacity-60" />
        </Appear>
      ))}
      <Pop delay={80} style={{ position: 'absolute', left: 56, top: 20, width: 96, height: 96 }}>
        <View className="size-24 items-center justify-center rounded-full bg-secondary">
          <Ionicons name="checkmark" size={56} color={colors.primary} />
        </View>
      </Pop>
    </View>
  );
}
