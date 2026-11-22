import type { ReactNode } from 'react';
import { Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';

/** Khối trắng bo góc có tiêu đề (kèm biểu tượng) và liên kết phụ ở góc phải. */
export function KhungThe({
  icon,
  tieuDe,
  tieuDePhu,
  chuPhu,
  lienKet,
  children,
}: {
  /** Biểu tượng đầu tiêu đề; bỏ trống thì không hiện. */
  icon?: string;
  tieuDe: string;
  /** Chữ nhạt đứng ngay sau tiêu đề, ví dụ "(không bắt buộc)". */
  tieuDePhu?: string;
  /** Chữ nhạt ở góc phải (không bấm được), ví dụ "Đã thêm 1/3 ảnh". */
  chuPhu?: string;
  lienKet?: { nhan: string; onPress(): void };
  children: ReactNode;
}) {
  const { colors } = useTheme();
  return (
    <View className="gap-3 rounded-2xl border border-border bg-card p-4">
      <View className="flex-row items-center gap-2">
        {icon ? <Icon name={icon} size={22} color={colors.text} /> : null}
        <Text accessibilityRole="header" className="flex-1 text-lg font-bold">
          {tieuDe}
          {tieuDePhu ? (
            <Text variant="muted" className="text-sm font-normal">
              {` ${tieuDePhu}`}
            </Text>
          ) : null}
        </Text>
        {chuPhu ? (
          <Text variant="muted" className="text-sm">
            {chuPhu}
          </Text>
        ) : null}
        {lienKet ? (
          <Pressable
            accessibilityRole="button"
            accessibilityLabel={`${lienKet.nhan} ${tieuDe}`}
            hitSlop={8}
            onPress={lienKet.onPress}
            className="flex-row items-center gap-0.5"
          >
            <Text className="text-sm font-bold text-primary">{lienKet.nhan}</Text>
            <Icon name="chevron-right" size={16} color={colors.primary} />
          </Pressable>
        ) : null}
      </View>
      {children}
    </View>
  );
}
