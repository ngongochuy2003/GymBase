import { useState } from 'react';
import {
  Alert,
  Image,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  View,
} from 'react-native';
import * as ImagePicker from 'expo-image-picker';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { TextField } from '@/shared/components/ui';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import { DauManCheckIn, GiaiThichPt, TheBuaAn } from '../components/check-in-bua-an';
import { KhungThe } from '../components/khung-the';
import { luuCheckIn } from '../data/check-in-store';
import { useNhiemVuBuaAn } from '../hooks/use-lo-trinh';
import type { TrangThaiBuaAn } from '../types';
import { gioHienTai } from '../utils/ngay';

/** Số ảnh bữa ăn tối đa (HV-04). */
export const TOI_DA_ANH = 3;
/** Độ dài tối đa của ghi chú gửi PT (N6). */
export const TOI_DA_KY_TU_GHI_CHU = 300;

/** Lựa chọn trạng thái bữa ăn (N3: chỉ hai giá trị). Chọn rồi thì ô đổi sang màu đặc. */
const LUA_CHON: readonly {
  gia: TrangThaiBuaAn;
  nhan: string;
  icon: string;
  nenChon: string;
  chuChon: string;
}[] = [
  {
    gia: 'da_an',
    nhan: 'Đã ăn',
    icon: 'check-circle',
    nenChon: 'bg-primary',
    chuChon: 'text-primary-foreground',
  },
  {
    gia: 'bo_bua',
    nhan: 'Bỏ bữa',
    icon: 'close-circle',
    nenChon: 'bg-destructive',
    chuChon: 'text-destructive-foreground',
  },
];

const O_ANH = 'aspect-square flex-1';

/**
 * Check-in bữa ăn (HV-04): chọn Đã ăn hoặc Bỏ bữa (bắt buộc), thêm tối đa 3 ảnh, ghi chú gửi PT, rồi gửi. Mở từ nút
 * "Check-in ngay" ở nhiệm vụ hôm nay, và từ "Chỉnh sửa check-in" ở HV-05 (khi đó điền sẵn dữ liệu đã gửi).
 */
