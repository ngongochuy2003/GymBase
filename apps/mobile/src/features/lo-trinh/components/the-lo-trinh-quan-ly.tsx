import { Pressable, View } from 'react-native';
import { useRouter } from 'expo-router';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { LoaiNhiemVu, LoTrinhQuanLy } from '../types';
import { soNgayConLai } from '../utils/ngay';
import { AnhDaiDien } from './anh-nguoi';
import { NhanTrangThai, NutMenu, ThanhTienDo } from './thanh-phan-nho';

/** Số ngày còn lại tối đa để gắn nhãn "Còn n ngày". */
// TODO(ROUTE): ngưỡng "sắp hết" chưa được sếp chốt (adr.md, M20); tạm 7 ngày.
const NGUONG_SAP_HET_NGAY = 7;

const ICON_NHIEM_VU: Record<LoaiNhiemVu, string> = {
  bua_an: 'meal',
  buoi_tap: 'dumbbell',
  can_nang: 'scale',
  anh_co_the: 'camera',
};

/** Ảnh đại diện nhóm: hình tròn ghép tối đa 4 ảnh thành viên (2x2); nhóm đông hơn thì ô cuối hiện "+n". */
function NhomAvatar({ nguoi }: { nguoi: LoTrinhQuanLy['nguoiThamGia'] }) {
  const choNhieu = nguoi.length > 4;
  const hienThi = choNhieu ? nguoi.slice(0, 3) : nguoi.slice(0, 4);
  return (
    <View className="h-14 w-14 flex-row flex-wrap overflow-hidden rounded-full border border-border bg-secondary">
      {hienThi.map((n) => (
        <AnhDaiDien key={n.id} uri={n.anh} ten={n.ten} className="h-7 w-7 rounded-none" />
      ))}
      {choNhieu ? (
        <View className="h-7 w-7 items-center justify-center bg-secondary">
          <Text className="text-xs font-bold text-foreground">+{nguoi.length - 3}</Text>
        </View>
      ) : null}
    </View>
  );
}

/** Thẻ lộ trình ở sub-tab "Lộ trình tôi quản lý" (PT-01). */
export function TheLoTrinhQuanLy({ loTrinh, homNay }: { loTrinh: LoTrinhQuanLy; homNay: Date }) {
  const { colors } = useTheme();
  const router = useRouter();
  const conLai = soNgayConLai(loTrinh.ngayKetThuc, homNay);
  const dangChay = loTrinh.trangThai === 'dang_hoat_dong';
  const sapHet = dangChay && conLai >= 0 && conLai <= NGUONG_SAP_HET_NGAY;
  const nhom = loTrinh.loai === 'nhom';
  const nguoiDau = loTrinh.nguoiThamGia[0];
  const tenNguoi = nhom ? `Nhóm ${loTrinh.nguoiThamGia.length} người` : (nguoiDau?.ten ?? '');
  const tiepTheo = dangChay ? loTrinh.nhiemVuTiepTheo : undefined;

  return (
    <View className="gap-3 rounded-3xl border border-border bg-card p-4">
      <View className="flex-row items-center gap-3">
        {nhom ? (
          <NhomAvatar nguoi={loTrinh.nguoiThamGia} />
        ) : (
          <AnhDaiDien uri={nguoiDau?.anh} ten={tenNguoi} className="h-14 w-14" />
        )}
        <View className="flex-1 gap-0.5">
          <Text className="text-lg font-bold leading-6 text-foreground">{tenNguoi}</Text>
          <Text className="text-sm text-muted-foreground" numberOfLines={2}>
            {loTrinh.ten}
          </Text>
          {loTrinh.soCheckInChoDuyet > 0 ? (
            <View className="mt-1.5 flex-row items-center gap-1 self-start rounded-full bg-warning/20 px-2.5 py-1">
              <Icon name="calendar-check" size={14} color={colors.warning} />
              <Text className="text-xs font-bold text-warning">
                {loTrinh.soCheckInChoDuyet} check-in chờ duyệt
              </Text>
            </View>
          ) : null}
        </View>
        <View className="self-start">
          <NutMenu nhan={`Tùy chọn ${loTrinh.ten}`} />
        </View>
      </View>

      {!dangChay ? <NhanTrangThai trangThai={loTrinh.trangThai} /> : null}

      <View className="gap-1.5">
        <View className="flex-row items-center justify-between">
          <View className="flex-row items-center gap-2">
            <Text className="text-sm text-foreground">Tiến độ lộ trình</Text>
            {sapHet ? (
              <View className="rounded-full bg-warning/20 px-2 py-0.5">
                <Text className="text-xs font-medium text-warning">
                  {conLai === 0 ? 'Hết hạn hôm nay' : `Còn ${conLai} ngày`}
                </Text>
              </View>
            ) : null}
          </View>
          <Text className="text-sm font-bold text-foreground">{loTrinh.tienDo}%</Text>
        </View>
        <ThanhTienDo phanTram={loTrinh.tienDo} className="flex-none" />
      </View>

      {loTrinh.lyDoDong ? (
        <Text className="text-xs text-muted-foreground" numberOfLines={2}>
          Lý do đóng: {loTrinh.lyDoDong}
        </Text>
      ) : null}

      <View className="flex-row items-center gap-3 border-t border-border pt-3">
        {tiepTheo ? (
          <View className="flex-1 flex-row items-center gap-2.5">
            <Icon
              name={tiepTheo.quaHan ? 'alert' : ICON_NHIEM_VU[tiepTheo.loai]}
              size={22}
              color={tiepTheo.quaHan ? colors.error : colors.text}
            />
            <View className="flex-1">
              <Text
                className={cn(
                  'text-sm font-bold',
                  tiepTheo.quaHan ? 'text-destructive' : 'text-foreground',
                )}
              >
                {tiepTheo.quaHan ? 'Đã quá hạn:' : 'Nhiệm vụ tiếp theo:'}
              </Text>
              <Text className="text-xs text-muted-foreground" numberOfLines={2}>
                {tiepTheo.chiTiet}
              </Text>
            </View>
          </View>
        ) : (
          <View className="flex-1" />
        )}
        <Pressable
          accessibilityRole="button"
          accessibilityLabel={`Xem chi tiết ${loTrinh.ten}`}
          onPress={() =>
            router.push({ pathname: '/chi-tiet-hoc-vien', params: { id: loTrinh.id } })
          }
          className="flex-row items-center gap-1 rounded-xl border border-primary px-4 py-2"
        >
          <Text className="text-sm font-medium text-primary">Xem chi tiết</Text>
          <Icon name="chevron-right" size={16} color={colors.primary} />
        </Pressable>
      </View>
    </View>
  );
}
