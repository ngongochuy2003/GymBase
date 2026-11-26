import { Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { CheckInQuanLy, NguoiLoTrinh, TrangThaiDuyet, TrangThaiThamGia } from '../../types';
import type { TrangThaiNhiemVuNhom } from '../../utils/nhiem-vu-nhom';
import { AnhDaiDien } from '../anh-nguoi';

/** Viên thuốc nhỏ: nền và chữ cùng tông. */
export function VienNhan({
  nhan,
  nen,
  chu,
}: {
  nhan: string;
  /** Lớp nền, ví dụ `bg-success/15`. */
  nen: string;
  chu: string;
}) {
  return (
    <View className={cn('rounded-full px-3 py-1.5', nen)}>
      <Text className={cn('text-xs font-bold', chu)}>{nhan}</Text>
    </View>
  );
}

/** Nhãn trạng thái của nhiệm vụ chung (cả nhóm), ví dụ "Chờ check-in", "Sắp tới 17:30". */
export function NhanNhiemVuNhom({
  trangThai,
  gio,
}: {
  trangThai: TrangThaiNhiemVuNhom;
  gio: string;
}) {
  switch (trangThai) {
    case 'da_hoan_thanh':
      return <VienNhan nhan="Đã hoàn thành" nen="bg-success/15" chu="text-success" />;
    case 'cho_check_in':
      return <VienNhan nhan="Chờ check-in" nen="bg-warning/15" chu="text-warning" />;
    case 'sap_toi':
      return <VienNhan nhan={`Sắp tới ${gio}`} nen="bg-secondary" chu="text-foreground" />;
    case 'ngay_mai':
      return <VienNhan nhan="Ngày mai" nen="bg-secondary" chu="text-muted-foreground" />;
    case 'qua_han':
      return <VienNhan nhan="Quá hạn" nen="bg-destructive/15" chu="text-destructive" />;
  }
}

/** Nhãn trạng thái duyệt của một check-in ở góc nhìn PT. */
export function NhanDuyet({ trangThai }: { trangThai: TrangThaiDuyet }) {
  switch (trangThai) {
    case 'cho_duyet':
      return <VienNhan nhan="Chờ duyệt" nen="bg-warning/15" chu="text-warning" />;
    case 'da_duyet':
      return <VienNhan nhan="Đã duyệt" nen="bg-success/15" chu="text-success" />;
    case 'can_bo_sung':
      return <VienNhan nhan="Cần bổ sung" nen="bg-info/15" chu="text-info" />;
  }
}

/** Nhãn lượt tham gia của một thành viên (K9). */
export function NhanThamGia({ trangThai }: { trangThai: TrangThaiThamGia }) {
  switch (trangThai) {
    case 'tham_gia':
      return <VienNhan nhan="Đã tham gia" nen="bg-success/15" chu="text-success" />;
    case 'cho_chap_nhan':
      return <VienNhan nhan="Chờ chấp nhận" nen="bg-warning/15" chu="text-warning" />;
    case 'tu_choi':
      return <VienNhan nhan="Đã từ chối" nen="bg-destructive/15" chu="text-destructive" />;
    case 'da_roi':
      return <VienNhan nhan="Đã rời" nen="bg-secondary" chu="text-muted-foreground" />;
  }
}

/** Các ảnh đại diện nhỏ xếp chồng; nhiều hơn `toiDa` thì ô cuối hiện "+n". */
export function AvatarChong({
  nguoi,
  toiDa = 3,
  kichThuoc = 'size-6',
}: {
  nguoi: readonly NguoiLoTrinh[];
  toiDa?: number;
  kichThuoc?: string;
}) {
  const hien = nguoi.slice(0, toiDa);
  const them = nguoi.length - hien.length;
  return (
    <View className="flex-row items-center">
      {hien.map((n, i) => (
        <View key={n.id} className={cn('rounded-full border-2 border-card', i > 0 && '-ml-2')}>
          <AnhDaiDien uri={n.anh} ten={n.ten} className={kichThuoc} />
        </View>
      ))}
      {them > 0 ? (
        <View
          className={cn(
            '-ml-2 items-center justify-center rounded-full border-2 border-card bg-secondary',
            kichThuoc,
          )}
        >
          <Text className="text-[10px] font-bold text-foreground">+{them}</Text>
        </View>
      ) : null}
    </View>
  );
}

/**
 * Một dòng check-in: biểu tượng bữa ăn hoặc buổi tập, tiêu đề, thời điểm, tên học viên (lộ trình nhóm cần biết của ai) và
 * nhãn duyệt. Có `onXem` thì thay nhãn bằng nút "Xem"; có `onMo` thì cả dòng bấm được (kèm nhãn duyệt).
 */
export function DongCheckIn({
  checkIn,
  hienTenHocVien,
  onXem,
  onMo,
}: {
  checkIn: CheckInQuanLy;
  hienTenHocVien: boolean;
  onXem?(): void;
  onMo?(): void;
}) {
  const { colors } = useTheme();
  const Khung = onMo ? Pressable : View;
  return (
    <Khung
      {...(onMo
        ? {
            accessibilityRole: 'button' as const,
            accessibilityLabel: `${checkIn.tieuDe} của ${checkIn.hocVien.ten}`,
            onPress: onMo,
          }
        : {})}
      className="flex-row items-center gap-3 py-3"
    >
      <Icon name={checkIn.loai === 'bua_an' ? 'meal' : 'dumbbell'} size={22} color={colors.text} />
      <View className="min-w-0 flex-1 gap-0.5">
        <Text className="text-base font-bold" numberOfLines={1}>
          {checkIn.tieuDe}
        </Text>
        <Text variant="muted" className="text-sm" numberOfLines={1}>
          {hienTenHocVien ? `${checkIn.hocVien.ten} • ` : ''}
          {checkIn.thoiDiem}
        </Text>
      </View>
      {onXem ? (
        <Pressable
          accessibilityRole="button"
          accessibilityLabel={`Xem ${checkIn.tieuDe} của ${checkIn.hocVien.ten}`}
          onPress={onXem}
          className="rounded-xl border border-border px-4 py-2"
        >
          <Text className="text-sm font-medium">Xem</Text>
        </Pressable>
      ) : (
        <NhanDuyet trangThai={checkIn.trangThai} />
      )}
    </Khung>
  );
}
