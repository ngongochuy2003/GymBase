import { Redirect, Stack } from 'expo-router';
import { useSession } from '@/core/auth/session';
import { ProfileDraftProvider } from '@/features/profile';
import { LoadingState, Screen } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';

/**
 * Nhóm đã đăng nhập: các màn chưa vào app (onboarding, thiết lập hồ sơ) là màn riêng trong Stack, phủ kín màn hình và không có thanh tab.
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

  // Bản nháp thiết lập hồ sơ sống cùng nhóm đã đăng nhập: đăng xuất thì layout gỡ ra và bản nháp mất theo.
  return (
    <ProfileDraftProvider>
      <Stack
        screenOptions={{
          headerShown: false,
          animation: 'fade',
          contentStyle: { backgroundColor: colors.background },
        }}
      >
        <Stack.Screen name="home" />
        <Stack.Screen name="route" />
        <Stack.Screen name="tao-lo-trinh" options={{ animation: 'slide_from_right' }} />
        <Stack.Screen name="chi-tiet-lo-trinh" options={{ animation: 'slide_from_right' }} />
        <Stack.Screen name="check-in-bua-an" options={{ animation: 'slide_from_right' }} />
        <Stack.Screen name="chi-tiet-check-in" options={{ animation: 'slide_from_right' }} />
        <Stack.Screen name="check-in-buoi-tap" options={{ animation: 'slide_from_right' }} />
        <Stack.Screen name="chi-tiet-check-in-buoi-tap" options={{ animation: 'slide_from_right' }} />
        <Stack.Screen name="ghi-can-nang" options={{ animation: 'slide_from_right' }} />
        <Stack.Screen name="ghi-anh-co-the" options={{ animation: 'slide_from_right' }} />
        <Stack.Screen name="profile" />
        <Stack.Screen name="compose" />
        <Stack.Screen name="post-detail" />
        <Stack.Screen name="onboarding" />
        <Stack.Screen name="profile-setup/index" options={{ animation: 'slide_from_right' }} />
        <Stack.Screen name="profile-setup/ca-nhan" options={{ animation: 'slide_from_right' }} />
        <Stack.Screen name="profile-setup/chi-tiet" options={{ animation: 'slide_from_right' }} />
        <Stack.Screen name="profile-setup/hoan-tat" options={{ animation: 'fade' }} />
      </Stack>
    </ProfileDraftProvider>
  );
}
