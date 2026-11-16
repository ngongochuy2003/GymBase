import { Redirect, Stack } from 'expo-router';
import { useSession } from '@/core/auth/session';
import { LoadingState, Screen } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';

/**
 * Nhóm đã đăng nhập: các màn chưa vào app (onboarding, thiết lập hồ sơ) nằm cùng Stack với nhóm tab, nên chúng phủ kín
 * màn hình, không có thanh tab phía dưới.
 */
export default function ProtectedLayout() {
  const { status } = useSession();
  const { colors } = useTheme();

  if (status === 'loading') {
    return (
      <Screen>
        <LoadingState />
      </Screen>
    );
  }

  if (status !== 'authenticated') {
    return <Redirect href="/welcome" />;
  }

  return (
    <Stack
      screenOptions={{
        headerShown: false,
        animation: 'fade',
        contentStyle: { backgroundColor: colors.background },
      }}
    >
      <Stack.Screen name="home" />
      <Stack.Screen name="(tabs)" />
      <Stack.Screen name="compose" />
      <Stack.Screen name="post-detail" />
      <Stack.Screen name="onboarding" />
      <Stack.Screen name="profile-setup/index" />
      <Stack.Screen name="profile-setup/chi-tiet" />
    </Stack>
  );
}
