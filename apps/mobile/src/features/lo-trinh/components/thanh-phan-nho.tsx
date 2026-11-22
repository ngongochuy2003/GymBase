import { Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { TrangThaiLoTrinh } from '../types';

const NHAN_TRANG_THAI: Record<
  TrangThaiLoTrinh,
  { nhan: string; nen: string; chu: string; cham: string }
> = {
  dang_hoat_dong: {
    nhan: 'Đang thực hiện',
    nen: 'bg-success/15',
    chu: 'text-success',
    cham: 'bg-success',
  },
  hoan_thanh: { nhan: 'Đã hoàn thành', nen: 'bg-info/15', chu: 'text-info', cham: 'bg-info' },
  that_bai: {
    nhan: 'Thất bại',
    nen: 'bg-destructive/15',
    chu: 'text-destructive',
    cham: 'bg-destructive',
  },
};

/** Nhãn trạng thái dạng viên thuốc có chấm màu. */
export function NhanTrangThai({ trangThai }: { trangThai: TrangThaiLoTrinh }) {
  const kieu = NHAN_TRANG_THAI[trangThai];
  return (
    <View
      className={cn('flex-row items-center gap-1.5 self-start rounded-full px-2.5 py-1', kieu.nen)}
    >
      <View className={cn('h-1.5 w-1.5 rounded-full', kieu.cham)} />
      <Text className={cn('text-xs font-medium', kieu.chu)}>{kieu.nhan}</Text>
    </View>
  );
}

/** Thanh tiến độ ngang, `phanTram` từ 0 đến 100. */
export function ThanhTienDo({ phanTram, className }: { phanTram: number; className?: string }) {
  const rong = Math.max(0, Math.min(100, phanTram));
  return (
    <View
      accessibilityRole="progressbar"
      accessibilityValue={{ min: 0, max: 100, now: rong }}
      className={cn('h-2 flex-1 overflow-hidden rounded-full bg-secondary', className)}
    >
      <View className="h-full rounded-full bg-primary" style={{ width: `${rong}%` }} />
    </View>
  );
}

/** Nút `...` mở menu của thẻ. */
export function NutMenu({ nhan }: { nhan: string }) {
  const { colors } = useTheme();
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={nhan}
      hitSlop={8}
      // TODO(ROUTE): menu `...` trên thẻ lộ trình chưa có thiết kế (luong-man-hinh.md, HV-01 và PT-01).
      onPress={() => undefined}
    >
      <Icon name="more" size={22} color={colors.muted} />
    </Pressable>
  );
}
