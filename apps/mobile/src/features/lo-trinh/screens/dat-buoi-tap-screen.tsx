import { Image, KeyboardAvoidingView, Platform, Pressable, ScrollView, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { TextField } from '@/shared/components/ui';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import { DauManCheckIn } from '../components/check-in-bua-an';
import { ChonGio } from '../components/form/chon-gio';
import { KhungThe } from '../components/khung-the';
import { datLichTap, layBanNhap, useBanNhapLoTrinh } from '../data/ban-nhap-lo-trinh-store';
import { layMotAnh } from '../hooks/use-chon-mot-anh';
import type { BaiTap, BuoiTapTuan } from '../types';
import {
  buoiTapCuaTuan,
  suaBuoiTapCuaTuan,
  taoIdBaiTap,
  xoaBuoiTapCuaTuan,
} from '../utils/lich-tap';
import { laLinkHopLe } from '../utils/link-huong-dan';

const THU_DAY_DU = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ nhật'];
const TOI_DA_CHU_SO = 3;

/** Chỉ giữ chữ số, tối đa `TOI_DA_CHU_SO`; rỗng thì trả 0. */
function docSo(chu: string): number {
  const so = chu.replace(/\D/g, '').slice(0, TOI_DA_CHU_SO);
  return so ? Number(so) : 0;
}

/** Nút nhỏ có biểu tượng cho dòng bài tập (đổi thứ tự, xóa). */
function NutIcon({
  icon,
  nhan,
  onPress,
  tat = false,
}: {
  icon: string;
  nhan: string;
  onPress(): void;
  tat?: boolean;
}) {
  const { colors } = useTheme();
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={nhan}
      accessibilityState={{ disabled: tat }}
      disabled={tat}
      hitSlop={6}
      onPress={onPress}
      className={cn(
        'size-9 items-center justify-center rounded-lg bg-secondary',
        tat && 'opacity-40',
      )}
    >
      <Icon name={icon} size={18} color={colors.text} />
    </Pressable>
  );
}

/** Một bài tập: tên, số set, số lần, ảnh minh họa, đổi thứ tự và xóa. */
function DongBaiTap({
  stt,
  bai,
  dau,
  cuoi,
  onSua,
  onXoa,
  onDoiCho,
}: {
  stt: number;
  bai: BaiTap;
  dau: boolean;
  cuoi: boolean;
  onSua(phan: Partial<BaiTap>): void;
  onXoa(): void;
  onDoiCho(huong: -1 | 1): void;
}) {
  const { colors } = useTheme();
  const layAnh = async () => {
    const ketQua = await layMotAnh('thu_vien', 'ảnh minh họa bài tập');
    if (ketQua.uri) onSua({ anh: ketQua.uri });
  };
  return (
    <View className={cn('gap-2 py-3', stt > 1 && 'border-t border-border')}>
      <View className="flex-row items-center gap-2">
        <Text className="w-6 text-base font-bold text-muted-foreground">{stt}</Text>
        <View className="min-w-0 flex-1">
          <TextField
            hideLabel
            label={`Tên bài tập ${stt}`}
            placeholder="Tên bài tập"
            value={bai.ten}
            onChangeText={(ten) => onSua({ ten })}
            maxLength={80}
            containerClassName="h-11 px-3"
          />
        </View>
        <NutIcon icon="close" nhan={`Xóa bài tập ${stt}`} onPress={onXoa} />
      </View>
      <View className="flex-row items-center gap-2 pl-8">
        <View className="w-14">
          <TextField
            hideLabel
            label={`Số set bài ${stt}`}
            placeholder="3"
            keyboardType="number-pad"
            value={bai.soSet > 0 ? String(bai.soSet) : ''}
            onChangeText={(chu) => onSua({ soSet: docSo(chu) })}
            containerClassName="h-11 px-3"
          />
        </View>
        <Text variant="muted" className="text-sm">
          set ×
        </Text>
        <View className="min-w-0 flex-1">
          <TextField
            hideLabel
            label={`Số lần bài ${stt}`}
            placeholder="8–10 rep"
            value={bai.soLan}
            onChangeText={(soLan) => onSua({ soLan })}
            maxLength={30}
            containerClassName="h-11 px-3"
          />
        </View>
      </View>
      <View className="flex-row items-center gap-2 pl-8">
        {bai.anh ? (
          <View className="size-9 overflow-hidden rounded-lg bg-secondary">
            <Image
              source={{ uri: bai.anh }}
              accessibilityLabel={`Ảnh minh họa bài ${stt}`}
              resizeMode="cover"
              className="size-full"
            />
          </View>
        ) : null}
        <Pressable
          accessibilityRole="button"
          accessibilityLabel={bai.anh ? `Đổi ảnh bài ${stt}` : `Thêm ảnh bài ${stt}`}
          onPress={() => void layAnh()}
          className="flex-row items-center gap-1.5 rounded-lg border border-dashed border-input px-3 py-2"
        >
          <Icon name="image" size={16} color={colors.muted} />
          <Text className="text-sm">{bai.anh ? 'Đổi ảnh' : 'Thêm ảnh minh họa'}</Text>
        </Pressable>
        {bai.anh ? (
          <NutIcon
            icon="close"
            nhan={`Bỏ ảnh bài ${stt}`}
            onPress={() => onSua({ anh: undefined })}
          />
        ) : null}
        <View className="flex-1" />
        <NutIcon
          icon="chevron-up"
          nhan={`Đưa bài ${stt} lên`}
          tat={dau}
          onPress={() => onDoiCho(-1)}
        />
        <NutIcon
          icon="chevron-down"
          nhan={`Đưa bài ${stt} xuống`}
          tat={cuoi}
          onPress={() => onDoiCho(1)}
        />
      </View>
      <View className="pl-8">
        <TextField
          hideLabel
          label={`Link hướng dẫn bài ${stt}`}
          placeholder="Link hướng dẫn (https://...), không bắt buộc"
          value={bai.link ?? ''}
          onChangeText={(link) => onSua({ link: link || undefined })}
          autoCapitalize="none"
          autoCorrect={false}
          keyboardType="url"
          maxLength={500}
          error={bai.link && !laLinkHopLe(bai.link) ? 'Link phải bắt đầu bằng https://' : undefined}
          containerClassName="h-11 px-3"
        />
      </View>
    </View>
  );
}

