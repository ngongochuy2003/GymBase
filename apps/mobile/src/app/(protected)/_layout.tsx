import { Text } from 'react-native';
import { Redirect, Tabs } from 'expo-router';
import { useSession } from '@/core/auth/session';
import { LoadingState, Screen } from '@/shared/components/ui';

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
    <Tabs
      screenOptions={{
        headerShown: false,
        // Các màn ScreenRenderer tự vẽ footer theo thiết kế; ẩn tab bar native để không bị lặp.
        tabBarStyle: { display: 'none' },
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
      <Tabs.Screen name="profile-setup" options={{ tabBarButton: () => null }} />
      <Tabs.Screen name="register-success" options={{ tabBarButton: () => null }} />
    </Tabs>
  );
}
