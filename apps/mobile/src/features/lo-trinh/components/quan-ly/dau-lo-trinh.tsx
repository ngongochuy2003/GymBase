import { useState } from 'react';
import { Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { ChiTietQuanLy } from '../../types';
import { thanhVienDangThamGia } from '../../utils/nhiem-vu-nhom';
import { AnhDaiDien } from '../anh-nguoi';
import { NhanTrangThai, ThanhTienDo } from '../thanh-phan-nho';
import { AvatarChong, NhanThamGia } from './thanh-phan';

/** Thông báo khi lộ trình đã đóng: chỉ xem, kèm lý do nếu PT nhập (N28). */
function DaiLoTrinhDong({ chiTiet }: { chiTiet: ChiTietQuanLy }) {
  const { colors } = useTheme();
  const { trangThai, lyDoDong } = chiTiet.loTrinh;
  const thatBai = trangThai === 'that_bai';
  return (
    <View className="gap-1 rounded-xl bg-secondary p-3">
      <View className="flex-row items-center gap-2">
        <Icon name="lock" size={16} color={colors.muted} />
        <Text className="flex-1 text-sm font-bold">
          {thatBai ? 'Lộ trình đã đóng: thất bại' : 'Lộ trình đã hoàn thành'}
        </Text>
      </View>
      <Text variant="muted" className="text-sm leading-5">
        {lyDoDong ? `Lý do: ${lyDoDong}. ` : ''}Lộ trình đã đóng chỉ để xem, không sửa kế hoạch hay
        duyệt thêm check-in.
      </Text>
    </View>
  );
}

/**
 * Đầu màn chi tiết lộ trình của PT: tên, học viên (hoặc nhóm) và PT phụ trách, tuần hiện tại kèm thanh tiến độ. Lộ trình nhóm
 * có thêm danh sách thành viên mở ra bằng cách bấm vào dòng nhóm, mỗi người kèm trạng thái tham gia. Lộ trình đã đóng hiện
 * thêm nhãn trạng thái và thông báo chỉ xem.
 */
export function DauLoTrinh({ chiTiet }: { chiTiet: ChiTietQuanLy }) {
  const { colors } = useTheme();
  const [moThanhVien, setMoThanhVien] = useState(false);
  const { loTrinh, pt, thanhVien } = chiTiet;
  const laNhom = loTrinh.loai === 'nhom';
  const dangChay = loTrinh.trangThai === 'dang_hoat_dong';
  const dau = thanhVien[0];
  const soDangThamGia = thanhVienDangThamGia(thanhVien).length;

  return (
    <View className="gap-3 rounded-2xl border border-border bg-card p-4">
      <View className="flex-row items-start gap-2">
        <Text className="flex-1 text-xl font-bold leading-7">{loTrinh.ten}</Text>
        {!dangChay ? <NhanTrangThai trangThai={loTrinh.trangThai} /> : null}
      </View>

      <View className="flex-row items-center gap-3">
        {laNhom ? (
          <Pressable
            accessibilityRole="button"
            accessibilityLabel={`Thành viên, nhóm ${thanhVien.length} người`}
            accessibilityState={{ expanded: moThanhVien }}
            onPress={() => setMoThanhVien((m) => !m)}
            className="min-w-0 flex-1 flex-row items-center gap-2"
          >
            <AvatarChong nguoi={thanhVien} toiDa={3} kichThuoc="size-9" />
            <View className="min-w-0 flex-1">
              <Text className="font-bold" numberOfLines={1}>
                Nhóm {thanhVien.length} người
              </Text>
              <Text variant="muted" className="text-xs" numberOfLines={1}>
                {soDangThamGia} đang tham gia
              </Text>
            </View>
            <Icon
              name={moThanhVien ? 'chevron-up' : 'chevron-down'}
              size={18}
              color={colors.muted}
            />
          </Pressable>
        ) : (
          <View className="min-w-0 flex-1 flex-row items-center gap-2">
            <AnhDaiDien uri={dau?.anh} ten={dau?.ten ?? ''} className="size-11" />
            <View className="min-w-0 flex-1">
              <Text className="font-bold" numberOfLines={1}>
                {dau?.ten}
              </Text>
              <Text variant="muted" className="text-xs">
                Học viên
              </Text>
            </View>
          </View>
        )}
        <View className="h-8 w-px bg-border" />
        <View className="min-w-0 flex-1 flex-row items-center gap-2">
          <AnhDaiDien uri={pt.anh} ten={pt.ten} className="size-11" />
          <View className="min-w-0 flex-1">
            <Text className="font-bold" numberOfLines={1}>
              {pt.ten}
            </Text>
            <Text variant="muted" className="text-xs">
              PT phụ trách
            </Text>
          </View>
        </View>
      </View>

      {laNhom && moThanhVien ? (
        <View className="rounded-xl bg-secondary px-3">
          {thanhVien.map((t, i) => (
            <View
              key={t.id}
              className={
                i > 0
                  ? 'flex-row items-center gap-3 border-t border-border py-2.5'
                  : 'flex-row items-center gap-3 py-2.5'
              }
            >
              <AnhDaiDien uri={t.anh} ten={t.ten} className="size-8" />
              <Text className="min-w-0 flex-1" numberOfLines={1}>
                {t.ten}
              </Text>
              <NhanThamGia trangThai={t.thamGia} />
            </View>
          ))}
        </View>
      ) : null}

      <View className="gap-1.5">
        <View className="flex-row items-center justify-between">
          <Text className="text-sm">
            Tuần {chiTiet.tuanHienTai} / {loTrinh.tongTuan}
          </Text>
          <Text className="text-sm font-bold">{loTrinh.tienDo}%</Text>
        </View>
        <ThanhTienDo phanTram={loTrinh.tienDo} className="flex-none" />
      </View>

      {!dangChay ? <DaiLoTrinhDong chiTiet={chiTiet} /> : null}
    </View>
  );
}