/**
 * PT-02b Chi tiết buổi tập (màn con của PT-02): PT đặt tên buổi, nhóm cơ, giờ, thời lượng và danh sách bài tập (số set,
 * số lần, ảnh minh họa, thứ tự) của buổi rơi vào `thu` trong tuần `tuan`. Sửa tuần 1 thì các tuần chưa sửa riêng đổi theo;
 * sửa tuần khác thì tách riêng tuần đó (N29). Sửa tới đâu ghi vào bản nháp của form tới đó nên không cần nút lưu.
 */
// TODO(ROUTE): PT-02b chưa có bản thiết kế (luong-man-hinh.md, PT-02b); màn này tự thiết kế theo khuôn các màn check-in.
//   Hướng dẫn mới là link (N41), chưa tải tệp video; ảnh minh họa chưa upload (shared/file), chỉ giữ đường dẫn cục bộ.
export function DatBuoiTapScreen({ tuan, thu }: { tuan?: string; thu?: string }) {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { colors } = useTheme();
  const { lichTap } = useBanNhapLoTrinh();
  const soTuan = Number(tuan) || 1;
  const soThu = Number(thu);
  const buoi = buoiTapCuaTuan(lichTap, soTuan).find((b) => b.thu === soThu);
  const daSuaRieng = soTuan > 1 && lichTap.rieng[soTuan] !== undefined;

  // Đọc bản nháp ngay lúc sửa để không ghi đè bằng state cũ của lần vẽ trước.
  const sua = (phan: Partial<BuoiTapTuan>) =>
    datLichTap(suaBuoiTapCuaTuan(layBanNhap().lichTap, soTuan, soThu, phan));
  const baiTap = buoi?.baiTap ?? [];
  const suaBai = (id: string, phan: Partial<BaiTap>) =>
    sua({ baiTap: baiTap.map((b) => (b.id === id ? { ...b, ...phan } : b)) });
  const doiCho = (vt: number, huong: -1 | 1) => {
    const ds = [...baiTap];
    const dich = vt + huong;
    const hien = ds[vt];
    const kia = ds[dich];
    if (!hien || !kia) return;
    ds[vt] = kia;
    ds[dich] = hien;
    sua({ baiTap: ds });
  };
  const themBai = () => {
    sua({
      baiTap: [
        ...baiTap,
        {
          id: taoIdBaiTap(),
          ten: '',
          soSet: 3,
          soLan: '10 rep',
        },
      ],
    });
  };
  const xoaBuoi = () => {
    datLichTap(xoaBuoiTapCuaTuan(layBanNhap().lichTap, soTuan, soThu));
    router.back();
  };

  return (
    <KeyboardAvoidingView
      className="flex-1 bg-background"
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      style={{ paddingTop: insets.top }}
    >
      <DauManCheckIn
        tieuDe="Chi tiết buổi tập"
        phuDe={buoi ? `Tuần ${soTuan} • ${THU_DAY_DU[soThu - 1] ?? ''}` : 'Không tìm thấy buổi tập'}
        onQuayLai={() => router.back()}
      />
      {buoi ? (
        <>
          <ScrollView
            className="flex-1"
            keyboardShouldPersistTaps="handled"
            contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4"
            contentContainerStyle={{ paddingBottom: 24 }}
          >
            {soTuan > 1 ? (
              <View className="flex-row items-center gap-2 rounded-xl bg-secondary px-3 py-2">
                <Icon name="calendar" size={18} color={colors.muted} />
                <Text className="flex-1 text-sm text-muted-foreground">
                  {daSuaRieng
                    ? `Tuần ${soTuan} đã sửa riêng, không còn theo Tuần 1.`
                    : `Tuần ${soTuan} đang theo Tuần 1. Sửa ở đây sẽ tách riêng tuần này.`}
                </Text>
              </View>
            ) : null}

            <KhungThe icon="dumbbell" tieuDe="Thông tin buổi tập">
              <View className="gap-3">
                <TextField
                  label="Tên buổi tập"
                  placeholder="Ví dụ: Upper A"
                  value={buoi.ten}
                  onChangeText={(ten) => sua({ ten })}
                  maxLength={60}
                  containerClassName="h-11 px-3"
                />
                <TextField
                  label="Nhóm cơ"
                  placeholder="Ví dụ: Ngực, vai, tay sau"
                  value={buoi.nhomCo ?? ''}
                  onChangeText={(nhomCo) => sua({ nhomCo })}
                  maxLength={80}
                  containerClassName="h-11 px-3"
                />
                <View className="flex-row items-center gap-3">
                  <Text className="flex-1 text-base">Giờ tập</Text>
                  <ChonGio
                    gio={buoi.gio}
                    tieuDe={`Giờ tập ${THU_DAY_DU[soThu - 1] ?? ''}`}
                    onDoi={(gio) => sua({ gio })}
                  />
                </View>
                <View className="flex-row items-center gap-3">
                  <Text className="flex-1 text-base">Thời lượng</Text>
                  <View className="w-20">
                    <TextField
                      hideLabel
                      label="Thời lượng buổi tập (phút)"
                      placeholder="60"
                      keyboardType="number-pad"
                      value={buoi.thoiLuongPhut ? String(buoi.thoiLuongPhut) : ''}
                      onChangeText={(chu) => sua({ thoiLuongPhut: docSo(chu) || undefined })}
                      containerClassName="h-11 px-3"
                    />
                  </View>
                  <Text variant="muted" className="text-base">
                    phút
                  </Text>
                </View>
              </View>
            </KhungThe>

            <KhungThe icon="clipboard" tieuDe="Bài tập" chuPhu={`${baiTap.length} bài`}>
              {baiTap.length === 0 ? (
                <Text variant="muted" className="text-sm">
                  Chưa có bài tập nào. Học viên sẽ không thấy gì để tập trong buổi này.
                </Text>
              ) : (
                <View>
                  {baiTap.map((bai, i) => (
                    <DongBaiTap
                      key={bai.id}
                      stt={i + 1}
                      bai={bai}
                      dau={i === 0}
                      cuoi={i === baiTap.length - 1}
                      onSua={(phan) => suaBai(bai.id, phan)}
                      onXoa={() => sua({ baiTap: baiTap.filter((b) => b.id !== bai.id) })}
                      onDoiCho={(huong) => doiCho(i, huong)}
                    />
                  ))}
                </View>
              )}
              <Button
                variant="outline"
                shape="pill"
                accessibilityLabel="Thêm bài tập"
                onPress={themBai}
              >
                <Icon name="plus" size={18} color={colors.text} />
                <Text>Thêm bài tập</Text>
              </Button>
            </KhungThe>

            <Button
              variant="outline"
              shape="pill"
              accessibilityLabel="Xóa buổi tập"
              onPress={xoaBuoi}
            >
              <Text className="text-destructive">Xóa buổi tập</Text>
            </Button>
          </ScrollView>

          <View
            className="border-t border-border bg-background px-4 pt-3"
            style={{ paddingBottom: 12 + insets.bottom }}
          >
            <Button
              shape="pill"
              accessibilityLabel="Xong"
              onPress={() => router.back()}
              className="min-h-[56px]"
            >
              <Text>Xong</Text>
            </Button>
          </View>
        </>
      ) : (
        <View className="items-center p-8">
          <Text variant="muted">Không tìm thấy buổi tập.</Text>
        </View>
      )}
    </KeyboardAvoidingView>
  );
}