export function CheckInBuaAnScreen({
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
  const daGui = du?.buaAn.checkIn;
  const [trangThaiBua, setTrangThaiBua] = useState<TrangThaiBuaAn | null>(
    daGui?.trangThaiBua ?? null,
  );
  const [anh, setAnh] = useState<string[]>(daGui?.anh ?? []);
  const [ghiChu, setGhiChu] = useState(daGui?.ghiChu ?? '');
  const [loiTrangThai, setLoiTrangThai] = useState<string>();
  const [loiAnh, setLoiAnh] = useState<string>();

  const themAnh = async (nguon: 'camera' | 'thu_vien') => {
    setLoiAnh(undefined);
    if (anh.length >= TOI_DA_ANH) return;
    const quyen =
      nguon === 'camera'
        ? await ImagePicker.requestCameraPermissionsAsync()
        : await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (!quyen.granted) {
      setLoiAnh(
        nguon === 'camera'
          ? 'GymBase cần quyền camera để chụp ảnh bữa ăn.'
          : 'GymBase cần quyền truy cập thư viện ảnh để chọn ảnh bữa ăn.',
      );
      return;
    }
    const ketQua =
      nguon === 'camera'
        ? await ImagePicker.launchCameraAsync({ mediaTypes: ['images'], quality: 0.8 })
        : await ImagePicker.launchImageLibraryAsync({
            mediaTypes: ['images'],
            allowsMultipleSelection: true,
            selectionLimit: TOI_DA_ANH - anh.length,
            quality: 0.8,
          });
    if (ketQua.canceled) return;
    setAnh((truoc) => [...truoc, ...ketQua.assets.map((a) => a.uri)].slice(0, TOI_DA_ANH));
  };

  /** Còn đúng một chỗ trống thì chỉ có một ô "Thêm ảnh", bấm vào hỏi chụp hay chọn từ thư viện. */
  const hoiNguonAnh = () =>
    Alert.alert('Thêm ảnh bữa ăn', undefined, [
      { text: 'Chụp ảnh', onPress: () => void themAnh('camera') },
      { text: 'Chọn từ thư viện', onPress: () => void themAnh('thu_vien') },
      { text: 'Hủy', style: 'cancel' },
    ]);

  const gui = () => {
    if (!du) return;
    if (!trangThaiBua) {
      setLoiTrangThai('Chọn trạng thái bữa ăn');
      return;
    }
    // TODO(ROUTE): gọi API gửi check-in kèm upload ảnh (shared/file), xử lý lỗi mạng; hỏi bỏ thay đổi khi thoát lúc nhập dở
    //   (luong-man-hinh.md, HV-04: chưa có thiết kế).
    luuCheckIn(
      du.nhiemVu.id,
      {
        trangThaiBua,
        anh,
        ghiChu: ghiChu.trim(),
        trangThaiDuyet: 'cho_duyet',
        gioGui: gioHienTai(),
      },
      `Đã gửi check-in ${du.buaAn.keHoach.ten.toLowerCase()}, PT sẽ xem và phản hồi sớm`,
    );
    router.back();
  };

  const dangSua = Boolean(daGui);
  const conTrong = TOI_DA_ANH - anh.length;

  return (
    <KeyboardAvoidingView
      className="flex-1 bg-background"
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      style={{ paddingTop: insets.top }}
    >
      {du ? (
        <>
          <DauManCheckIn
            tieuDe={`${dangSua ? 'Sửa check-in' : 'Check-in'} ${du.buaAn.keHoach.ten.toLowerCase()}`}
            phuDe={`Lộ trình: ${du.loTrinh.ten}`}
            onQuayLai={() => router.back()}
          />
          <ScrollView
            className="flex-1"
            contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4"
            contentContainerStyle={{ paddingBottom: 24 + insets.bottom }}
            keyboardShouldPersistTaps="handled"
          >
            <TheBuaAn keHoach={du.buaAn.keHoach} />

            <KhungThe icon="meal" tieuDe="Trạng thái bữa ăn">
              <View accessibilityRole="radiogroup" className="flex-row gap-3">
                {LUA_CHON.map((lc) => {
                  const dangChon = trangThaiBua === lc.gia;
                  return (
                    <Pressable
                      key={lc.gia}
                      accessibilityRole="radio"
                      accessibilityState={{ checked: dangChon }}
                      accessibilityLabel={lc.nhan}
                      onPress={() => {
                        setTrangThaiBua(lc.gia);
                        setLoiTrangThai(undefined);
                      }}
                      className={cn(
                        'flex-1 flex-row items-center justify-center gap-2 rounded-xl py-3.5',
                        dangChon ? lc.nenChon : 'bg-secondary',
                      )}
                    >
                      <Icon
                        name={lc.icon}
                        size={22}
                        color={dangChon ? colors.onPrimary : colors.muted}
                      />
                      <Text
                        className={cn('font-bold', dangChon ? lc.chuChon : 'text-muted-foreground')}
                      >
                        {lc.nhan}
                      </Text>
                    </Pressable>
                  );
                })}
              </View>
              {loiTrangThai ? (
                <Text accessibilityRole="alert" variant="destructive" className="text-sm">
                  {loiTrangThai}
                </Text>
              ) : null}
            </KhungThe>

            <KhungThe
              icon="image"
              tieuDe="Ảnh bữa ăn"
              chuPhu={`Đã thêm ${anh.length}/${TOI_DA_ANH} ảnh`}
            >
              <View className="flex-row gap-3">
                {anh.map((uri) => (
                  <View key={uri} className={cn(O_ANH, 'overflow-hidden rounded-xl bg-secondary')}>
                    <Image
                      source={{ uri }}
                      accessibilityLabel="Ảnh bữa ăn đã thêm"
                      resizeMode="cover"
                      className="size-full"
                    />
                    <Pressable
                      accessibilityRole="button"
                      accessibilityLabel="Xóa ảnh"
                      hitSlop={8}
                      onPress={() => setAnh((truoc) => truoc.filter((u) => u !== uri))}
                      className="absolute right-1.5 top-1.5 size-6 items-center justify-center rounded-full bg-background/90"
                    >
                      <Icon name="close" size={14} color={colors.text} />
                    </Pressable>
                  </View>
                ))}
                {conTrong >= 2 ? (
                  <>
                    <Pressable
                      accessibilityRole="button"
                      accessibilityLabel="Chụp ảnh"
                      onPress={() => void themAnh('camera')}
                      className={cn(
                        O_ANH,
                        'items-center justify-center gap-2 rounded-xl border border-dashed border-input',
                      )}
                    >
                      <Icon name="camera" size={26} color={colors.text} />
                      <Text className="text-sm">Chụp ảnh</Text>
                    </Pressable>
                    <Pressable
                      accessibilityRole="button"
                      accessibilityLabel="Chọn từ thư viện"
                      onPress={() => void themAnh('thu_vien')}
                      className={cn(
                        O_ANH,
                        'items-center justify-center gap-2 rounded-xl border border-dashed border-input',
                      )}
                    >
                      <Icon name="image" size={26} color={colors.text} />
                      <Text className="text-center text-sm">Chọn từ thư viện</Text>
                    </Pressable>
                  </>
                ) : null}
                {conTrong === 1 ? (
                  <Pressable
                    accessibilityRole="button"
                    accessibilityLabel="Thêm ảnh"
                    onPress={hoiNguonAnh}
                    className={cn(
                      O_ANH,
                      'items-center justify-center gap-2 rounded-xl border border-dashed border-input',
                    )}
                  >
                    <Icon name="camera" size={26} color={colors.text} />
                    <Text className="text-sm">Thêm ảnh</Text>
                  </Pressable>
                ) : null}
                {/* Giữ ô ảnh đã thêm không giãn ra khi còn ít chỗ trống. */}
                {Array.from({ length: Math.max(0, conTrong - 2) }, (_, i) => (
                  <View key={`trong-${i}`} className={O_ANH} />
                ))}
              </View>
              {loiAnh ? (
                <Text accessibilityRole="alert" variant="destructive" className="text-sm">
                  {loiAnh}
                </Text>
              ) : null}
            </KhungThe>

            <KhungThe icon="edit" tieuDe="Ghi chú gửi PT" tieuDePhu="(không bắt buộc)">
              <View>
                <TextField
                  hideLabel
                  multiline
                  label="Ghi chú gửi PT"
                  placeholder="Ví dụ: Mình dùng 150g ức gà, 1 bát cơm và nhiều rau hơn kế hoạch một chút."
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

            <Button
              shape="pill"
              accessibilityLabel={dangSua ? 'Gửi lại cho PT' : 'Gửi PT đánh giá'}
              onPress={gui}
              className="min-h-[56px]"
            >
              <Icon name="send" size={20} color={colors.onPrimary} />
              <Text>{dangSua ? 'Gửi lại cho PT' : 'Gửi PT đánh giá'}</Text>
            </Button>

            <GiaiThichPt />
          </ScrollView>
        </>
      ) : (
        <View className="items-center p-8">
          <Text variant="muted">Không tìm thấy bữa ăn cần check-in.</Text>
        </View>
      )}
    </KeyboardAvoidingView>
  );
}
