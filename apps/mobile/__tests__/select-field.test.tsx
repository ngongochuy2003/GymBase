import { fireEvent, render, screen } from '@testing-library/react-native';
import { SelectField } from '@/shared/components/select-field';
import { ThemeProvider } from '@/shared/theme/theme';

const OPTIONS = [
  { ma: 'nam', ten: 'Nam' },
  { ma: 'nu', ten: 'Nữ' },
  { ma: 'khong_muon_noi', ten: 'Không muốn nói' },
];

async function renderField(props: Partial<React.ComponentProps<typeof SelectField>> = {}) {
  const onChange = jest.fn();
  await render(
    <ThemeProvider>
      <SelectField
        label="Giới tính *"
        value={null}
        options={OPTIONS}
        onChange={onChange}
        placeholder="Chọn giới tính"
        {...props}
      />
    </ThemeProvider>,
  );
  return onChange;
}

test('shows the placeholder, then the chosen item name instead of its code', async () => {
  await renderField();
  expect(screen.getByText('Chọn giới tính')).toBeTruthy();

  await screen.unmount();
  await renderField({ value: 'nu' });
  expect(screen.getByText('Nữ')).toBeTruthy();
  expect(screen.queryByText('nu')).toBeNull();
});

test('opens a list of names, and choosing one sends its code and closes the list', async () => {
  const onChange = await renderField({ value: 'nam' });
  expect(screen.queryAllByRole('radio')).toHaveLength(0);

  await fireEvent.press(screen.getByRole('button', { name: 'Giới tính *' }));

  const radios = screen.getAllByRole('radio');
  expect(radios.map((radio) => radio.props.accessibilityLabel)).toEqual([
    'Nam',
    'Nữ',
    'Không muốn nói',
  ]);
  expect(screen.getByRole('radio', { name: 'Nam' })).toBeChecked();
  expect(screen.getByRole('radio', { name: 'Nữ' })).not.toBeChecked();

  await fireEvent.press(screen.getByRole('radio', { name: 'Nữ' }));

  expect(onChange).toHaveBeenCalledWith('nu');
  expect(screen.queryAllByRole('radio')).toHaveLength(0);
});

test('while loading it says so and cannot be opened', async () => {
  await renderField({ loading: true, options: [] });

  expect(screen.getByText('Đang tải…')).toBeTruthy();
  expect(screen.getByRole('button', { name: 'Giới tính *' })).toBeDisabled();
});

test('a load error shows the message with a Retry button and blocks opening', async () => {
  const onRetry = jest.fn();
  await renderField({ options: [], loadError: 'Không tải được danh sách.', onRetry });

  expect(screen.getByText('Không tải được danh sách.')).toBeTruthy();
  expect(screen.getByRole('button', { name: 'Giới tính *' })).toBeDisabled();

  await fireEvent.press(screen.getByRole('button', { name: 'Thử lại' }));
  expect(onRetry).toHaveBeenCalledTimes(1);
});

test('a validation error shows under the field', async () => {
  await renderField({ error: 'Vui lòng chọn giới tính.' });

  expect(screen.getByRole('alert')).toHaveTextContent('Vui lòng chọn giới tính.');
});

test('the required asterisk is its own text so it can be red, and the field name stays whole', async () => {
  await renderField();

  const asterisk = screen.getByText('*');
  expect(asterisk.props.className).toContain('text-destructive');
  expect(screen.getByRole('button', { name: 'Giới tính *' })).toBeTruthy();
});

test('a label without an asterisk is plain text', async () => {
  await renderField({ label: 'Ghi chú' });

  expect(screen.getByText('Ghi chú')).toBeTruthy();
  expect(screen.queryByText('*')).toBeNull();
});
