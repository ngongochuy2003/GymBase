import { Text } from 'react-native';
import { Redirect, Tabs } from 'expo-router';
import { useSession } from '@/core/auth/session';
import { LoadingState, Screen } from '@/shared/components/ui';
import { Stack } from 'expo-router';
import { useTheme } from '@/shared/theme/theme';
/**
 * Nhóm đã đăng nhập: các màn chưa vào app (onboarding, thiết lập hồ sơ) nằm cùng Stack với nhóm tab, nên chúng phủ kín
 * màn hình, không có thanh tab phía dưới.
 */
export default function ProtectedLayout() {
  const { status } = useSession();

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
        // Các màn ScreenRenderer tự vẽ footer theo thiết kế; ẩn tab bar native để không bị lặp.
        contentStyle: { display: 'none' },
      }}
    >
      <Tabs.Screen
        name="home"
        options={{
          title: 'Bài viết',
          tabBarIcon: ({ color }) => <Text style={{ color, fontSize: 22 }}>◉</Text>,
        }}
      />
      <Tabs.Screen
        name="profile"
        options={{
          title: 'Cá nhân',
          tabBarIcon: ({ color }) => <Text style={{ color, fontSize: 22 }}>◎</Text>,
        }}
      />
      <Tabs.Screen name="compose" options={{ tabBarButton: () => null }} />
      <Tabs.Screen name="post-detail" options={{ tabBarButton: () => null }} />
      <Tabs.Screen name="profile-setup" options={{ tabBarButton: () => null }} />
      <Tabs.Screen name="register-success" options={{ tabBarButton: () => null }} />
      <Tabs.Screen
        options={{
          animation: 'fade',
          
        }}
      />
      <Stack.Screen name="(tabs)" />
      <Stack.Screen name="onboarding" />
      <Stack.Screen name="profile-setup" />
    </Stack>
  );
}
