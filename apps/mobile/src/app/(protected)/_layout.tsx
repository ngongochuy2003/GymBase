import { Tabs } from 'expo-router';
import { Text } from 'react-native';
import { useTheme } from '@/shared/theme/theme';

export default function ProtectedLayout() {
  const { colors } = useTheme();
  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.muted,
        tabBarStyle: { backgroundColor: colors.surface, borderTopColor: colors.border },
        tabBarLabelStyle: { fontSize: 12, fontWeight: '600' },
      }}
    >
      <Tabs.Screen
        name="today"
        options={{
          title: 'Hôm nay',
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
    </Tabs>
  );
}
