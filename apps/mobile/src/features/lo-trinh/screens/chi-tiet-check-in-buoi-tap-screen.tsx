import { Image, ScrollView, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import { DauManCheckIn } from '../components/check-in-bua-an';
import { DanhSachBaiTap, KET_QUA_BUOI_TAP, TheBuoiTap } from '../components/check-in-buoi-tap';
import { KhoiDanhGiaPt } from '../components/chi-tiet-tien-do';
import { KetQuaCheckIn } from '../components/ket-qua-check-in';
import { KhungThe } from '../components/khung-the';
import { useNhiemVuBuoiTap } from '../hooks/use-lo-trinh';
import { useToastCheckIn } from '../hooks/use-toast-check-in';

/**
 * Chi tiết check-in buổi tập đã gửi: thẻ buổi tập theo kế hoạch lúc gửi, kết quả (Đã tập, Tập một phần hoặc Nghỉ tập),
 * nhận xét của PT, các bài đã tick, ảnh và ghi chú. Học viên chỉnh sửa được khi check-in chờ duyệt hoặc cần bổ sung; sau
 * khi PT duyệt thì ẩn nút sửa (N13, cùng quy tắc với bữa ăn).
 */
// TODO(ROUTE): chưa có bản thiết kế cho check-in buổi tập đã gửi (luong-man-hinh.md, HV-06: "chưa có thiết kế cho trạng
//   thái PT đã đánh giá buổi tập"); màn này dựng theo khuôn chi tiết check-in bữa ăn.
export function ChiTietCheckInBuoiTapScreen({
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
  const checkIn = du?.buoiTap.checkIn;
  const toast = useToastCheckIn();

  return (
    <View className="flex-1 bg-background" style={{ paddingTop: insets.top }}>
      {du && checkIn ? (
        <>
          <DauManCheckIn
            tieuDe="Check-in buổi tập"
            phuDe={`Lộ trình: ${du.loTrinh.ten}`}
            onQuayLai={() => router.back()}
          />
          <ScrollView
            className="flex-1"
            contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4 pb-6"
          >
            <TheBuoiTap keHoach={du.buoiTap.keHoach} />

            <KetQuaCheckIn
              kieu={KET_QUA_BUOI_TAP[checkIn.trangThaiBuoi].kieu}
              tieuDe={KET_QUA_BUOI_TAP[checkIn.trangThaiBuoi].nhan}
              tieuDeDaDuyet="Buổi tập đã được duyệt"
              moTa={`${KET_QUA_BUOI_TAP[checkIn.trangThaiBuoi].moTa} • gửi lúc ${checkIn.gioGui}`}
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

            <View className="gap-1">
              <Text accessibilityRole="header" className="text-xl font-bold">
                {du.nhiemVu.id.startsWith('ls-') ? 'Bài tập' : 'Bài tập hôm nay'}
              </Text>
              <Text variant="muted" className="text-sm">
                Đã hoàn thành {checkIn.baiTapXong.length}/{du.buoiTap.keHoach.baiTap.length} bài
              </Text>
            </View>
            <DanhSachBaiTap baiTap={du.buoiTap.keHoach.baiTap} daXong={checkIn.baiTapXong} />

            <KhungThe icon="image" tieuDe="Ảnh buổi tập">
              {checkIn.anh ? (
                <Image
                  source={{ uri: checkIn.anh }}
                  accessibilityLabel="Ảnh buổi tập đã gửi"
                  resizeMode="cover"
                  className="aspect-[16/9] w-full rounded-xl bg-secondary"
                />
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
                    pathname: '/check-in-buoi-tap',
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
            tieuDe="Check-in buổi tập"
            phuDe={du ? `Lộ trình: ${du.loTrinh.ten}` : 'Không tìm thấy buổi tập'}
            onQuayLai={() => router.back()}
          />
          <View className="items-center p-8">
            <Text variant="muted">Chưa có check-in cho buổi tập này.</Text>
          </View>
        </>
      )}
      {toast}
    </View>
  );
}
