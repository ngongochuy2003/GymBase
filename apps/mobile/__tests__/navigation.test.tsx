import { fireEvent, renderRouter, screen, testRouter, waitFor } from 'expo-router/testing-library';
import * as RootLayout from '@/app/_layout';
import EntryRoute from '@/app/index';
import AuthLayout from '@/app/(auth)/_layout';
import LoginRoute from '@/app/(auth)/login';
import ProtectedLayout from '@/app/(protected)/_layout';
import HomeRoute from '@/app/(protected)/home';
import ComposeRoute from '@/app/(protected)/compose';
import ProfileRoute from '@/app/(protected)/profile';
import NotFound from '@/app/+not-found';

afterEach(() => {
  jest.useRealTimers();
});

test('protects deep links and returns to login after logout instead of not-found', async () => {
  await renderRouter(
    {
      _layout: RootLayout,
      index: EntryRoute,
      '(auth)/_layout': AuthLayout,
      '(auth)/login': LoginRoute,
      '(protected)/_layout': ProtectedLayout,
      '(protected)/home': HomeRoute,
      '(protected)/compose': ComposeRoute,
      '(protected)/profile': ProfileRoute,
      '+not-found': NotFound,
    },
    { initialUrl: '/profile' },
  );

  expect(await screen.findByLabelText('Tên của bạn')).toBeTruthy();
  expect(screen.queryByText('Không gian của bạn')).toBeNull();
  await fireEvent.changeText(screen.getByLabelText('Tên của bạn'), 'Minh Anh');
  await fireEvent.press(screen.getByRole('button', { name: 'Khám phá bản demo' }));
  await waitFor(() => expect(screen).toHavePathname('/home'));
  expect(await screen.findByText(/Buổi Push hôm nay/)).toBeTruthy();
  await fireEvent.press(screen.getByRole('tab', { name: 'Đăng bài' }));
  await waitFor(() => expect(screen).toHavePathname('/compose'));
  expect(await screen.findByText('Quyền xem')).toBeTruthy();
  await testRouter.navigate('/profile');
  await fireEvent.press(await screen.findByRole('button', { name: 'Thoát bản demo' }));
  await waitFor(() => expect(screen).toHavePathname('/login'));
  expect(await screen.findByLabelText('Tên của bạn')).toBeTruthy();
  expect(screen.queryByText('Không tìm thấy màn hình')).toBeNull();
}, 30_000);
