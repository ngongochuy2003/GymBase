import { Text } from 'react-native';
import { Tabs } from 'expo-router';

export default function ProtectedLayout() {
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
