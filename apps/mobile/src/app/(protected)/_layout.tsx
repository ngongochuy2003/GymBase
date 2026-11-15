import { Stack } from 'expo-router';
import { useTheme } from '@/shared/theme/theme';

/**
 * Nhóm đã đăng nhập: các màn chưa vào app (onboarding, thiết lập hồ sơ) nằm cùng Stack với nhóm tab, nên chúng phủ kín
 * màn hình, không có thanh tab phía dưới.
 */
export default function ProtectedLayout() {
  const { colors } = useTheme();
  return (
    <Stack
      screenOptions={{
        headerShown: false,
        animation: 'fade',
        contentStyle: { backgroundColor: colors.background },
      }}
    >
      <Stack.Screen name="(tabs)" />
      <Stack.Screen name="onboarding" />
      <Stack.Screen name="profile-setup" />
    </Stack>
  );
}
