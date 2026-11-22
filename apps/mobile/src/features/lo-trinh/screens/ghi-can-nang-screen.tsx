import { useState } from 'react';
import {
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  TextInput,
  View,
} from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import { DauManCheckIn, GiaiThichPt } from '../components/check-in-bua-an';
import { ChonMotAnh } from '../components/chon-mot-anh';
import { DaGhi, TheNhiemVuGhi } from '../components/ghi-chi-so';
import { KhungThe } from '../components/khung-the';
import { luuCanNang } from '../data/check-in-store';
import { useChonMotAnh } from '../hooks/use-chon-mot-anh';
import { useNhiemVuCanNang } from '../hooks/use-lo-trinh';
import { gioHienTai } from '../utils/ngay';
import {
  CAN_NANG_TOI_DA,
  CAN_NANG_TOI_THIEU,
  chinhCanNang,
  docCanNang,
  soThapPhan,
} from '../utils/tien-do';

/** Mỗi lần bấm − hoặc + đổi bấy nhiêu kg. */
const BUOC_KG = 0.1;
/** Chỉ cho gõ tối đa 3 chữ số và 1 chữ số thập phân, dấu phẩy hoặc dấu chấm. */
const DANG_HOP_LE = /^\d{0,3}([.,]\d?)?$/;

function NutChinh({ icon, nhan, onPress }: { icon: string; nhan: string; onPress(): void }) {
  const { colors } = useTheme();
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={nhan}
      hitSlop={8}
      onPress={onPress}
      className="size-12 items-center justify-center rounded-full border border-input bg-card active:opacity-75"
    >
      <Icon name={icon} size={22} color={colors.text} />
    </Pressable>
  );
}

/**
 * Ghi cân nặng (HV-08): nhập số kg (gõ hoặc bấm − và +), xem chênh lệch so với lần cân trước và khoảng còn lại tới mục tiêu,
 * rồi lưu. Ghi trực tiếp, không gửi PT duyệt (N16): PT xem trong tiến độ. Đã ghi rồi thì mở lại với số đã ghi để sửa.
 */
