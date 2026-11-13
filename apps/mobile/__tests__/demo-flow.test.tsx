import { fireEvent, render, screen, waitFor } from '@testing-library/react-native';
import { AppProviders } from '@/providers/app-providers';
import { DashboardScreen } from '@/features/pt-sessions';
import { demoSessionsRepository } from '@/features/pt-sessions/api/demo-sessions';

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
