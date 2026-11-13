import { Component, useState, type PropsWithChildren, type ErrorInfo } from 'react';
import {
  QueryCache,
  QueryClient,
  QueryClientProvider,
  useQueryClient,
} from '@tanstack/react-query';
import { router } from 'expo-router';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { Text, View } from 'react-native';
import { useEffect } from 'react';
import { SessionProvider, useSession } from '@/core/auth/session';
import { ApiError } from '@/core/api/api-error';
import { ThemeProvider } from '@/shared/theme/theme';

export function createQueryClient() {
  return new QueryClient({
    // Chức năng cần hồ sơ mà tài khoản chưa có (PROFILE_REQUIRED): đưa người dùng tới bước thiết lập hồ sơ.
    queryCache: new QueryCache({
      onError: (error) => {
        if (error instanceof ApiError && error.code === 'PROFILE_REQUIRED') {
          router.navigate('/profile-setup');
        }
      },
    }),
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        gcTime: process.env.NODE_ENV === 'test' ? Infinity : 300_000,
        retry: (count, error) =>
          count < 1 &&
          !(error instanceof ApiError && (error.status ?? 0) >= 400 && (error.status ?? 0) < 500),
      },
      mutations: { retry: false },
    },
  });
}

function ClearSessionCache() {
  const { status } = useSession();
  const client = useQueryClient();
  useEffect(() => {
    if (status === 'guest') client.clear();
  }, [status, client]);
  return null;
}

class AppErrorBoundary extends Component<PropsWithChildren, { failed: boolean }> {
  state = { failed: false };
  static getDerivedStateFromError() {
    return { failed: true };
  }
  componentDidCatch(_error: Error, _info: ErrorInfo) {
    /* Wire a redacted reporting adapter here. */
  }
  render() {
    if (this.state.failed)
      return (
        <View style={{ flex: 1, padding: 32, justifyContent: 'center' }}>
          <Text>Không thể mở GymBase. Vui lòng đóng và mở lại ứng dụng.</Text>
        </View>
      );
    return this.props.children;
  }
}

export function AppProviders({ children }: PropsWithChildren) {
  const [client] = useState(createQueryClient);
  return (
    <AppErrorBoundary>
      <SafeAreaProvider>
        <ThemeProvider>
          <QueryClientProvider client={client}>
            <SessionProvider>
              <ClearSessionCache />
              {children}
            </SessionProvider>
          </QueryClientProvider>
        </ThemeProvider>
      </SafeAreaProvider>
    </AppErrorBoundary>
  );
}