// TODO(ROUTE): HV-08 chưa có bản thiết kế (luong-man-hinh.md, HV-08); màn này dựng theo khuôn các màn check-in.
export function GhiCanNangScreen({
  loTrinhId,
  nhiemVuId,
}: {
  loTrinhId?: string;
  nhiemVuId?: string;
}) {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const du = useNhiemVuCanNang(loTrinhId, nhiemVuId);
  const goc = du?.ghi?.kg ?? du?.lanTruoc?.kg ?? du?.loTrinh.mucTieu.tu ?? 60;
  const [nhap, setNhap] = useState(soThapPhan(goc));
  const [loi, setLoi] = useState<string>();
  const { anh, loi: loiAnh, chup, thuVien, xoa } = useChonMotAnh(du?.ghi?.anh, 'ảnh cân nặng');

  const kg = docCanNang(nhap);
  const mucTieu = du?.loTrinh.mucTieu;
  const lanTruoc = du?.lanTruoc;

  const doiChu = (chu: string) => {
    if (!DANG_HOP_LE.test(chu)) return;
    setNhap(chu);
    setLoi(undefined);
  };
  const chinh = (buoc: number) => {
    setNhap(chinhCanNang(nhap, buoc, goc));
    setLoi(undefined);
  };

  const luu = () => {
    if (!du) return;
    if (kg === null) {
      setLoi(`Nhập cân nặng từ ${CAN_NANG_TOI_THIEU} đến ${CAN_NANG_TOI_DA} kg`);
      return;
    }
    // TODO(ROUTE): gọi API ghi chi_so_co_the (cân nặng, ket_noi_pt_id của lộ trình, K7) rồi invalidate tiến độ; chưa có API.
    luuCanNang(
      du.nhiemVu.id,
      { kg, anh, gioGhi: gioHienTai() },
      `Đã ghi cân nặng ${soThapPhan(kg)} ${mucTieu?.donVi ?? 'kg'}`,
    );
    router.back();
  };

  /** Dòng so với lần cân trước; chưa có lần nào thì báo đây là lần đầu. */
  const dongSoSanh = (() => {
    if (kg === null) return null;
    if (!lanTruoc) return 'Đây là lần cân đầu tiên của lộ trình.';
    const lech = Math.round((kg - lanTruoc.kg) * 10) / 10;
    const sau = `so với lần cân trước (${soThapPhan(lanTruoc.kg)} kg, Tuần ${lanTruoc.tuan})`;
    if (lech === 0) return `Không đổi ${sau}`;
    return `${lech > 0 ? 'Tăng' : 'Giảm'} ${soThapPhan(Math.abs(lech))} kg ${sau}`;
  })();

  /** Dòng tới mục tiêu: còn bao nhiêu kg, hoặc đã đạt (đúng cho cả tăng cân lẫn giảm cân). */
  const dongMucTieu = (() => {
    if (kg === null || !mucTieu) return null;
    const tang = mucTieu.den >= mucTieu.tu;
    const daDat = tang ? kg >= mucTieu.den : kg <= mucTieu.den;
    if (daDat) return `Bạn đã đạt mục tiêu ${mucTieu.den} ${mucTieu.donVi}`;
    return `Còn ${soThapPhan(Math.abs(mucTieu.den - kg))} ${mucTieu.donVi} tới mục tiêu ${mucTieu.den} ${mucTieu.donVi}`;
  })();

  return (
    <KeyboardAvoidingView
      className="flex-1 bg-background"
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      style={{ paddingTop: insets.top }}
    >
      {du ? (
        <>
          <DauManCheckIn
            tieuDe="Ghi cân nặng"
            phuDe={`Lộ trình: ${du.loTrinh.ten}`}
            onQuayLai={() => router.back()}
          />
          <ScrollView
            className="flex-1"
            contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4"
            contentContainerStyle={{ paddingBottom: 24 + insets.bottom }}
            keyboardShouldPersistTaps="handled"
          >
            <TheNhiemVuGhi icon="scale" tieuDe={du.nhiemVu.tieuDe} moTa={du.nhiemVu.moTa} />

            {du.ghi ? (
              <DaGhi
                gioGhi={du.ghi.gioGhi}
                noiDung={`${soThapPhan(du.ghi.kg)} kg • bạn có thể sửa lại bên dưới`}
              />
            ) : null}

            <KhungThe icon="scale" tieuDe="Cân nặng hôm nay">
              <View className="flex-row items-center justify-between gap-3">
                <NutChinh
                  icon="minus"
                  nhan={`Giảm ${soThapPhan(BUOC_KG)} kg`}
                  onPress={() => chinh(-BUOC_KG)}
                />
                <View className="flex-1 flex-row items-end justify-center gap-1">
                  <TextInput
                    accessibilityLabel="Cân nặng (kg)"
                    value={nhap}
                    onChangeText={doiChu}
                    keyboardType="decimal-pad"
                    selectTextOnFocus
                    maxLength={5}
                    className="min-w-[96px] p-0 text-center text-5xl font-bold text-foreground"
                    style={{ lineHeight: undefined }}
                  />
                  <Text variant="muted" className="pb-2 text-lg font-medium">
                    kg
                  </Text>
                </View>
                <NutChinh
                  icon="plus"
                  nhan={`Tăng ${soThapPhan(BUOC_KG)} kg`}
                  onPress={() => chinh(BUOC_KG)}
                />
              </View>
              {loi ? (
                <Text
                  accessibilityRole="alert"
                  variant="destructive"
                  className="text-center text-sm"
                >
                  {loi}
                </Text>
              ) : (
                <View className="gap-0.5">
                  {dongSoSanh ? (
                    <Text variant="muted" className="text-center text-sm">
                      {dongSoSanh}
                    </Text>
                  ) : null}
                  {dongMucTieu ? (
                    <Text className="text-center text-sm font-medium text-primary">
                      {dongMucTieu}
                    </Text>
                  ) : null}
                </View>
              )}
            </KhungThe>

            <KhungThe
              icon="camera"
              tieuDe="Ảnh cân nặng"
              tieuDePhu="(tùy chọn)"
              chuPhu={`Đã thêm ${anh ? 1 : 0}/1 ảnh`}
            >
              <ChonMotAnh
                anh={anh}
                nhanAnh="Ảnh cân nặng đã thêm"
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
              accessibilityLabel={du.ghi ? 'Cập nhật cân nặng' : 'Lưu cân nặng'}
              onPress={luu}
              className="min-h-[56px]"
            >
              <Text>{du.ghi ? 'Cập nhật cân nặng' : 'Lưu cân nặng'}</Text>
            </Button>

            <GiaiThichPt
              tieuDe="PT xem trong tiến độ"
              noiDung="Cân nặng được ghi thẳng vào tiến độ của bạn, không cần PT duyệt."
            />
          </ScrollView>
        </>
      ) : (
        <>
          <DauManCheckIn
            tieuDe="Ghi cân nặng"
            phuDe="Không tìm thấy nhiệm vụ"
            onQuayLai={() => router.back()}
          />
          <View className="items-center p-8">
            <Text variant="muted">Không tìm thấy nhiệm vụ cân nặng.</Text>
          </View>
        </>
      )}
    </KeyboardAvoidingView>
  );
}
