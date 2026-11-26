import { useState } from 'react';
import { KeyboardAvoidingView, Platform, Pressable, ScrollView, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { TextField } from '@/shared/components/ui';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import { DauManCheckIn } from '../components/check-in-bua-an';
import { DanhSachBaiTap, TheBuoiTap } from '../components/check-in-buoi-tap';
import { ChonMotAnh } from '../components/chon-mot-anh';
import { KhungThe } from '../components/khung-the';
import { luuCheckInBuoiTap } from '../data/check-in-store';
import { useChonMotAnh } from '../hooks/use-chon-mot-anh';
import { useNhiemVuBuoiTap } from '../hooks/use-lo-trinh';
import type { TrangThaiBuoiTap } from '../types';
import { gioHienTai } from '../utils/ngay';

/** Độ dài tối đa của ghi chú gửi PT (N6, cùng giới hạn với check-in bữa ăn). */
export const TOI_DA_KY_TU_GHI_CHU = 300;
/** Số ảnh buổi tập tối đa (HV-06: một ảnh, tùy chọn). */
export const TOI_DA_ANH = 1;
/** Ba lựa chọn trạng thái buổi tập. Chọn rồi thì ô đổi sang màu đặc theo sắc thái của kết quả. */
const LUA_CHON: readonly {
  gia: TrangThaiBuoiTap;
  nhan: string;
  icon: string;
  nenChon: string;
  chuChon: string;
}[] = [
  {
    gia: 'da_tap',
    nhan: 'Đã tập',
    icon: 'check-circle',
    nenChon: 'bg-primary',
    chuChon: 'text-primary-foreground',
  },
  {
    gia: 'tap_mot_phan',
    nhan: 'Tập một phần',
    icon: 'circle-half',
    nenChon: 'bg-warning',
    chuChon: 'text-primary-foreground',
  },
  {
    gia: 'nghi_tap',
    nhan: 'Nghỉ tập',
    icon: 'close-circle',
    nenChon: 'bg-destructive',
    chuChon: 'text-destructive-foreground',
  },
];

/**
 * Check-in buổi tập (HV-06): tick từng bài đã tập, chọn Đã tập, Tập một phần hoặc Nghỉ tập, ghi chú cho PT, một ảnh
 * tùy chọn rồi "Hoàn thành buổi tập". Mở từ nút "Check-in ngay" ở nhiệm vụ hôm nay, và từ "Chỉnh sửa check-in" ở màn
 * chi tiết (khi đó điền sẵn dữ liệu đã gửi).
 */
export function CheckInBuoiTapScreen({
  loTrinhId,
  nhiemVuId,
}: {
  loTrinhId?: string;
  nhiemVuId?: string;
}) {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { colors } = useTheme();
  const du = useNhiemVuBuoiTap(loTrinhId, nhiemVuId);
  const daGui = du?.buoiTap.checkIn;
  const [trangThaiBuoi, setTrangThaiBuoi] = useState<TrangThaiBuoiTap>(
    daGui?.trangThaiBuoi ?? 'da_tap',
  );
  const [baiTapXong, setBaiTapXong] = useState<string[]>(daGui?.baiTapXong ?? []);
  const [ghiChu, setGhiChu] = useState(daGui?.ghiChu ?? '');
  const { anh, loi: loiAnh, chup, thuVien, xoa } = useChonMotAnh(daGui?.anh, 'ảnh buổi tập');

  const doiTick = (id: string) =>
    setBaiTapXong((truoc) => (truoc.includes(id) ? truoc.filter((b) => b !== id) : [...truoc, id]));

  const gui = () => {
    if (!du) return;
    // TODO(ROUTE): gọi API gửi check-in kèm upload ảnh (shared/file), xử lý lỗi mạng; hỏi bỏ thay đổi khi thoát lúc nhập dở
    //   (luong-man-hinh.md, HV-06: chưa có thiết kế trạng thái PT đã đánh giá buổi tập).
    luuCheckInBuoiTap(
      du.nhiemVu.id,
      {
        trangThaiBuoi,
        // Nghỉ tập thì không có bài nào được tính, dù đã lỡ tick trước khi đổi trạng thái.
        baiTapXong: trangThaiBuoi === 'nghi_tap' ? [] : baiTapXong,
        ghiChu: ghiChu.trim(),
        anh,
        trangThaiDuyet: 'cho_duyet',
        gioGui: gioHienTai(),
      },
      'Đã gửi check-in buổi tập, PT sẽ xem và phản hồi sớm',
    );
    router.back();
  };

  const dangSua = Boolean(daGui);
  const soAnh = anh ? 1 : 0;

  return (
    <KeyboardAvoidingView
      className="flex-1 bg-background"
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      style={{ paddingTop: insets.top }}
    >
      {du ? (
        <>
          <DauManCheckIn
            tieuDe={dangSua ? 'Sửa check-in buổi tập' : 'Check-in buổi tập'}
            phuDe={`Lộ trình: ${du.loTrinh.ten}`}
            onQuayLai={() => router.back()}
          />
          <ScrollView
            className="flex-1"
            contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4"
            contentContainerStyle={{ paddingBottom: 24 + insets.bottom }}
            keyboardShouldPersistTaps="handled"
          >
            <TheBuoiTap keHoach={du.buoiTap.keHoach} />

            <Text accessibilityRole="header" className="text-xl font-bold">
              Bài tập hôm nay
            </Text>
            <DanhSachBaiTap
              baiTap={du.buoiTap.keHoach.baiTap}
              daXong={baiTapXong}
              onDoi={doiTick}
            />

            <Text accessibilityRole="header" className="text-xl font-bold">
              Trạng thái buổi tập
            </Text>
            <View accessibilityRole="radiogroup" className="flex-row gap-2">
              {LUA_CHON.map((lc) => {
                const dangChon = trangThaiBuoi === lc.gia;
                return (
                  <Pressable
                    key={lc.gia}
                    accessibilityRole="radio"
                    accessibilityState={{ checked: dangChon }}
                    accessibilityLabel={lc.nhan}
                    onPress={() => setTrangThaiBuoi(lc.gia)}
                    className={cn(
                      'flex-1 flex-row items-center justify-center gap-1.5 rounded-full border px-2 py-3',
                      dangChon ? cn('border-transparent', lc.nenChon) : 'border-border bg-card',
                    )}
                  >
                    <Icon
                      name={lc.icon}
                      size={18}
                      color={dangChon ? colors.onPrimary : colors.muted}
                    />
                    <Text
                      className={cn(
                        'text-sm font-medium',
                        dangChon ? lc.chuChon : 'text-muted-foreground',
                      )}
                      numberOfLines={1}
                    >
                      {lc.nhan}
                    </Text>
                  </Pressable>
                );
              })}
            </View>

            <KhungThe icon="edit" tieuDe="Ghi chú gửi PT" tieuDePhu="(không bắt buộc)">
              <View>
                <TextField
                  hideLabel
                  multiline
                  label="Ghi chú gửi PT"
                  placeholder="Ví dụ: Cảm giác ổn, tăng nhẹ mức tạ ở bài đầu."
                  value={ghiChu}
                  onChangeText={setGhiChu}
                  maxLength={TOI_DA_KY_TU_GHI_CHU}
                  textAlignVertical="top"
                  containerClassName="min-h-[110px] pb-8"
                />
                <Text variant="muted" className="absolute bottom-2 right-3 text-xs">
                  {ghiChu.length}/{TOI_DA_KY_TU_GHI_CHU}
                </Text>
              </View>
            </KhungThe>

            <KhungThe
              icon="image"
              tieuDe="Ảnh buổi tập"
              tieuDePhu="(tùy chọn)"
              chuPhu={`Đã thêm ${soAnh}/${TOI_DA_ANH} ảnh`}
            >
              <ChonMotAnh
                anh={anh}
                nhanAnh="Ảnh buổi tập đã thêm"
                onChup={chup}
                onThuVien={thuVien}
                onXoa={xoa}
              />
              {loiAnh ? (
                <Text accessibilityRole="alert" variant="destructive" className="text-sm">
                  {loiAnh}
                </Text>
              ) : null}
            </KhungThe>

            <Button
              shape="pill"
              accessibilityLabel={dangSua ? 'Gửi lại cho PT' : 'Hoàn thành buổi tập'}
              onPress={gui}
              className="min-h-[56px]"
            >
              <Text>{dangSua ? 'Gửi lại cho PT' : 'Hoàn thành buổi tập'}</Text>
            </Button>
          </ScrollView>
        </>
      ) : (
        <>
          <DauManCheckIn
            tieuDe="Check-in buổi tập"
            phuDe="Không tìm thấy buổi tập"
            onQuayLai={() => router.back()}
          />
          <View className="items-center p-8">
            <Text variant="muted">Không tìm thấy buổi tập cần check-in.</Text>
          </View>
        </>
      )}
    </KeyboardAvoidingView>
  );
}
