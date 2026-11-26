import { useState } from 'react';
import { Pressable, ScrollView, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import { AnhDaiDien, AnhVuong } from '../components/anh-nguoi';
import { HopThoaiLoiMoi } from '../components/hop-thoai-loi-moi';
import { KhungThe } from '../components/khung-the';
import { TabKeHoach } from '../components/quan-ly/tab-ke-hoach';
import { traLoiLoiMoi, type TraLoiLoiMoi } from '../data/loi-moi-store';
import { useLoiMoi } from '../hooks/use-lo-trinh';
import { ngayNganGon } from '../utils/ngay';

/**
 * Chi tiết lời mời tham gia lộ trình (N23): thông tin chung, lời nhắn của PT, mục tiêu, kế hoạch và việc theo dõi định kỳ.
 * Hai nút Từ chối, Tham gia nằm cuối nội dung cuộn (không ghim đáy màn) để học viên xem hết rồi mới trả lời.
 */
export function LoiMoiLoTrinhScreen({ id }: { id?: string }) {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { colors } = useTheme();
  const duLieu = useLoiMoi(id);
  const [hoi, setHoi] = useState<TraLoiLoiMoi | null>(null);
  const [tuan, setTuan] = useState(1);

  const dau = (
    <View className="flex-row items-center gap-3 px-4 py-3">
      <Pressable
        accessibilityRole="button"
        accessibilityLabel="Quay lại"
        hitSlop={8}
        onPress={() => router.back()}
      >
        <Icon name="back" size={28} color={colors.text} />
      </Pressable>
      <Text accessibilityRole="header" className="flex-1 text-2xl font-bold">
        Lời mời tham gia
      </Text>
    </View>
  );

  if (!duLieu) {
    return (
      <View className="flex-1 bg-background" style={{ paddingTop: insets.top }}>
        {dau}
        <View className="items-center p-8">
          <Text variant="muted">Không tìm thấy lời mời.</Text>
        </View>
      </View>
    );
  }

  const { loiMoi, traLoi } = duLieu;
  const { loTrinh } = loiMoi;
  return (
    <View className="flex-1 bg-background" style={{ paddingTop: insets.top }}>
      {dau}
      <ScrollView
        className="flex-1"
        contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4"
        contentContainerStyle={{ paddingBottom: 24 + insets.bottom }}
      >
        <View className="gap-3 rounded-2xl border border-border bg-card p-4">
          <AnhVuong uri={loTrinh.anhBia} ten={loTrinh.ten} className="h-40 w-full rounded-xl" />
          <Text className="text-xl font-bold">{loTrinh.ten}</Text>
          <View className="flex-row items-center gap-2">
            <AnhDaiDien uri={loTrinh.pt.anh} ten={loTrinh.pt.ten} className="size-8" />
            <Text className="flex-1 text-sm">PT {loTrinh.pt.ten}</Text>
          </View>
          <Text variant="muted" className="text-sm">
            {ngayNganGon(loTrinh.ngayBatDau)} – {ngayNganGon(loTrinh.ngayKetThuc)} (
            {loTrinh.tongTuan} tuần) • {loTrinh.loai === 'nhom' ? 'Lộ trình nhóm' : 'Lộ trình đơn'}
          </Text>
        </View>

        {loiMoi.loiNhan ? (
          <KhungThe icon="message" tieuDe="Lời nhắn từ PT">
            <Text className="text-sm leading-5">{loiMoi.loiNhan}</Text>
          </KhungThe>
        ) : null}

        <KhungThe icon="target" tieuDe="Mục tiêu">
          <Text className="text-base">
            Từ {loTrinh.mucTieu.tu} {loTrinh.mucTieu.donVi} → {loTrinh.mucTieu.den}{' '}
            {loTrinh.mucTieu.donVi}
          </Text>
        </KhungThe>

        <TabKeHoach
          chiTiet={{ keHoach: loiMoi.keHoach, tuanHienTai: tuan }}
          chonTuan={{ soTuan: loTrinh.tongTuan, onDoiTuan: setTuan }}
        />

        <KhungThe icon="scale" tieuDe="Theo dõi định kỳ">
          <Text className="text-sm leading-5">Cân nặng: {loiMoi.canMoiTuan} lần mỗi tuần</Text>
          <Text className="text-sm leading-5">Ảnh cơ thể: {loiMoi.chuKyAnhTuan} tuần một lần</Text>
        </KhungThe>

        {traLoi ? (
          <Text variant="muted" className="text-center text-sm">
            {traLoi === 'tham_gia'
              ? 'Bạn đã tham gia lộ trình này.'
              : 'Bạn đã từ chối lời mời này.'}
          </Text>
        ) : (
          <View className="flex-row gap-3">
            <Button
              variant="outline"
              shape="pill"
              accessibilityLabel="Từ chối"
              onPress={() => setHoi('tu_choi')}
              className="flex-2"
            >
              <Text>Từ chối</Text>
            </Button>
            <Button
              shape="pill"
              accessibilityLabel="Tham gia"
              onPress={() => setHoi('tham_gia')}
              className="flex-1"
            >
              <Text>Tham gia</Text>
            </Button>
          </View>
        )}
      </ScrollView>

      <HopThoaiLoiMoi
        traLoi={hoi}
        tenLoTrinh={loTrinh.ten}
        onHuy={() => setHoi(null)}
        onXacNhan={(chon) => {
          setHoi(null);
          traLoiLoiMoi(loTrinh.id, chon);
          router.back();
        }}
      />
    </View>
  );
}
