import { useState } from 'react';
import { Ionicons } from '@expo/vector-icons';
import { Modal, Pressable, ScrollView, Text, View } from 'react-native';
import { cn } from '@/lib/utils';
import { Icon } from '../icons';
import { useTheme } from '../theme/theme';
import { FieldLabel } from './field-label';
import { Button } from './ui';

export type SelectOption = { ma: string; ten: string };

/**
 * Ô chọn một giá trị từ danh sách (danh mục): hiển thị `ten`, giữ và trả `ma`. Bấm vào mở bảng chọn, mục đang chọn có dấu
 * tick. Đang tải thì ô hiện "Đang tải…" và không mở được; lỗi tải thì hiện thông báo kèm nút "Thử lại".
 */
export function SelectField({
  label,
  value,
  options,
  onChange,
  placeholder = 'Chọn',
  error,
  loading = false,
  loadError,
  onRetry,
  hideLabel = false,
  icon,
  triggerClassName,
  valueClassName,
  singleLine = false,
}: {
  /** Tên ô: hiện phía trên ô và là nhãn truy cập. */
  label: string;
  value: string | null;
  options: readonly SelectOption[];
  onChange(ma: string): void;
  placeholder?: string;
  /** Lỗi nhập liệu (chưa chọn). */
  error?: string;
  loading?: boolean;
  /** Thông báo khi không tải được danh sách; có thì hiện kèm nút "Thử lại". */
  loadError?: string;
  onRetry?(): void;
  /** Không hiện nhãn phía trên (nhãn vẫn là tên bảng chọn và nhãn truy cập), dùng khi ô nằm trong một hàng có nhãn riêng. */
  hideLabel?: boolean;
  /** Icon (tên trong `shared/icons`) đặt đầu ô. */
  icon?: string;
  /** Class thêm vào ô bấm, ghi đè kích thước và nền mặc định (ví dụ `min-h-0 h-10 rounded-full`). */
  triggerClassName?: string;
  /** Class thêm vào chữ giá trị (ví dụ `text-sm`). */
  valueClassName?: string;
  /** Cắt giá trị dài thành một dòng (mặc định cho xuống dòng). */
  singleLine?: boolean;
}) {
  const { colors } = useTheme();
  const [open, setOpen] = useState(false);
  const selected = options.find((option) => option.ma === value);
  const display = loading ? 'Đang tải…' : (selected?.ten ?? placeholder);
  const unavailable = loading || Boolean(loadError);

  return (
    <View className="gap-2">
      {hideLabel ? null : <FieldLabel label={label} />}
      <Pressable
        accessibilityRole="button"
        accessibilityLabel={label}
        accessibilityValue={{ text: selected?.ten ?? '' }}
        accessibilityState={{ disabled: unavailable, busy: loading }}
        disabled={unavailable}
        onPress={() => setOpen(true)}
        className={cn(
          'min-h-[52px] flex-row items-center gap-3 rounded-xl border bg-card px-4',
          triggerClassName,
          error ? 'border-destructive' : 'border-input',
          unavailable && 'opacity-60',
        )}
      >
        {icon ? <Icon name={icon} size={20} color={colors.primary} /> : null}
        <Text
          numberOfLines={singleLine ? 1 : undefined}
          className={cn(
            'flex-1 text-base',
            selected ? 'text-foreground' : 'text-muted-foreground',
            valueClassName,
          )}
        >
          {display}
        </Text>
        <Ionicons name="chevron-down" size={20} color={colors.muted} />
      </Pressable>
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
      <Modal visible={open} transparent animationType="slide" onRequestClose={() => setOpen(false)}>
        <Pressable
          accessibilityLabel="Đóng"
          onPress={() => setOpen(false)}
          className="flex-1 justify-end bg-black/40"
        >
          <Pressable className="max-h-[70%] rounded-t-3xl bg-background px-5 pb-8 pt-5">
            <FieldLabel label={label} className="pb-3 text-lg font-bold" />
            {options.length === 0 ? (
              <Text className="py-4 text-muted-foreground">Chưa có lựa chọn nào.</Text>
            ) : null}
            <ScrollView accessibilityRole="radiogroup">
              {options.map((option) => {
                const checked = option.ma === value;
                return (
                  <Pressable
                    key={option.ma}
                    accessibilityRole="radio"
                    accessibilityLabel={option.ten}
                    accessibilityState={{ checked }}
                    onPress={() => {
                      onChange(option.ma);
                      setOpen(false);
                    }}
                    className="min-h-12 flex-row items-center gap-3 border-b border-border py-3"
                  >
                    <Text className="flex-1 text-base text-foreground">{option.ten}</Text>
                    {checked ? (
                      <Ionicons name="checkmark" size={20} color={colors.primary} />
                    ) : null}
                  </Pressable>
                );
              })}
            </ScrollView>
          </Pressable>
        </Pressable>
      </Modal>
    </View>
  );
}
