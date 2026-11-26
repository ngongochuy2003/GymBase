import { View } from 'react-native';
import { useRouter } from 'expo-router';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import type { LoiMoiLoTrinh } from '../types';
import { ngayNganGon } from '../utils/ngay';
import { AnhVuong } from './anh-nguoi';

/** Thẻ lời mời tham gia lộ trình, nằm trên các lộ trình đang thực hiện ở HV-01 (N23). */
export function TheLoiMoi({ loiMoi }: { loiMoi: LoiMoiLoTrinh }) {
  const router = useRouter();
  const { loTrinh } = loiMoi;
  return (
    <View className="gap-3 rounded-3xl border border-primary bg-card p-4">
      <View className="self-start rounded-full bg-success/15 px-3 py-1">
        <Text className="text-sm font-medium text-primary">Lời mời tham gia</Text>
      </View>
      <View className="flex-row items-center gap-3">
        <AnhVuong
          uri={loTrinh.anhBia}
          ten={loTrinh.ten}
          className="h-[72px] w-[72px] rounded-2xl"
        />
        <View className="flex-1 gap-1">
          <Text className="text-lg font-bold leading-6" numberOfLines={2}>
            {loTrinh.ten}
          </Text>
          <Text variant="muted" className="text-sm" numberOfLines={1}>
            PT {loTrinh.pt.ten} mời bạn
          </Text>
          <Text variant="muted" className="text-sm" numberOfLines={1}>
            {ngayNganGon(loTrinh.ngayBatDau)} – {ngayNganGon(loTrinh.ngayKetThuc)} (
            {loTrinh.tongTuan} tuần)
          </Text>
        </View>
      </View>
      <Button
        shape="pill"
        size="sm"
        accessibilityLabel={`Xem chi tiết lời mời ${loTrinh.ten}`}
        onPress={() => router.push({ pathname: '/loi-moi-lo-trinh', params: { id: loTrinh.id } })}
      >
        <Text>Xem chi tiết</Text>
      </Button>
    </View>
  );
}
