import { fireEvent, render, screen, waitFor } from '@testing-library/react-native';
import { AppProviders } from '@/providers/app-providers';
import { useSession } from '@/core/auth/session';
import { LoginScreen } from '@/features/auth';
import { DashboardScreen } from '@/features/pt-sessions';
import { ProfileScreen } from '@/features/profile';
import { demoSessionsRepository } from '@/features/pt-sessions/api/demo-sessions';
import { AppText } from '@/shared/components/ui';

function TestApp() {
  const { status } = useSession();
  if (status === 'loading') return <AppText>Loading session</AppText>;
  if (status === 'guest') return <LoginScreen />;
  return (
    <>
      <DashboardScreen />
      <ProfileScreen />
    </>
  );
}

test('validates name, enters demo, updates progress, changes theme and logs out', async () => {
  await render(
    <AppProviders>
      <TestApp />
    </AppProviders>,
  );
  await fireEvent.press(await screen.findByRole('button', { name: 'Khám phá bản demo' }));
  expect(await screen.findByText('Nhập ít nhất 2 ký tự.')).toBeTruthy();
  await fireEvent.changeText(screen.getByLabelText('Tên của bạn'), 'Minh Anh');
  await fireEvent.press(screen.getByRole('button', { name: 'Khám phá bản demo' }));
  expect(await screen.findByText('Chào Minh Anh,')).toBeTruthy();
  await fireEvent.press(
    await screen.findByRole('button', { name: 'Đánh dấu đã tập: Sức mạnh thân trên' }),
  );
  expect(await screen.findByText('1/2')).toBeTruthy();
  await fireEvent.press(screen.getByRole('button', { name: 'Chuyển sang giao diện tối' }));
  expect(await screen.findByText('Hiện tại: Tối')).toBeTruthy();
  await fireEvent.press(screen.getByRole('button', { name: 'Thoát bản demo' }));
  expect(await screen.findByLabelText('Tên của bạn')).toBeTruthy();
  expect(screen.queryByText('Sức mạnh thân trên')).toBeNull();
  await fireEvent.changeText(screen.getByLabelText('Tên của bạn'), 'Người mới');
  await fireEvent.press(screen.getByRole('button', { name: 'Khám phá bản demo' }));
  expect(
    await screen.findByRole('button', { name: 'Đánh dấu đã tập: Sức mạnh thân trên' }),
  ).toBeTruthy();
  expect(screen.getByText('0/2')).toBeTruthy();
}, 30_000);

test('shows an empty schedule', async () => {
  const spy = jest.spyOn(demoSessionsRepository, 'list').mockResolvedValue([]);
  await render(
    <AppProviders>
      <DashboardScreen />
    </AppProviders>,
  );
  expect(
    await screen.findByText('Chưa có buổi tập. Lịch tập mới sẽ xuất hiện tại đây.'),
  ).toBeTruthy();
  spy.mockRestore();
});

test('shows an error and retries successfully', async () => {
  const spy = jest
    .spyOn(demoSessionsRepository, 'list')
    .mockRejectedValue(new Error('Mạng demo bị gián đoạn'));
  await render(
    <AppProviders>
      <DashboardScreen />
    </AppProviders>,
  );
  expect(await screen.findByText('Mạng demo bị gián đoạn', {}, { timeout: 4000 })).toBeTruthy();
  spy.mockResolvedValue([]);
  await fireEvent.press(screen.getByRole('button', { name: 'Thử lại' }));
  await waitFor(() =>
    expect(screen.getByText('Chưa có buổi tập. Lịch tập mới sẽ xuất hiện tại đây.')).toBeTruthy(),
  );
  spy.mockRestore();
});
