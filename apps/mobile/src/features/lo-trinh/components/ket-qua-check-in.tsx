import { View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { TrangThaiDuyet } from '../types';

/** Sắc thái của kết quả: hoàn thành (xanh), một phần (cam), không hoàn thành (đỏ). */
export type KieuKetQua = 'thanh_cong' | 'canh_bao' | 'loi';

const KIEU: Record<KieuKetQua, { nen: string; nenIcon: string; chu: string; icon: string }> = {
  thanh_cong: { nen: 'bg-success/15', nenIcon: 'bg-primary', chu: 'text-success', icon: 'check' },
  canh_bao: {
    nen: 'bg-warning/15',
    nenIcon: 'bg-warning',
    chu: 'text-warning',
    icon: 'circle-half',
  },
  loi: {
    nen: 'bg-destructive/15',
    nenIcon: 'bg-destructive',
    chu: 'text-destructive',
    icon: 'close',
  },
};

/**
 * Ô lớn báo kết quả của một check-in (bữa ăn hoặc buổi tập) ở màn chi tiết. Khi PT đã duyệt thì tiêu đề đổi thành
 * `tieuDeDaDuyet`, dòng phụ `moTa` vẫn nói rõ kết quả và màu vẫn theo kết quả. Chưa duyệt thì thêm một dòng báo "Đang chờ
 * PT duyệt" hoặc "PT yêu cầu bổ sung".
 */
export function KetQuaCheckIn({
  kieu,
  tieuDe,
  tieuDeDaDuyet,
  moTa,
  trangThaiDuyet,
}: {
  kieu: KieuKetQua;
  tieuDe: string;
  tieuDeDaDuyet: string;
  moTa: string;
  trangThaiDuyet: TrangThaiDuyet;
}) {
  const { colors } = useTheme();
  const k = KIEU[kieu];
  return (
    <View className={cn('flex-row items-center gap-3 rounded-2xl p-4', k.nen)}>
      <View className={cn('size-12 items-center justify-center rounded-full', k.nenIcon)}>
        <Icon name={k.icon} size={26} color={colors.onPrimary} />
      </View>
      <View className="flex-1">
        <Text className={cn('text-lg font-bold', k.chu)}>
          {trangThaiDuyet === 'da_duyet' ? tieuDeDaDuyet : tieuDe}
        </Text>
        <Text variant="muted" className="text-sm">
          {moTa}
        </Text>
        {trangThaiDuyet === 'cho_duyet' ? (
          <Text variant="muted" className="text-sm font-medium">
            Đang chờ PT duyệt
          </Text>
        ) : null}
        {trangThaiDuyet === 'can_bo_sung' ? (
          <Text className="text-sm font-medium text-warning">PT yêu cầu bổ sung</Text>
        ) : null}
      </View>
    </View>
  );
}
