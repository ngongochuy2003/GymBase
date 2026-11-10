import { fireEvent, render, screen } from '@testing-library/react-native';
import { Text } from 'react-native';
import { AppProviders } from '@/providers/app-providers';
import { ComposerScreen } from '@/features/composer';
import { Icon } from '@/shared/icons';
import { ScreenRenderer, registerSection, type ScreenConfig } from '@/shared/screen-config';
import { pickTabs } from '@/shared/navigation';

jest.mock('expo-router', () => ({
  useRouter: () => ({ navigate: jest.fn(), back: jest.fn(), canGoBack: () => false }),
}));

describe('ScreenRenderer', () => {
  const base: ScreenConfig = {
    id: 't',
    sections: [],
    bottomTabs: pickTabs(['home', 'route', 'compose', 'messages', 'profile']),
    activeTabId: 'home',
  };

  test('renders registered sections, skips hidden ones and warns on unknown types', async () => {
    registerSection('hello', ({ name }: { name: string }) => <Text>{`Xin chào ${name}`}</Text>);
    await render(
      <ScreenRenderer
        config={{
          ...base,
          sections: [
            { id: 'a', type: 'hello', props: { name: 'An' } },
            { id: 'b', type: 'hello', props: { name: 'Ẩn' }, visible: false },
            { id: 'c', type: 'khong-ton-tai' },
          ],
        }}
      />,
    );
    expect(screen.getByText('Xin chào An')).toBeTruthy();
    expect(screen.queryByText('Xin chào Ẩn')).toBeNull();
    expect(screen.getByText(/Section chưa đăng ký: "khong-ton-tai"/)).toBeTruthy();
  });

  test('adding a tab only needs a config object', async () => {
    await render(
      <ScreenRenderer
        config={{
          ...base,
          bottomTabs: [...base.bottomTabs, { id: 'extra', label: 'Thêm mới', icon: 'flame' }],
        }}
      />,
    );
    expect(screen.getByRole('tab', { name: 'Thêm mới' })).toBeTruthy();
    expect(screen.getByRole('tab', { name: 'Trang chủ' }).props.accessibilityState.selected).toBe(
      true,
    );
  });

  test('unknown icon renders a placeholder instead of crashing', async () => {
    await render(<Icon name="khong-co" />);
    expect(screen.getByTestId('icon-placeholder-khong-co')).toBeTruthy();
  });
});

describe('ComposerScreen', () => {
  test('media grid, single topic, visibility and mock submit', async () => {
    const log = jest.spyOn(console, 'log').mockImplementation(() => undefined);
    await render(
      <AppProviders>
        <ComposerScreen />
      </AppProviders>,
    );
    // Một ảnh ban đầu; thêm tới 10 thì ô thêm biến mất.
    expect(screen.getAllByLabelText('Xóa ảnh')).toHaveLength(1);
    for (let i = 0; i < 9; i += 1) {
      await fireEvent.press(screen.getByRole('button', { name: 'Thêm ảnh/video' }));
    }
    expect(screen.getAllByLabelText('Xóa ảnh')).toHaveLength(10);
    expect(screen.queryByRole('button', { name: 'Thêm ảnh/video' })).toBeNull();
    await fireEvent.press(screen.getAllByLabelText('Xóa ảnh')[0]!);
    expect(screen.getByRole('button', { name: 'Thêm ảnh/video' })).toBeTruthy();

    // Chủ đề chọn đơn.
    expect(
      screen.getByRole('button', { name: 'Tập luyện' }).props.accessibilityState.selected,
    ).toBe(true);
    await fireEvent.press(screen.getByRole('button', { name: 'Meal prep' }));
    expect(
      screen.getByRole('button', { name: 'Meal prep' }).props.accessibilityState.selected,
    ).toBe(true);
    expect(
      screen.getByRole('button', { name: 'Tập luyện' }).props.accessibilityState.selected,
    ).toBe(false);

    // Quyền xem.
    await fireEvent.press(screen.getByRole('radio', { name: 'Chỉ người theo dõi' }));
    expect(screen.getByLabelText('Quyền xem: Chỉ người theo dõi')).toBeTruthy();

    // Hashtag + đăng.
    await fireEvent.changeText(screen.getByLabelText('Nội dung bài viết'), 'Hôm nay tập chân');
    await fireEvent.press(screen.getByRole('button', { name: 'Thêm hashtag' }));
    expect(screen.getByLabelText('Nội dung bài viết').props.value).toBe('Hôm nay tập chân #');
    await fireEvent.press(screen.getByRole('button', { name: 'Đăng' }));
    expect(await screen.findByText('Đã đăng (mock)')).toBeTruthy();
    expect(log).toHaveBeenCalledWith(
      '[composer] payload',
      expect.objectContaining({
        content: 'Hôm nay tập chân #',
        topicIds: ['meal-prep'],
        visibilityId: 'followers',
      }),
    );
    log.mockRestore();
  });
});
