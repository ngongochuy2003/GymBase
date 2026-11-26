import { Image, Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { KeHoachBuaAn } from '../types';

/** Đầu màn check-in: nút quay lại, tiêu đề và dòng phụ (tên lộ trình). */
export function DauManCheckIn({
  tieuDe,
  phuDe,
  onQuayLai,
}: {
  tieuDe: string;
  phuDe: string;
  onQuayLai(): void;
}) {
  const { colors } = useTheme();
  return (
    <View className="flex-row items-center gap-3 px-4 py-3">
      <Pressable
        accessibilityRole="button"
        accessibilityLabel="Quay lại"
        hitSlop={8}
        onPress={onQuayLai}
      >
        <Icon name="back" size={28} color={colors.text} />
      </Pressable>
      <View className="flex-1">
        <Text accessibilityRole="header" className="text-2xl font-bold leading-8">
          {tieuDe}
        </Text>
        <Text variant="muted" className="text-sm" numberOfLines={1}>
          {phuDe}
        </Text>
      </View>
    </View>
  );
}

/**
 * Thẻ bữa ăn theo kế hoạch của PT: ảnh món, tên bữa, giờ, mô tả và năng lượng dạng "~ 550 kcal".
 */
export function TheBuaAn({ keHoach }: { keHoach: KeHoachBuaAn }) {
  const { colors } = useTheme();
  return (
    <View className="gap-3 rounded-2xl border border-border bg-card p-4">
      <View className="flex-row gap-3">
        <View className="size-36 overflow-hidden rounded-2xl bg-secondary">
          {keHoach.anh ? (
            <Image
              source={{ uri: keHoach.anh }}
              accessibilityLabel={`Món trong kế hoạch ${keHoach.ten.toLowerCase()}`}
              resizeMode="cover"
              className="size-full"
            />
          ) : (
            <View className="size-full items-center justify-center">
              <Icon name="meal" size={36} color={colors.muted} />
            </View>
          )}
        </View>
        <View className="flex-1 gap-2">
          <View className="flex-row items-start gap-2">
            <View className="size-11 items-center justify-center rounded-xl bg-secondary">
              <Icon name="meal" size={22} color={colors.primary} />
            </View>
            <View className="flex-1">
              <Text className="text-lg font-bold leading-6">{keHoach.ten}</Text>
              <View className="flex-row items-center gap-1">
                <Icon name="clock" size={14} color={colors.muted} />
                <Text variant="muted" className="text-sm">
                  {keHoach.gio}
                </Text>
              </View>
            </View>
            <Pressable
              accessibilityRole="button"
              accessibilityLabel="Tùy chọn bữa ăn"
              hitSlop={8}
              // TODO(ROUTE): menu `...` của thẻ bữa ăn chưa có thiết kế (luong-man-hinh.md, HV-04).
              onPress={() => undefined}
            >
              <Icon name="more" size={22} color={colors.muted} />
            </Pressable>
          </View>
          <View className="gap-0.5">
            <Text className="text-base leading-6">{keHoach.moTa}</Text>
            <Text variant="muted" className="text-sm font-medium">
              ~ {keHoach.kcal} kcal
            </Text>
          </View>
        </View>
      </View>
    </View>
  );
}

/** Khối giải thích cuối form; mặc định là lời nhắn của check-in: PT sẽ xem và phản hồi. */
export function GiaiThichPt({
  tieuDe = 'PT sẽ xem và phản hồi',
  noiDung = 'Huấn luyện viên sẽ kiểm tra bữa ăn của bạn và gửi nhận xét, điều chỉnh (nếu cần) trong thời gian sớm nhất.',
}: {
  tieuDe?: string;
  noiDung?: string;
}) {
  const { colors } = useTheme();
  return (
    <View className="flex-row items-center gap-3 rounded-2xl border border-border bg-card p-4">
      <View className="size-12 items-center justify-center rounded-xl bg-secondary">
        <Icon name="person-check" size={26} color={colors.primary} />
      </View>
      <View className="h-12 w-px bg-border" />
      <View className="flex-1 gap-0.5">
        <Text className="font-bold">{tieuDe}</Text>
        <Text variant="muted" className="text-sm leading-5">
          {noiDung}
        </Text>
      </View>
    </View>
  );
}
