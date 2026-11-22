import { View } from 'react-native';
import { Text } from '@/components/ui/text';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';

/** Thẻ đầu màn ghi số đo: biểu tượng, tên nhiệm vụ (kèm tuần) và mô tả chu kỳ. */
export function TheNhiemVuGhi({
  icon,
  tieuDe,
  moTa,
}: {
  icon: string;
  tieuDe: string;
  moTa: string;
}) {
  const { colors } = useTheme();
  return (
    <View className="flex-row items-center gap-3 rounded-2xl border border-border bg-card p-4">
      <View className="size-12 items-center justify-center rounded-full bg-secondary">
        <Icon name={icon} size={24} color={colors.primary} />
      </View>
      <View className="flex-1">
        <Text className="text-lg font-bold leading-6">{tieuDe}</Text>
        <Text variant="muted" className="text-sm">
          {moTa}
        </Text>
      </View>
    </View>
  );
}

/** Dải xanh báo nhiệm vụ đã ghi (thay cho "đã gửi, chờ PT" vì không qua duyệt, N16). */
export function DaGhi({ gioGhi, noiDung }: { gioGhi: string; noiDung: string }) {
  const { colors } = useTheme();
  return (
    <View className="flex-row items-center gap-3 rounded-2xl bg-success/15 p-4">
      <View className="size-10 items-center justify-center rounded-full bg-primary">
        <Icon name="check" size={22} color={colors.onPrimary} />
      </View>
      <View className="flex-1">
        <Text className="font-bold text-success">Đã ghi lúc {gioGhi}</Text>
        <Text variant="muted" className="text-sm">
          {noiDung}
        </Text>
      </View>
    </View>
  );
}
