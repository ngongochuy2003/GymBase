import type { ReactNode } from 'react';
import { Image, Pressable, View } from 'react-native';
import Animated, { useAnimatedStyle, type SharedValue } from 'react-native-reanimated';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { AnhCoTheMoc, DanhGiaTuPt, DiemCanNang, LoTrinhCuaToi } from '../types';
import { useChayLen } from '../hooks/use-chay-len';
import { useSoDemLen } from '../hooks/use-so-dem-len';
import { phanTramMucTieu, soThapPhan } from '../utils/tien-do';
import { soNgayConLai } from '../utils/ngay';
import { AnhDaiDien } from './anh-nguoi';
import { BieuDoCanNang } from './bieu-do-can-nang';
import { KhungThe } from './khung-the';

/** Thanh tiến độ ngang có animation đầy dần chạy trên luồng UI; `tienTrinh` chạy từ 0 tới 1. */
function ThanhTienDoChay({
  phanTram,
  tienTrinh,
}: {
  phanTram: number;
  tienTrinh: SharedValue<number>;
}) {
  const { colors } = useTheme();
  const kieuDay = useAnimatedStyle(() => ({ width: `${phanTram * tienTrinh.value}%` }));
  return (
    <View
      accessibilityRole="progressbar"
      accessibilityValue={{ min: 0, max: 100, now: phanTram }}
      className="h-2 flex-1 overflow-hidden rounded-full bg-secondary"
    >
      <Animated.View
        style={[{ height: '100%', borderRadius: 999, backgroundColor: colors.primary }, kieuDay]}
      />
    </View>
  );
}

/** Tiến độ mục tiêu: cân nặng bắt đầu đến mục tiêu, phần trăm đã đi và số ngày còn lại. */
export function KhoiMucTieu({
  loTrinh,
  canNangHienTai,
  homNay,
}: {
  loTrinh: LoTrinhCuaToi;
  canNangHienTai: number;
  homNay: Date;
}) {
  const { colors } = useTheme();
  const { tu, den, donVi } = loTrinh.mucTieu;
  const phanTram = phanTramMucTieu(canNangHienTai, tu, den);
  const daDoi = Math.abs(canNangHienTai - tu);
  const conLai = Math.max(0, soNgayConLai(loTrinh.ngayKetThuc, homNay));
  // Thanh chạy trên luồng UI; số % và số kg đếm lên cùng nhịp (chữ chỉ vẽ lại khi số hiển thị đổi).
  const tienTrinh = useChayLen(1000, 250);
  const phanTramHien = useSoDemLen(phanTram, 0, 1000, 250);
  const daDoiHien = useSoDemLen(daDoi, 1, 1000, 250);
  return (
    <View className="gap-3 rounded-2xl border border-border bg-card p-4">
      <View className="flex-row items-center gap-3">
        <View className="size-12 items-center justify-center rounded-full bg-secondary">
          <Icon name="target" size={26} color={colors.primary} />
        </View>
        <View className="flex-1 gap-0.5">
          <Text className="text-lg font-bold leading-6" numberOfLines={2}>
            {loTrinh.ten}
          </Text>
          <Text variant="muted" className="text-sm">
            Từ {tu} {donVi} → {den} {donVi}
          </Text>
        </View>
      </View>
      <View className="flex-row items-center gap-3">
        <ThanhTienDoChay phanTram={phanTram} tienTrinh={tienTrinh} />
        <Text className="w-11 text-right font-bold">{phanTramHien}%</Text>
      </View>
      <Text variant="muted" className="text-sm">
        {den >= tu ? 'Đã tăng' : 'Đã giảm'} {soThapPhan(daDoiHien)} {donVi} • Còn {conLai} ngày
      </Text>
    </View>
  );
}

/** Biểu đồ cân nặng các tuần đã đo. */
export function KhoiCanNang({
  diem,
  donVi,
  onMoChiTiet,
}: {
  diem: readonly DiemCanNang[];
  donVi: string;
  onMoChiTiet(): void;
}) {
  const soTuan = diem.at(-1)?.tuan ?? 0;
  return (
    <KhungThe
      icon="trend"
      tieuDe={`Cân nặng ${soTuan} tuần`}
      lienKet={{ nhan: 'Chi tiết', onPress: onMoChiTiet }}
    >
      <BieuDoCanNang diem={diem} donVi={donVi} />
    </KhungThe>
  );
}

/**
 * Một phần của thẻ chỉ số, xếp dọc: biểu tượng, giá trị, nhãn. Nhãn luôn hai dòng nên ba phần thẳng hàng với nhau;
 * `ngan` vẽ vạch dọc ngăn với phần bên trái.
 */
