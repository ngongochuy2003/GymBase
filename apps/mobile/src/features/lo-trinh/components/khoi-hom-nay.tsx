import { Pressable, View } from 'react-native';
import { useRouter } from 'expo-router';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { LoaiNhiemVu, NhiemVuHomNay, TrangThaiNhiemVu } from '../types';
import { nhanNgayDayDu } from '../utils/ngay';

const ICON_NHIEM_VU: Record<LoaiNhiemVu, string> = {
  bua_an: 'meal',
  buoi_tap: 'dumbbell',
  can_nang: 'scale',
  anh_co_the: 'camera',
};

function DauTrangThai({ trangThai }: { trangThai: TrangThaiNhiemVu }) {
  const { colors } = useTheme();
  if (trangThai === 'da_check') {
    return (
      <View className="h-8 w-8 items-center justify-center rounded-full bg-primary">
        <Icon name="check" size={18} color={colors.onPrimary} />
      </View>
    );
  }
  if (trangThai === 'khong_hoan_thanh') {
    return (
      <View className="h-8 w-8 items-center justify-center rounded-full bg-destructive">
        <Icon name="close" size={18} color={colors.onPrimary} />
      </View>
    );
  }
  return <View className="h-8 w-8 rounded-full border-2 border-border" />;
}

/** Khối "Nhiệm vụ hôm nay" dưới thẻ lộ trình đang thực hiện (HV-01). */
export function KhoiHomNay({
  loTrinhId,
  nhiemVu,
  homNay,
}: {
  loTrinhId: string;
  nhiemVu: NhiemVuHomNay[];
  homNay: Date;
}) {
  const { colors } = useTheme();
  const router = useRouter();

  /**
   * Nhiệm vụ chưa check-in mở form (bữa ăn HV-04, buổi tập HV-06); đã xong hoặc không hoàn thành thì mở chi tiết.
   */
  const moNhiemVu = (nv: NhiemVuHomNay) => {
    // TODO(ROUTE): nhiệm vụ "sắp tới" chưa tới giờ thì chưa mở.
    // Cân nặng và ảnh cơ thể ghi trực tiếp (N16): chưa ghi hay đã ghi đều mở cùng một màn, đã ghi thì điền sẵn để sửa.
    const duongDan = {
      bua_an: { form: '/check-in-bua-an', chiTiet: '/chi-tiet-check-in' },
      buoi_tap: { form: '/check-in-buoi-tap', chiTiet: '/chi-tiet-check-in-buoi-tap' },
      can_nang: { form: '/ghi-can-nang', chiTiet: '/ghi-can-nang' },
      anh_co_the: { form: '/ghi-anh-co-the', chiTiet: '/ghi-anh-co-the' },
    } as const;
    const params = { loTrinhId, nhiemVuId: nv.id };
    if (nv.trangThai === 'check_in_ngay') router.push({ pathname: duongDan[nv.loai].form, params });
    else if (nv.trangThai === 'da_check' || nv.trangThai === 'khong_hoan_thanh') {
      router.push({ pathname: duongDan[nv.loai].chiTiet, params });
    }
  };

  const soXong = nhiemVu.filter((nv) => nv.trangThai === 'da_check').length;
  return (
    <View className="gap-2">
      <View className="px-1">
        <Text className="text-xl font-bold text-foreground">Nhiệm vụ hôm nay</Text>
        <View className="mt-0.5 flex-row items-center justify-between">
          <Text className="text-sm text-muted-foreground">{nhanNgayDayDu(homNay)}</Text>
          <Text className="text-sm text-muted-foreground">
            {soXong}/{nhiemVu.length} hoàn thành
          </Text>
        </View>
      </View>
      <View>
        {nhiemVu.map((nv, i) => {
          const checkInNgay = nv.trangThai === 'check_in_ngay';
          return (
            <Pressable
              key={nv.id}
              accessibilityRole="button"
              accessibilityLabel={`${nv.tieuDe}, ${nv.moTa}`}
              onPress={() => moNhiemVu(nv)}
              className={cn(
                'flex-row items-center gap-3 px-3 py-3',
                checkInNgay
                  ? 'rounded-2xl border border-primary bg-card'
                  : i > 0 && 'border-t border-border',
              )}
            >
              <View className="h-12 w-12 items-center justify-center rounded-full bg-secondary">
                <Icon name={ICON_NHIEM_VU[nv.loai]} size={22} color={colors.text} />
              </View>
              <View className="flex-1">
                <Text className="text-base font-bold text-foreground">{nv.tieuDe}</Text>
                <Text className="text-sm text-muted-foreground" numberOfLines={1}>
                  {nv.moTa}
                </Text>
              </View>
              {checkInNgay ? (
                <View className="rounded-full bg-primary px-4 py-2">
                  <Text className="text-sm font-bold text-primary-foreground">
                    {nv.loai === 'bua_an' || nv.loai === 'buoi_tap' ? 'Check-in ngay' : 'Ghi ngay'}
                  </Text>
                </View>
              ) : (
                <DauTrangThai trangThai={nv.trangThai} />
              )}
            </Pressable>
          );
        })}
      </View>
    </View>
  );
}
