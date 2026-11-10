import { fireEvent, render, screen } from '@testing-library/react-native';
import { AppProviders } from '@/providers/app-providers';
import { useSession } from '@/core/auth/session';
import { LoginScreen } from '@/features/auth';
import { FeedScreen } from '@/features/feed';
import { ProfileScreen } from '@/features/profile';
import { AppText } from '@/shared/components/ui';

jest.mock('expo-router', () => ({
  useRouter: () => ({ navigate: jest.fn(), back: jest.fn(), canGoBack: () => false }),
}));

function TestApp() {
  const { status } = useSession();
  if (status === 'loading') return <AppText>Loading session</AppText>;
  if (status === 'guest') return <LoginScreen />;
  return (
    <>
      <FeedScreen />
      <ProfileScreen />
    </>
  );
}

test('validates name, enters the feed home, toggles like, changes theme and logs out', async () => {
  await render(
    <AppProviders>
      <TestApp />
    </AppProviders>,
  );
  await fireEvent.press(await screen.findByRole('button', { name: 'Khám phá bản demo' }));
  expect(await screen.findByText('Nhập ít nhất 2 ký tự.')).toBeTruthy();
  await fireEvent.changeText(screen.getByLabelText('Tên của bạn'), 'Minh Anh');
  await fireEvent.press(screen.getByRole('button', { name: 'Khám phá bản demo' }));

  expect(await screen.findByText(/Buổi Push hôm nay/)).toBeTruthy();
  expect(screen.getByText('Lan Anh')).toBeTruthy();
  expect(screen.getByText('1,248')).toBeTruthy();
  await fireEvent.press(screen.getAllByRole('button', { name: 'like' })[0]!);
  expect(await screen.findByText('1,249')).toBeTruthy();
  await fireEvent.press(screen.getAllByRole('button', { name: 'like' })[0]!);
  expect(await screen.findByText('1,248')).toBeTruthy();

  await fireEvent.press(screen.getByRole('button', { name: 'Chuyển sang giao diện tối' }));
  expect(await screen.findByText('Hiện tại: Tối')).toBeTruthy();
  await fireEvent.press(screen.getByRole('button', { name: 'Thoát bản demo' }));
  expect(await screen.findByLabelText('Tên của bạn')).toBeTruthy();
  expect(screen.queryByText(/Buổi Push hôm nay/)).toBeNull();
}, 30_000);

test('filter chips switch the active filter', async () => {
  await render(
    <AppProviders>
      <FeedScreen />
    </AppProviders>,
  );
  const trending = await screen.findByRole('button', { name: 'Xu hướng' });
  expect(
    screen.getByRole('button', { name: 'Dành cho bạn' }).props.accessibilityState.selected,
  ).toBe(true);
  await fireEvent.press(trending);
  expect(screen.getByRole('button', { name: 'Xu hướng' }).props.accessibilityState.selected).toBe(
    true,
  );
  expect(
    screen.getByRole('button', { name: 'Dành cho bạn' }).props.accessibilityState.selected,
  ).toBe(false);
});
