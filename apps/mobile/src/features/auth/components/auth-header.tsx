import { Image, Pressable, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useTheme } from '@/shared/theme/theme';

const LOGO_LIGHT = require('../../../../assets/images/logo.png');
const LOGO_DARK = require('../../../../assets/images/logo-dark.png');

/** Logo gốc rộng 480, cao 117; hiển thị cao 40. */
const LOGO_SIZE = { width: 164, height: 40 } as const;

/** Đầu trang chung của các màn auth: logo GymBase căn giữa, tùy chọn mũi tên quay lại ở góc trái. */
export function AuthHeader({ onBack }: { onBack?(): void }) {
  const { colors, isDark } = useTheme();
  return (
    <View className="min-h-11 items-center justify-center">
      {onBack ? (
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Quay lại"
          hitSlop={12}
          onPress={onBack}
          className="absolute left-0 z-10 size-10 items-start justify-center"
        >
          <Ionicons name="chevron-back" size={26} color={colors.text} />
        </Pressable>
      ) : null}
      <Image
        source={isDark ? LOGO_DARK : LOGO_LIGHT}
        accessibilityLabel="GymBase"
        resizeMode="contain"
        style={LOGO_SIZE}
      />
    </View>
  );
}
