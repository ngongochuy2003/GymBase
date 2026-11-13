import { renderRouter, screen, waitFor } from 'expo-router/testing-library';
import * as RootLayout from '@/app/_layout';
import EntryRoute from '@/app/index';
import AuthLayout from '@/app/(auth)/_layout';
import LoginRoute from '@/app/(auth)/login';
import ProtectedLayout from '@/app/(protected)/_layout';
import HomeRoute from '@/app/(protected)/home';
import ComposeRoute from '@/app/(protected)/compose';
import ProfileRoute from '@/app/(protected)/profile';
import NotFound from '@/app/+not-found';

test('unauthenticated deep links lead to the real login screen', async () => {
  await renderRouter({
    _layout: RootLayout, index: EntryRoute, '(auth)/_layout': AuthLayout,
    '(auth)/login': LoginRoute, '(protected)/_layout': ProtectedLayout,
    '(protected)/home': HomeRoute, '(protected)/compose': ComposeRoute,
    '(protected)/profile': ProfileRoute, '+not-found': NotFound,
  }, { initialUrl: '/profile' });
  await waitFor(() => expect(screen).toHavePathname('/login'));
  expect(await screen.findByLabelText('Tên đăng nhập')).toBeTruthy();
  expect(screen.getByLabelText('Mật khẩu')).toBeTruthy();
  expect(screen.queryByText('Không gian của bạn')).toBeNull();
});
