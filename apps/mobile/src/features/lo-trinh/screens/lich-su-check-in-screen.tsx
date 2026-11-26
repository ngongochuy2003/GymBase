import { useState } from 'react';
import { Image, Pressable, ScrollView, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Text } from '@/components/ui/text';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import { DauManCheckIn } from '../components/check-in-bua-an';
import { ChipLoc } from '../components/chip-loc';
import { NhanDuyet, VienNhan } from '../components/quan-ly/thanh-phan';
import { useLichSuCheckIn } from '../hooks/use-lo-trinh';
import type { LoaiCheckIn, MucLichSu } from '../types';
import { ngayNganGon } from '../utils/ngay';

type LocLoai = 'tat_ca' | LoaiCheckIn;
type LocKetQua = 'tat_ca' | 'da_check' | 'bo_lo';

const LOAI: readonly { gia: LocLoai; nhan: string }[] = [
  { gia: 'tat_ca', nhan: 'Tất cả' },
  { gia: 'bua_an', nhan: 'Bữa ăn' },
  { gia: 'buoi_tap', nhan: 'Buổi tập' },
];
const KET_QUA: readonly { gia: LocKetQua; nhan: string }[] = [
  { gia: 'tat_ca', nhan: 'Mọi kết quả' },
  { gia: 'da_check', nhan: 'Đã check' },
  { gia: 'bo_lo', nhan: 'Bỏ lỡ' },
];

function DongLichSu({ muc, onMo }: { muc: MucLichSu; onMo?(): void }) {
  const { colors } = useTheme();
  const nv = muc.nhiemVu;
  const checkIn = nv.buaAn?.checkIn ?? nv.buoiTap?.checkIn;
  const anh = nv.buaAn?.checkIn?.anh[0] ?? nv.buoiTap?.checkIn?.anh;
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={`${nv.tieuDe} ngày ${ngayNganGon(muc.ngay)}`}
      accessibilityState={{ disabled: !onMo }}
      disabled={!onMo}
      onPress={onMo}
      className="flex-row items-center gap-3 rounded-2xl border border-border bg-card p-3"
    >
      {anh ? (
        <Image source={{ uri: anh }} className="size-12 rounded-lg bg-secondary" />
      ) : (
        <View className="size-12 items-center justify-center rounded-lg bg-secondary">
          <Icon name={nv.loai === 'bua_an' ? 'meal' : 'dumbbell'} size={22} color={colors.text} />
        </View>
      )}
      <View className="min-w-0 flex-1">
        <Text className="text-base font-bold" numberOfLines={1}>
          {nv.tieuDe}
        </Text>
        <Text variant="muted" className="text-sm" numberOfLines={1}>
          {nv.moTa}
        </Text>
      </View>
      {nv.trangThai === 'khong_hoan_thanh' ? (
        <VienNhan nhan="Bỏ lỡ" nen="bg-destructive/15" chu="text-destructive" />
      ) : checkIn ? (
        <NhanDuyet trangThai={checkIn.trangThaiDuyet} />
      ) : null}
    </Pressable>
  );
}

/**
 * HV-07c Lịch sử check-in (N42): bữa ăn và buổi tập theo ngày, mới nhất ở trên, gồm cả mục bỏ lỡ. Mở từ hai ô chỉ số ở
 * chi tiết lộ trình, lọc sẵn theo ô đã bấm. Mục có check-in thì bấm để xem chi tiết.
 */
// TODO(ROUTE): HV-07c chưa có bản thiết kế; danh sách chưa phân trang.
export function LichSuCheckInScreen({ id, loai }: { id?: string; loai?: string }) {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const du = useLichSuCheckIn(id);
  const [locLoai, setLocLoai] = useState<LocLoai>(
    loai === 'bua_an' || loai === 'buoi_tap' ? loai : 'tat_ca',
  );
  const [locKetQua, setLocKetQua] = useState<LocKetQua>('tat_ca');

  const theoLoai = du?.muc.filter((m) => locLoai === 'tat_ca' || m.nhiemVu.loai === locLoai) ?? [];
  const daCheck = theoLoai.filter((m) => m.nhiemVu.trangThai === 'da_check').length;
  const danhSach = theoLoai.filter(
    (m) =>
      locKetQua === 'tat_ca' || (m.nhiemVu.trangThai === 'da_check') === (locKetQua === 'da_check'),
  );
  const cacNgay = [...new Set(danhSach.map((m) => m.ngay))];

  const mo = (m: MucLichSu) =>
    router.push({
      pathname: m.nhiemVu.loai === 'bua_an' ? '/chi-tiet-check-in' : '/chi-tiet-check-in-buoi-tap',
      params: { loTrinhId: id, nhiemVuId: m.nhiemVu.id },
    });

  return (
    <View className="flex-1 bg-background" style={{ paddingTop: insets.top }}>
      <DauManCheckIn
        tieuDe="Lịch sử check-in"
        phuDe={du ? du.loTrinh.ten : 'Không tìm thấy lộ trình'}
        onQuayLai={() => router.back()}
      />
      {du ? (
        <ScrollView
          className="flex-1"
          contentContainerClassName="w-full max-w-[680px] gap-3 self-center px-4"
          contentContainerStyle={{ paddingBottom: 24 + insets.bottom }}
        >
          <Text className="text-base font-bold">
            Đã check {daCheck}/{theoLoai.length}
          </Text>
          <ChipLoc nhom="loại" luaChon={LOAI} chon={locLoai} onChon={setLocLoai} />
          <ChipLoc nhom="kết quả" luaChon={KET_QUA} chon={locKetQua} onChon={setLocKetQua} />
          {danhSach.length === 0 ? (
            <Text variant="muted" className="text-center text-sm">
              Chưa có check-in nào ở mục này.
            </Text>
          ) : (
            cacNgay.map((ngay) => {
              const trongNgay = danhSach.filter((m) => m.ngay === ngay);
              return (
                <View key={ngay} className="gap-2">
                  <Text variant="muted" className="pt-2 text-sm font-medium">
                    {ngayNganGon(ngay)} • Tuần {trongNgay[0]?.tuan}
                  </Text>
                  {trongNgay.map((m) => (
                    <DongLichSu
                      key={m.nhiemVu.id}
                      muc={m}
                      onMo={
                        m.nhiemVu.buaAn?.checkIn || m.nhiemVu.buoiTap?.checkIn
                          ? () => mo(m)
                          : undefined
                      }
                    />
                  ))}
                </View>
              );
            })
          )}
        </ScrollView>
      ) : (
        <View className="items-center p-8">
          <Text variant="muted">Không tìm thấy lộ trình.</Text>
        </View>
      )}
    </View>
  );
}
