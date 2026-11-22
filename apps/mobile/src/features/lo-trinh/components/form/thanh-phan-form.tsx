import type { PropsWithChildren } from 'react';
import { Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';

/** Khối của form: ô icon vuông, tiêu đề, mô tả ngắn, rồi nội dung. */
export function KhoiForm({
  icon,
  tieuDe,
  moTa,
  children,
}: PropsWithChildren<{ icon: string; tieuDe: string; moTa: string }>) {
  const { colors } = useTheme();
  return (
    <View className="gap-4 rounded-3xl border border-border bg-card p-4">
      <View className="flex-row items-center gap-3">
        <View className="h-11 w-11 items-center justify-center rounded-2xl bg-secondary">
          <Icon name={icon} size={24} color={colors.primary} />
        </View>
        <View className="flex-1">
          <Text className="text-lg font-bold text-foreground">{tieuDe}</Text>
          <Text className="text-sm text-muted-foreground">{moTa}</Text>
        </View>
      </View>
      {children}
    </View>
  );
}

/** Một hàng nhãn bên trái, ô nhập bên phải; `loi` hiện dưới ô nhập. */
export function HangForm({
  nhan,
  batBuoc = false,
  loi,
  canTren = false,
  children,
}: PropsWithChildren<{ nhan: string; batBuoc?: boolean; loi?: string; canTren?: boolean }>) {
  return (
    <View className={cn('flex-row gap-3', canTren ? 'items-start' : 'items-center')}>
      <Text className={cn('w-[88px] text-base text-foreground', canTren && 'pt-2.5')}>
        {nhan}
        {batBuoc ? <Text className="text-destructive"> *</Text> : null}
      </Text>
      <View className="flex-1 gap-1">
        {children}
        {loi ? (
          <Text accessibilityRole="alert" className="text-xs text-destructive">
            {loi}
          </Text>
        ) : null}
      </View>
    </View>
  );
}

/** Ô tích vuông dùng cho các dòng bật/tắt trong form. */
export function HopKiem({
  chon,
  onDoi,
  nhan,
}: {
  chon: boolean;
  onDoi(chon: boolean): void;
  nhan: string;
}) {
  const { colors } = useTheme();
  return (
    <Pressable
      accessibilityRole="checkbox"
      accessibilityLabel={nhan}
      accessibilityState={{ checked: chon }}
      hitSlop={8}
      onPress={() => onDoi(!chon)}
      className={cn(
        'h-7 w-7 items-center justify-center rounded-lg border-2',
        chon ? 'border-primary bg-primary' : 'border-input bg-card',
      )}
    >
      {chon ? <Icon name="check" size={16} color={colors.onPrimary} /> : null}
    </Pressable>
  );
}
