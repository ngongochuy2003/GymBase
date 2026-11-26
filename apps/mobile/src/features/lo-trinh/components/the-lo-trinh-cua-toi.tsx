import { Pressable, View } from 'react-native';
import { useRouter } from 'expo-router';
import { Text } from '@/components/ui/text';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { LoTrinhCuaToi } from '../types';
import { ngayNganGon } from '../utils/ngay';
import { AnhDaiDien, AnhVuong } from './anh-nguoi';
import { NhanTrangThai, ThanhTienDo } from './thanh-phan-nho';

/** Thẻ lộ trình ở sub-tab "Lộ trình của tôi" (HV-01). */
export function TheLoTrinhCuaToi({ loTrinh }: { loTrinh: LoTrinhCuaToi }) {
  const { colors } = useTheme();
  const router = useRouter();
  const dangChay = loTrinh.trangThai === 'dang_hoat_dong';
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={loTrinh.ten}
      onPress={() => router.push({ pathname: '/chi-tiet-lo-trinh', params: { id: loTrinh.id } })}
      className="gap-3 rounded-3xl border border-border bg-card p-4"
    >
      <View className="flex-row items-center gap-3">
        <AnhVuong
          uri={loTrinh.anhBia}
          ten={loTrinh.ten}
          className="h-[96px] w-[96px] rounded-2xl"
        />
        <View className="flex-1 gap-2">
          <Text className="text-lg font-bold leading-6 text-foreground" numberOfLines={2}>
            {loTrinh.ten}
          </Text>
          <View className="flex-row items-center gap-1.5">
            <AnhDaiDien uri={loTrinh.pt.anh} ten={loTrinh.pt.ten} className="size-5" />
            <Text className="flex-1 text-sm text-muted-foreground" numberOfLines={1}>
              PT {loTrinh.pt.ten}
            </Text>
          </View>
          <View className="flex-row items-center gap-1.5">
            <Icon name="calendar" size={16} color={colors.muted} />
            <Text className="flex-1 text-sm text-muted-foreground" numberOfLines={1}>
              {ngayNganGon(loTrinh.ngayBatDau)} – {ngayNganGon(loTrinh.ngayKetThuc)} (
              {loTrinh.tongTuan} tuần)
            </Text>
          </View>
          {dangChay ? (
            <View className="self-start rounded-full bg-success/15 px-3 py-1">
              <Text className="text-sm font-medium text-primary">
                Tuần {loTrinh.tuanHienTai} / {loTrinh.tongTuan}
              </Text>
            </View>
          ) : (
            <NhanTrangThai trangThai={loTrinh.trangThai} />
          )}
        </View>
        <Icon name="chevron-right" size={22} color={colors.text} />
      </View>
      <View className="flex-row items-center gap-3">
        <ThanhTienDo phanTram={loTrinh.tienDo} />
        <Text className="w-10 text-right text-sm font-bold text-foreground">{loTrinh.tienDo}%</Text>
      </View>
    </Pressable>
  );
}
