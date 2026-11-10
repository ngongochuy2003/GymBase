import { useEffect } from 'react';
import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { AppProviders } from '@/providers/app-providers';
import { useSession } from '@/core/auth/session';
import { useTheme } from '@/shared/theme/theme';
import { LoadingState, Screen } from '@/shared/components/ui';
import {
  requestNotificationPermissionsAsync,
  setupNotificationChannelsAsync,
} from '@/shared/notifications';

export const unstable_settings = { initialRouteName: 'index', anchor: 'index' };

function Navigation() {
  const { status } = useSession();
  const { isDark } = useTheme();

  useEffect(() => {
    // Khởi tạo Notification Channels (IMPORTANCE_MAX) và xin quyền màn hình khóa
    setupNotificationChannelsAsync().catch(() => {});
    requestNotificationPermissionsAsync().catch(() => {});
  }, []);
  if (status === 'loading')
    return (
      <Screen>
        <LoadingState />
      </Screen>
    );
  return (
    <>
      <StatusBar style={isDark ? 'light' : 'dark'} />
      <Stack screenOptions={{ headerShown: false }}>
        <Stack.Screen name="index" />
        <Stack.Protected guard={status === 'guest'}>
          <Stack.Screen name="(auth)" />
        </Stack.Protected>
        <Stack.Protected guard={status === 'demo'}>
          <Stack.Screen name="(protected)" />
        </Stack.Protected>
      </Stack>
    </>
  );
}

export default function RootLayout() {
  return (
    <AppProviders>
      <Navigation />
    </AppProviders>
  );
}
