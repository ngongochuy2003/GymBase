import { useEffect } from 'react';
import '../../global.css';
import { DarkTheme, DefaultTheme, Stack, ThemeProvider } from 'expo-router';
import * as SystemUI from 'expo-system-ui';
import { StatusBar } from 'expo-status-bar';
import { AppProviders } from '@/providers/app-providers';
import { useSession } from '@/core/auth/session';
import { useTheme } from '@/shared/theme/theme';
import { LoadingState, Screen } from '@/shared/components/ui';

export const unstable_settings = { initialRouteName: 'index', anchor: 'index' };

function Navigation() {
  const { status } = useSession();

  const { isDark, colors } = useTheme();

  // Thư viện điều hướng có theme riêng (mặc định nền trắng). Không đồng bộ với theme của app thì mỗi lần chuyển màn ở chế độ
  // tối, nền trắng của navigator lộ ra sau màn đang trượt, nhìn như bị chớp.
  const base = isDark ? DarkTheme : DefaultTheme;
  const navigationTheme = {
    ...base,
    colors: {
      ...base.colors,
      primary: colors.primary,
      background: colors.background,
      card: colors.surface,
      text: colors.text,
      border: colors.border,
    },
  };

  // Màu nền của cửa sổ native: lộ ra trong khoảnh khắc mở app và khi màn hình đổi kích thước hoặc đang chuyển cảnh.
  useEffect(() => {
    void SystemUI.setBackgroundColorAsync(colors.background).catch(() => undefined);
  }, [colors.background]);

  if (status === 'loading')
    return (
      <Screen>
        <LoadingState />
      </Screen>
    );
  return (
    <ThemeProvider value={navigationTheme}>
      <StatusBar style={isDark ? 'light' : 'dark'} />
      {/* Chuyển giữa nhóm khách và nhóm đã đăng nhập (và màn index chỉ để chuyển hướng) dùng mờ dần, không trượt: không có
          "màn trước" để trượt ra nên trượt sẽ trông như giật. */}
      <Stack
        screenOptions={{
          headerShown: false,
          animation: 'fade',
          contentStyle: { backgroundColor: colors.background },
        }}
      >
        <Stack.Screen name="index" />
        <Stack.Protected guard={status === 'guest'}>
          <Stack.Screen name="(auth)" />
        </Stack.Protected>
        <Stack.Protected guard={status === 'authenticated'}>
          <Stack.Screen name="(protected)" />
        </Stack.Protected>
      </Stack>
    </ThemeProvider>
  );
}

export default function RootLayout() {
  return (
    <AppProviders>
      <Navigation />
    </AppProviders>
  );
}
