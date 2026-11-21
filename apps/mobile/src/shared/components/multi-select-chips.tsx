import { Pressable, Text, View } from 'react-native';
import { cn } from '@/lib/utils';
import { FieldLabel } from './field-label';
import type { SelectOption } from './select-field';
import { Button } from './ui';

/**
 * Chọn nhiều giá trị từ danh sách ngắn (danh mục): mỗi lựa chọn là một chip bật/tắt, hiển thị `ten`, giữ và trả `ma`.
 * Đang tải thì hiện "Đang tải…"; lỗi tải thì hiện thông báo kèm nút "Thử lại".
 */
export function MultiSelectChips({
  label,
  value,
  options,
  onChange,
  error,
  loading = false,
  loadError,
  onRetry,
}: {
  /** Tên ô: hiện phía trên ô. */
  label: string;
  value: readonly string[];
  options: readonly SelectOption[];
  onChange(value: string[]): void;
  /** Lỗi nhập liệu (chưa chọn). */
  error?: string;
  loading?: boolean;
  loadError?: string;
  onRetry?(): void;
}) {
  const toggle = (ma: string) =>
    onChange(value.includes(ma) ? value.filter((item) => item !== ma) : [...value, ma]);

  return (
    <View className="gap-2">
      <FieldLabel label={label} />
      {loading ? <Text className="text-base text-muted-foreground">Đang tải…</Text> : null}
      <View className="flex-row flex-wrap gap-2">
        {options.map((option) => {
          const checked = value.includes(option.ma);
          return (
            <Pressable
              key={option.ma}
              accessibilityRole="checkbox"
              accessibilityLabel={option.ten}
              accessibilityState={{ checked }}
              onPress={() => toggle(option.ma)}
              className={cn(
                'min-h-11 justify-center rounded-full border px-4',
                checked ? 'border-primary bg-primary' : 'border-input bg-card',
              )}
            >
              <Text
                className={cn(
                  'text-sm font-semibold',
                  checked ? 'text-primary-foreground' : 'text-foreground',
                )}
              >
                {option.ten}
              </Text>
            </Pressable>
          );
        })}
      </View>
      {error ? (
        <Text accessibilityRole="alert" className="text-destructive">
          {error}
        </Text>
      ) : null}
      {loadError ? (
        <View className="gap-2">
          <Text accessibilityRole="alert" className="text-destructive">
            {loadError}
          </Text>
          {onRetry ? <Button label="Thử lại" secondary onPress={onRetry} /> : null}
        </View>
      ) : null}
    </View>
  );
}
