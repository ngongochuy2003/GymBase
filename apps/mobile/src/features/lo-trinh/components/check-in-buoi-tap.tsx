import { Image, Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { BaiTap, KeHoachBuoiTap } from '../types';
import type { KieuKetQua } from './ket-qua-check-in';
import { NutHuongDan } from './nut-huong-dan';

/** Nhãn và sắc thái của từng kết quả buổi tập (N3 chỉ áp cho bữa ăn; buổi tập giữ ba giá trị theo HV-06). */
export const KET_QUA_BUOI_TAP = {
  da_tap: { nhan: 'Đã tập', kieu: 'thanh_cong' as KieuKetQua, moTa: 'Bạn đã tập buổi này' },
  tap_mot_phan: {
    nhan: 'Tập một phần',
    kieu: 'canh_bao' as KieuKetQua,
    moTa: 'Bạn mới tập một phần buổi này',
  },
  nghi_tap: { nhan: 'Nghỉ tập', kieu: 'loi' as KieuKetQua, moTa: 'Bạn đã nghỉ buổi này' },
} as const;

/** Chữ mô tả một bài: số set và số lần, ví dụ "4 set × 8–10 rep". */
export function nhanBaiTap(bai: BaiTap): string {
  return `${bai.soSet} set × ${bai.soLan}`;
}

/** Thẻ buổi tập theo kế hoạch của PT: ảnh, tên buổi, giờ, nhóm cơ và thời lượng. */
export function TheBuoiTap({ keHoach }: { keHoach: KeHoachBuoiTap }) {
  const { colors } = useTheme();
  const dong: { icon: string; chu: string }[] = [
    { icon: 'clock', chu: keHoach.gio },
    { icon: 'target', chu: keHoach.nhomCo },
    { icon: 'dumbbell', chu: `${keHoach.thoiLuongPhut} phút` },
  ];
  return (
    <View className="flex-row gap-3 rounded-2xl border border-border bg-card p-4">
      <View className="h-28 w-36 overflow-hidden rounded-xl bg-secondary">
        {keHoach.anh ? (
          <Image
            source={{ uri: keHoach.anh }}
            accessibilityLabel={`Ảnh buổi tập ${keHoach.ten}`}
            resizeMode="cover"
            className="size-full"
          />
        ) : (
          <View className="size-full items-center justify-center">
            <Icon name="dumbbell" size={36} color={colors.muted} />
          </View>
        )}
      </View>
      <View className="flex-1 justify-center gap-1.5">
        <Text accessibilityRole="header" className="text-xl font-bold leading-7">
          {keHoach.ten}
        </Text>
        {dong.map((d) => (
          <View key={d.icon} className="flex-row items-center gap-2">
            <Icon name={d.icon} size={16} color={colors.text} />
            <Text className="flex-1 text-sm">{d.chu}</Text>
          </View>
        ))}
      </View>
    </View>
  );
}

/** Vòng tròn tick: rỗng khi chưa xong, đặc kèm dấu check khi đã xong. */
function OTick({ daXong }: { daXong: boolean }) {
  const { colors } = useTheme();
  return (
    <View
      className={cn(
        'size-7 items-center justify-center rounded-full border-2',
        daXong ? 'border-primary bg-primary' : 'border-input',
      )}
    >
      {daXong ? <Icon name="check" size={16} color={colors.onPrimary} /> : null}
    </View>
  );
}

function AnhBaiTap({ baiTap }: { baiTap: BaiTap }) {
  const { colors } = useTheme();
  return (
    <View className="size-12 items-center justify-center overflow-hidden rounded-lg bg-secondary">
      {baiTap.anh ? (
        <Image
          source={{ uri: baiTap.anh }}
          accessibilityLabel={baiTap.ten}
          resizeMode="cover"
          className="size-full"
        />
      ) : (
        <Icon name="dumbbell" size={22} color={colors.primary} />
      )}
    </View>
  );
}

/**
 * Danh sách bài tập trong một thẻ, mỗi dòng có ảnh nhỏ, tên, số hiệp x số lần và vòng tick. Có `onDoi` thì bấm cả dòng để
 * tick hoặc bỏ tick (form check-in); không có thì chỉ xem (màn chi tiết).
 */
export function DanhSachBaiTap({
  baiTap,
  daXong,
  onDoi,
}: {
  baiTap: readonly BaiTap[];
  daXong: readonly string[];
  onDoi?: (id: string) => void;
}) {
  return (
    <View className="rounded-2xl border border-border bg-card px-4">
      {baiTap.map((bai, i) => {
        const xong = daXong.includes(bai.id);
        const noiDung = (
          <>
            <AnhBaiTap baiTap={bai} />
            <View className="flex-1">
              <Text className="font-bold">{bai.ten}</Text>
              <Text variant="muted" className="text-sm">
                {nhanBaiTap(bai)}
              </Text>
            </View>
            <OTick daXong={xong} />
          </>
        );
        const lop = 'flex-row items-center gap-3 py-3';
        // Nút hướng dẫn nằm ngoài dòng tick để bấm nó không đổi trạng thái bài.
        return (
          <View key={bai.id} className={cn(i > 0 && 'border-t border-border')}>
            {onDoi ? (
              <Pressable
                accessibilityRole="checkbox"
                accessibilityState={{ checked: xong }}
                accessibilityLabel={`${bai.ten} ${nhanBaiTap(bai)}`}
                onPress={() => onDoi(bai.id)}
                className={lop}
              >
                {noiDung}
              </Pressable>
            ) : (
              <View className={lop}>{noiDung}</View>
            )}
            <View className="pl-[60px]">
              <NutHuongDan tenBai={bai.ten} link={bai.link} />
            </View>
          </View>
        );
      })}
    </View>
  );
}
