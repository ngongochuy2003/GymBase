import { Image, ScrollView, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import { KhoiDanhGiaPt } from '../components/chi-tiet-tien-do';
import { DauManCheckIn, TheBuaAn } from '../components/check-in-bua-an';
import { KetQuaCheckIn } from '../components/ket-qua-check-in';
import { KhungThe } from '../components/khung-the';
import { useNhiemVuBuaAn } from '../hooks/use-lo-trinh';
import { useToastCheckIn } from '../hooks/use-toast-check-in';

/** Số ô ảnh trên một hàng (khớp số ảnh tối đa của form check-in). */
const SO_O_ANH = 3;

/**
 * Chi tiết check-in bữa ăn đã gửi (HV-05): thẻ bữa theo kế hoạch lúc gửi, trạng thái duyệt, kết quả (Đã ăn hoặc Bỏ bữa),
 * nhận xét của PT, ảnh và ghi chú. Học viên chỉnh sửa được khi check-in chờ duyệt hoặc cần bổ sung; sau khi PT duyệt thì
 * ẩn nút sửa (N13).
 */
// TODO(ROUTE): HV-05 chưa có bản thiết kế riêng, màn này dựng theo thẻ bữa ăn của HV-04 (luong-man-hinh.md, mục 8).
export function ChiTietCheckInBuaAnScreen({
  loTrinhId,
  nhiemVuId,
}: {
  loTrinhId?: string;
  nhiemVuId?: string;
}) {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { colors } = useTheme();
  const du = useNhiemVuBuaAn(loTrinhId, nhiemVuId);
  const checkIn = du?.buaAn.checkIn;
  const toast = useToastCheckIn();

  return (
    <View className="flex-1 bg-background" style={{ paddingTop: insets.top }}>
      {du && checkIn ? (
        <>
          <DauManCheckIn
            tieuDe={`Check-in ${du.buaAn.keHoach.ten.toLowerCase()}`}
            phuDe={`Lộ trình: ${du.loTrinh.ten}`}
            onQuayLai={() => router.back()}
          />
          <ScrollView
            className="flex-1"
            contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4 pb-6"
          >
            <TheBuaAn keHoach={du.buaAn.keHoach} />

            <KetQuaCheckIn
              kieu={checkIn.trangThaiBua === 'da_an' ? 'thanh_cong' : 'loi'}
              tieuDe={checkIn.trangThaiBua === 'da_an' ? 'Đã ăn' : 'Bỏ bữa'}
              tieuDeDaDuyet="Bữa ăn đã được duyệt"
              moTa={`${
                checkIn.trangThaiBua === 'da_an' ? 'Bạn đã ăn bữa này' : 'Bạn đã báo bỏ bữa này'
              } • gửi lúc ${checkIn.gioGui}`}
              trangThaiDuyet={checkIn.trangThaiDuyet}
            />

            {checkIn.danhGia ? (
              <KhoiDanhGiaPt
                danhGia={checkIn.danhGia}
                tieuDe={
                  checkIn.trangThaiDuyet === 'can_bo_sung'
                    ? 'Yêu cầu bổ sung từ PT'
                    : 'Đánh giá từ PT'
                }
              />
            ) : null}

            <KhungThe icon="image" tieuDe="Ảnh bữa ăn">
              {checkIn.anh.length > 0 ? (
                <View className="flex-row gap-3">
                  {checkIn.anh.map((uri) => (
                    <Image
                      key={uri}
                      source={{ uri }}
                      accessibilityLabel="Ảnh bữa ăn đã gửi"
                      resizeMode="cover"
                      className="aspect-square flex-1 rounded-xl bg-secondary"
                    />
                  ))}
                  {Array.from({ length: Math.max(0, SO_O_ANH - checkIn.anh.length) }, (_, i) => (
                    <View key={`trong-${i}`} className="aspect-square flex-1" />
                  ))}
                </View>
              ) : (
                <Text variant="muted" className="text-sm">
                  Bạn không gửi ảnh nào.
                </Text>
              )}
            </KhungThe>

            <KhungThe icon="edit" tieuDe="Ghi chú của bạn">
              {checkIn.ghiChu ? (
                <Text className="leading-6">{checkIn.ghiChu}</Text>
              ) : (
                <Text variant="muted" className="text-sm">
                  Bạn không ghi chú gì.
                </Text>
              )}
            </KhungThe>
          </ScrollView>

          <View
            className="gap-2 border-t border-border bg-background px-4 pt-3"
            style={{ paddingBottom: 12 + insets.bottom }}
          >
            {checkIn.trangThaiDuyet !== 'da_duyet' ? (
              <Button
                shape="pill"
                accessibilityLabel="Chỉnh sửa check-in"
                onPress={() =>
                  router.push({
                    pathname: '/check-in-bua-an',
                    params: { loTrinhId: du.loTrinh.id, nhiemVuId: du.nhiemVu.id },
                  })
                }
                className="min-h-0 border-0 px-0 py-3.5"
              >
                <Icon name="edit" size={18} color={colors.onPrimary} />
                <Text>Chỉnh sửa check-in</Text>
              </Button>
            ) : (
              <Text variant="muted" className="text-center text-xs">
                PT đã duyệt check-in này. Muốn đổi, hãy nhắn PT để mở lại.
              </Text>
            )}
            <Button
              variant="outline"
              shape="pill"
              accessibilityLabel="Nhắn PT"
              // TODO(ROUTE): mở cuộc trò chuyện với PT khi có màn tin nhắn (luong-man-hinh.md, HV-05).
              onPress={() => undefined}
              className="min-h-0 px-0 py-3.5"
            >
              <Icon name="message" size={18} color={colors.text} />
              <Text>Nhắn PT</Text>
            </Button>
          </View>
        </>
      ) : (
        <>
          <DauManCheckIn
            tieuDe="Check-in bữa ăn"
            phuDe={du ? `Lộ trình: ${du.loTrinh.ten}` : 'Không tìm thấy bữa ăn'}
            onQuayLai={() => router.back()}
          />
          <View className="items-center p-8">
            <Text variant="muted">Chưa có check-in cho bữa ăn này.</Text>
          </View>
        </>
      )}
      {toast}
    </View>
  );
}