function ChiSo({
  icon,
  nhan,
  giaTri,
  ngan = false,
}: {
  icon: string;
  /** Đúng hai dòng chữ, mỗi phần tử một dòng. */
  nhan: readonly [string, string];
  giaTri: ReactNode;
  ngan?: boolean;
}) {
  const { colors } = useTheme();
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={nhan.join(' ')}
      // TODO(ROUTE): mở tab Check-in của HV-07 lọc theo loại; "Tuân thủ kế hoạch" chưa có thiết kế.
      onPress={() => undefined}
      className={cn('flex-1 items-center gap-1.5 px-2 py-4', ngan && 'border-l border-border')}
    >
      <View className="size-11 items-center justify-center rounded-full bg-secondary">
        <Icon name={icon} size={24} color={colors.primary} />
      </View>
      <Text className="text-2xl font-bold leading-8">{giaTri}</Text>
      <View>
        {nhan.map((dong) => (
          <Text key={dong} variant="muted" className="text-center text-xs leading-4">
            {dong}
          </Text>
        ))}
      </View>
    </Pressable>
  );
}

/** Ba chỉ số trong một thẻ: buổi tập hoàn thành, bữa ăn đã check, tuân thủ kế hoạch. */
export function KhoiChiSo({
  buoiTap,
  buaAn,
  tuanThu,
}: {
  buoiTap: { xong: number; tong: number };
  buaAn: { xong: number; tong: number };
  tuanThu: number;
}) {
  // Các số đếm lên từ 0 khi vào màn; số tổng (mẫu số) đứng yên.
  const buoiTapHien = useSoDemLen(buoiTap.xong, 0, 900, 450);
  const buaAnHien = useSoDemLen(buaAn.xong, 0, 900, 450);
  const tuanThuHien = useSoDemLen(tuanThu, 0, 900, 450);
  return (
    <View className="flex-row overflow-hidden rounded-2xl border border-border bg-card">
      <ChiSo
        icon="dumbbell"
        nhan={['Buổi tập', 'hoàn thành']}
        giaTri={
          <>
            {buoiTapHien}
            <Text variant="muted" className="text-base font-normal">
              /{buoiTap.tong}
            </Text>
          </>
        }
      />
      <ChiSo
        ngan
        icon="meal"
        nhan={['Bữa ăn', 'đã check']}
        giaTri={
          <>
            {buaAnHien}
            <Text variant="muted" className="text-base font-normal">
              /{buaAn.tong}
            </Text>
          </>
        }
      />
      <ChiSo ngan icon="check-circle" nhan={['Tuân thủ', 'kế hoạch']} giaTri={`${tuanThuHien}%`} />
    </View>
  );
}

/** Ảnh cơ thể theo mốc tuần: mốc đầu và mốc mới nhất cạnh nhau; "Xem tất cả" mở màn ảnh theo mốc. */
export function KhoiAnhMoc({
  anh,
  onXemTatCa,
}: {
  anh: readonly AnhCoTheMoc[];
  onXemTatCa(): void;
}) {
  const [dau, cuoi] = [anh[0], anh.at(-1)];
  const hai = dau && cuoi && dau !== cuoi ? [dau, cuoi] : dau ? [dau] : [];
  return (
    <KhungThe
      icon="images"
      tieuDe="Ảnh cơ thể theo mốc"
      lienKet={{ nhan: 'Xem tất cả', onPress: onXemTatCa }}
    >
      <View className="flex-row gap-3">
        {hai.map((a) => (
          <View
            key={a.tuan}
            className="aspect-[3/4] flex-1 overflow-hidden rounded-xl bg-secondary"
          >
            <Image
              source={{ uri: a.uri }}
              accessibilityLabel={`Ảnh cơ thể tuần ${a.tuan}`}
              resizeMode="cover"
              className="size-full"
            />
            <View className="absolute left-2 top-2 rounded-full bg-background/90 px-3 py-1">
              <Text className="text-sm font-bold">Tuần {a.tuan}</Text>
            </View>
          </View>
        ))}
      </View>
    </KhungThe>
  );
}

/** Nhận xét mới nhất của PT: ảnh, tên, thời điểm và nội dung. */
export function KhoiDanhGiaPt({
  danhGia,
  tieuDe = 'Đánh giá từ PT',
}: {
  danhGia: DanhGiaTuPt;
  tieuDe?: string;
}) {
  return (
    <KhungThe icon="message" tieuDe={tieuDe}>
      <View className="gap-2">
        <View className="flex-row items-center gap-3">
          <AnhDaiDien uri={danhGia.pt.anh} ten={danhGia.pt.ten} className="size-10" />
          <View className="flex-1">
            <Text className="font-bold">{danhGia.pt.ten}</Text>
            <Text variant="muted" className="text-sm">
              Huấn luyện viên
            </Text>
          </View>
          <Text variant="muted" className="text-sm">
            {danhGia.thoiDiem}
          </Text>
        </View>
        <View className="ml-[52px] rounded-2xl bg-secondary p-3">
          <Text className="text-sm leading-5">{danhGia.noiDung}</Text>
        </View>
      </View>
    </KhungThe>
  );
}
