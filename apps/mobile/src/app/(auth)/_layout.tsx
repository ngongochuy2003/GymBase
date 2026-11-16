import { Stack } from 'expo-router';
import { useTheme } from '@/shared/theme/theme';

/**
 * Các màn khách (chào mừng, đăng ký, đăng nhập) trượt ngang như một chồng màn. `contentStyle` đặt nền theo theme để không
 * chớp nền sáng khi đang trượt ở chế độ tối.
 */
export default function AuthLayout() {
  const { colors } = useTheme();
  return (
    <Stack
      screenOptions={{
        headerShown: false,
        animation: 'slide_from_right',
        contentStyle: { backgroundColor: colors.background },
      }}
    />
  );
}
