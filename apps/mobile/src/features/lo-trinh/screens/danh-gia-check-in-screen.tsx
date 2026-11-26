import { useState } from 'react';
import { Image, KeyboardAvoidingView, Platform, ScrollView, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { TextField } from '@/shared/components/ui';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import { AnhDaiDien } from '../components/anh-nguoi';
import { DauManCheckIn, TheBuaAn } from '../components/check-in-bua-an';
import { DanhSachBaiTap, KET_QUA_BUOI_TAP, TheBuoiTap } from '../components/check-in-buoi-tap';
import { KhoiDanhGiaPt } from '../components/chi-tiet-tien-do';
import { KhungThe } from '../components/khung-the';
import { VienNhan } from '../components/quan-ly/thanh-phan';
import { guiDanhGia, phienBanHienTai } from '../data/danh-gia-check-in-store';
import { useCheckInQuanLy } from '../hooks/use-lo-trinh';
import type { CheckInQuanLy } from '../types';

/** Độ dài tối đa của nhận xét (cùng giới hạn N6 với các ghi chú khác). */
const TOI_DA_KY_TU_NHAN_XET = 300;
/** Số ô ảnh trên một hàng (khớp số ảnh tối đa của check-in bữa ăn). */
const SO_O_ANH = 3;

/** Ô báo kết quả học viên đã báo: xanh khi hoàn thành, cam khi làm một phần, đỏ khi bỏ bữa hoặc nghỉ tập. */
function KetQuaHocVien({
  kieu,
  tieuDe,
  moTa,
}: {
  kieu: 'thanh_cong' | 'canh_bao' | 'loi';
  tieuDe: string;
  moTa: string;
}) {
  const { colors } = useTheme();
  const k = {
    thanh_cong: { nen: 'bg-success/15', icon: 'check', nenIcon: 'bg-primary', chu: 'text-success' },
    canh_bao: {
      nen: 'bg-warning/15',
      icon: 'circle-half',
      nenIcon: 'bg-warning',
      chu: 'text-warning',
    },
    loi: {
      nen: 'bg-destructive/15',
      icon: 'close',
      nenIcon: 'bg-destructive',
      chu: 'text-destructive',
    },
  }[kieu];
  return (
    <View className={cn('flex-row items-center gap-3 rounded-2xl p-4', k.nen)}>
      <View className={cn('size-12 items-center justify-center rounded-full', k.nenIcon)}>
        <Icon name={k.icon} size={26} color={colors.onPrimary} />
      </View>
      <View className="flex-1">
        <Text className={cn('text-lg font-bold', k.chu)}>{tieuDe}</Text>
        <Text variant="muted" className="text-sm">
          {moTa}
        </Text>
      </View>
    </View>
  );
}

/** Học viên gửi gì: người gửi, ảnh và ghi chú. Bữa ăn có tối đa 3 ảnh; buổi tập có một ảnh. */
function KhoiHocVienGui({
  checkIn,
  anh,
  nhanAnh,
  ghiChu,
  gioGui,
}: {
  checkIn: CheckInQuanLy;
  anh: readonly string[];
  nhanAnh: string;
  ghiChu: string;
  gioGui: string;
}) {
  return (
    <KhungThe icon="image" tieuDe="Học viên gửi">
      <View className="flex-row items-center gap-3">
        <AnhDaiDien uri={checkIn.hocVien.anh} ten={checkIn.hocVien.ten} className="size-10" />
        <View className="flex-1">
          <Text className="font-bold">{checkIn.hocVien.ten}</Text>
          <Text variant="muted" className="text-sm">
            Gửi lúc {gioGui}
          </Text>
        </View>
      </View>
      {anh.length > 0 ? (
        <View className="flex-row gap-3">
          {anh.map((uri) => (
            <Image
              key={uri}
              source={{ uri }}
              accessibilityLabel={nhanAnh}
              resizeMode="cover"
              className="aspect-square flex-1 rounded-xl bg-secondary"
            />
          ))}
          {Array.from({ length: Math.max(0, SO_O_ANH - anh.length) }, (_, i) => (
            <View key={`trong-${i}`} className="aspect-square flex-1" />
          ))}
        </View>
      ) : (
        <Text variant="muted" className="text-sm">
          Học viên không gửi ảnh nào.
        </Text>
      )}
      <View className="rounded-2xl bg-secondary p-3">
        <Text variant="muted" className="text-xs">
          Ghi chú của học viên
        </Text>
        <Text className="leading-6">{ghiChu || 'Học viên không ghi chú gì.'}</Text>
      </View>
    </KhungThe>
  );
}

/**
 * PT-04 Đánh giá check-in bữa ăn hoặc buổi tập của một học viên: kế hoạch lúc check-in (không phải kế hoạch mới sau khi sửa,
 * K5), kết quả học viên báo, ảnh và ghi chú, rồi ô "Nhận xét của PT" với ba nút. "Duyệt" cho nhận xét tùy chọn; "Yêu cầu bổ
 * sung" bắt buộc nhận xét để học viên biết cần bổ sung gì (N13). Gửi xong quay về PT-03, nơi hiện banner "Đã gửi đánh giá cho
 * học viên". Check-in đã duyệt hoặc đang chờ bổ sung, và lộ trình đã đóng, chỉ xem. Học viên sửa trong lúc PT đang xem thì
 * không ghi được đánh giá (phiên bản đã cũ).
 */
// TODO(ROUTE): PT-04 chỉ có thiết kế cho bữa ăn; buổi tập dựng theo màn chi tiết check-in buổi tập của học viên. "Nhắn tin
//   học viên" chưa có màn chat; PT duyệt xong thì màn học viên (HV-05) chưa tự đổi vì dữ liệu mẫu hai phía tách nhau.
export function DanhGiaCheckInScreen({
  loTrinhId,
  checkInId,
}: {
  loTrinhId?: string;
  checkInId?: string;
}) {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { colors } = useTheme();
  const du = useCheckInQuanLy(loTrinhId, checkInId);
  const [nhanXet, setNhanXet] = useState('');
  const [loi, setLoi] = useState<string>();
  // Phiên bản PT đã mở xem; giữ lại để phát hiện học viên sửa sau đó.
  const [phienBanDaXem, setPhienBanDaXem] = useState(du?.checkIn.phienBan ?? 0);

  const checkIn = du?.checkIn;
  const dangChay = du?.chiTiet.loTrinh.trangThai === 'dang_hoat_dong';
  const choDuyet = checkIn?.trangThai === 'cho_duyet';
  const duocDanhGia = Boolean(choDuyet && dangChay);

  const gui = (ketQua: 'da_duyet' | 'can_bo_sung') => {
    if (!du || !checkIn) return;
    const loiNhanXet = nhanXet.trim();
    if (ketQua === 'can_bo_sung' && !loiNhanXet) {
      setLoi('Nhập nội dung cần học viên bổ sung');
      return;
    }
    const kq = guiDanhGia(
      checkIn,
      { loTrinhId: du.chiTiet.loTrinh.id, trangThai: ketQua, nhanXet: loiNhanXet },
      phienBanDaXem,
    );
    if (kq === 'da_doi') {
      setPhienBanDaXem(phienBanHienTai(checkIn));
      setLoi('Học viên vừa sửa check-in này. Hãy xem lại nội dung mới rồi đánh giá.');
      return;
    }
    if (kq === 'da_xu_ly') {
      setLoi('Check-in này đã được đánh giá.');
      return;
    }
    router.back();
  };

  const tieuDeMan = duocDanhGia ? 'Đánh giá check-in' : 'Chi tiết check-in';
  const buaCheckIn = checkIn?.bua?.checkIn;
  const buoiCheckIn = checkIn?.buoiTap?.checkIn;

  return (
    <KeyboardAvoidingView
      className="flex-1 bg-background"
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      style={{ paddingTop: insets.top }}
    >
      <DauManCheckIn
        tieuDe={tieuDeMan}
        phuDe={
          du && checkIn
            ? `${checkIn.hocVien.ten} • ${du.chiTiet.loTrinh.ten}`
            : 'Không tìm thấy check-in'
        }
        onQuayLai={() => router.back()}
      />
      {du && checkIn ? (
        <>
          <ScrollView
            className="flex-1"
            keyboardShouldPersistTaps="handled"
            contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4"
            contentContainerStyle={{ paddingBottom: 24 }}
          >
            {checkIn.bua && buaCheckIn ? (
              <>
                <TheBuaAn keHoach={checkIn.bua.keHoach} />
                <KetQuaHocVien
                  kieu={buaCheckIn.trangThaiBua === 'da_an' ? 'thanh_cong' : 'loi'}
                  tieuDe={buaCheckIn.trangThaiBua === 'da_an' ? 'Đã ăn' : 'Bỏ bữa'}
                  moTa="Kết quả học viên báo"
                />
                <KhoiHocVienGui
                  checkIn={checkIn}
                  anh={buaCheckIn.anh}
                  nhanAnh="Ảnh bữa ăn học viên gửi"
                  ghiChu={buaCheckIn.ghiChu}
                  gioGui={buaCheckIn.gioGui}
                />
              </>
            ) : null}

            {checkIn.buoiTap && buoiCheckIn ? (
              <>
                <TheBuoiTap keHoach={checkIn.buoiTap.keHoach} />
                <KetQuaHocVien
                  kieu={KET_QUA_BUOI_TAP[buoiCheckIn.trangThaiBuoi].kieu}
                  tieuDe={KET_QUA_BUOI_TAP[buoiCheckIn.trangThaiBuoi].nhan}
                  moTa="Kết quả học viên báo"
                />
                <View className="gap-1">
                  <Text accessibilityRole="header" className="text-xl font-bold">
                    Bài tập
                  </Text>
                  <Text variant="muted" className="text-sm">
                    Đã hoàn thành {buoiCheckIn.baiTapXong.length}/
                    {checkIn.buoiTap.keHoach.baiTap.length} bài
                  </Text>
                </View>
                <DanhSachBaiTap
                  baiTap={checkIn.buoiTap.keHoach.baiTap}
                  daXong={buoiCheckIn.baiTapXong}
                />
                <KhoiHocVienGui
                  checkIn={checkIn}
                  anh={buoiCheckIn.anh ? [buoiCheckIn.anh] : []}
                  nhanAnh="Ảnh buổi tập học viên gửi"
                  ghiChu={buoiCheckIn.ghiChu}
                  gioGui={buoiCheckIn.gioGui}
                />
              </>
            ) : null}

            {duocDanhGia ? (
              <KhungThe
                icon="edit"
                tieuDe="Nhận xét của PT"
                tieuDePhu="(bắt buộc khi yêu cầu bổ sung)"
              >
                <View>
                  <TextField
                    hideLabel
                    multiline
                    label="Nhận xét của PT"
                    placeholder="Ví dụ: Khẩu phần ổn, đủ protein. Bữa sau giữ lượng rau như hôm nay nhé."
                    value={nhanXet}
                    onChangeText={(chu) => {
                      setNhanXet(chu);
                      setLoi(undefined);
                    }}
                    maxLength={TOI_DA_KY_TU_NHAN_XET}
                    textAlignVertical="top"
                    containerClassName="min-h-[96px] pb-8"
                  />
                  <Text variant="muted" className="absolute bottom-2 right-3 text-xs">
                    {nhanXet.length}/{TOI_DA_KY_TU_NHAN_XET}
                  </Text>
                </View>
                {loi ? (
                  <Text accessibilityRole="alert" variant="destructive" className="text-sm">
                    {loi}
                  </Text>
                ) : null}
              </KhungThe>
            ) : (
              <>
                {checkIn.nhanXet ? (
                  <KhoiDanhGiaPt
                    danhGia={checkIn.nhanXet}
                    tieuDe={
                      checkIn.trangThai === 'can_bo_sung'
                        ? 'Yêu cầu bổ sung đã gửi'
                        : 'Nhận xét của PT'
                    }
                  />
                ) : null}
                <View className="flex-row items-center gap-2 rounded-xl bg-secondary p-3">
                  <Icon name="lock" size={16} color={colors.muted} />
                  <Text variant="muted" className="flex-1 text-sm">
                    {!dangChay
                      ? 'Lộ trình đã đóng nên chỉ xem được.'
                      : checkIn.trangThai === 'can_bo_sung'
                        ? 'Đang chờ học viên bổ sung. Học viên gửi lại thì bạn đánh giá tiếp.'
                        : 'Check-in này đã được duyệt. Học viên không sửa được nữa.'}
                  </Text>
                  <VienNhan
                    nhan={checkIn.trangThai === 'can_bo_sung' ? 'Cần bổ sung' : 'Đã duyệt'}
                    nen={checkIn.trangThai === 'can_bo_sung' ? 'bg-info/15' : 'bg-success/15'}
                    chu={checkIn.trangThai === 'can_bo_sung' ? 'text-info' : 'text-success'}
                  />
                </View>
              </>
            )}
          </ScrollView>

          <View
            className="gap-2 border-t border-border bg-background px-4 pt-3"
            style={{ paddingBottom: 12 + insets.bottom }}
          >
            {duocDanhGia ? (
              <View className="flex-row gap-2">
                <Button
                  shape="pill"
                  accessibilityLabel="Duyệt"
                  onPress={() => gui('da_duyet')}
                  className="min-h-0 flex-1 border-0 px-0 py-3.5"
                >
                  <Icon name="check" size={18} color={colors.onPrimary} />
                  <Text>Duyệt</Text>
                </Button>
                <Button
                  variant="outline"
                  shape="pill"
                  accessibilityLabel="Yêu cầu bổ sung"
                  onPress={() => gui('can_bo_sung')}
                  className="min-h-0 flex-1 px-0 py-3.5"
                >
                  <Text>Yêu cầu bổ sung</Text>
                </Button>
              </View>
            ) : null}
            <Button
              variant="outline"
              shape="pill"
              accessibilityLabel="Nhắn tin học viên"
              // TODO(ROUTE): mở cuộc trò chuyện với học viên khi có màn tin nhắn (phần 15 của tài liệu thiết kế).
              onPress={() => undefined}
              className="min-h-0 px-0 py-3.5"
            >
              <Icon name="message" size={18} color={colors.text} />
              <Text>Nhắn tin học viên</Text>
            </Button>
          </View>
        </>
      ) : (
        <View className="items-center p-8">
          <Text variant="muted">Không tìm thấy check-in.</Text>
        </View>
      )}
    </KeyboardAvoidingView>
  );
}
