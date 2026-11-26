import { View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { ChiTietQuanLy, CheckInQuanLy, LoaiNhiemVu } from '../../types';
import { moTaTienDoNhiemVu, trangThaiNhiemVuNhom } from '../../utils/nhiem-vu-nhom';
import { AnhDaiDien } from '../anh-nguoi';
import { KhungThe } from '../khung-the';
import { AvatarChong, DongCheckIn, NhanNhiemVuNhom } from './thanh-phan';

const ICON_NHIEM_VU: Record<LoaiNhiemVu, string> = {
  bua_an: 'meal',
  buoi_tap: 'dumbbell',
  can_nang: 'scale',
  anh_co_the: 'camera',
};

/**
 * Tab Hôm nay của chi tiết lộ trình PT (PT-03). Công việc hôm nay là của cả lộ trình (nhóm cũng một danh sách): nhãn trạng thái
 * là của cả nhóm, thành viên nào đã check-in thì có ảnh đại diện nhỏ ở dòng đó. Dưới là check-in chờ duyệt (từng người một vì
 * PT duyệt riêng từng bản), hoạt động gần đây và các check-in đã xử lý.
 */
export function TabHomNay({
  chiTiet,
  onXemCheckIn,
  onDuyetNhanh,
}: {
  chiTiet: ChiTietQuanLy;
  onXemCheckIn(checkIn: CheckInQuanLy): void;
  /** Mở PT-05 cho lộ trình này (N43); lộ trình đã đóng thì không truyền. */
  onDuyetNhanh?(): void;
}) {
  const { colors } = useTheme();
  const { thanhVien, nhiemVuHomNay, checkIn, hoatDong } = chiTiet;
  const laNhom = chiTiet.loTrinh.loai === 'nhom';
  const choDuyet = checkIn.filter((c) => c.trangThai === 'cho_duyet');
  const daXuLy = checkIn.filter((c) => c.trangThai !== 'cho_duyet');

  return (
    <View className="gap-4">
      <View className="gap-1">
        <Text accessibilityRole="header" className="px-1 text-xl font-bold">
          Công việc hôm nay
        </Text>
        {nhiemVuHomNay.length === 0 ? (
          <Text variant="muted" className="px-1 text-sm">
            Lộ trình không còn nhiệm vụ hôm nay.
          </Text>
        ) : (
          <View>
            {nhiemVuHomNay.map((nv, i) => {
              const trangThai = trangThaiNhiemVuNhom(nv, thanhVien);
              const xong = trangThai === 'da_hoan_thanh';
              const daLam = thanhVien.filter((t) => nv.daCheckIn.includes(t.id));
              return (
                <View
                  key={nv.id}
                  accessibilityLabel={`${nv.tieuDe}, ${moTaTienDoNhiemVu(nv, thanhVien, laNhom)}`}
                  className={cn(
                    'flex-row items-center gap-3 px-1 py-3',
                    i > 0 && 'border-t border-border',
                  )}
                >
                  <View
                    className={cn(
                      'size-11 items-center justify-center rounded-full',
                      xong ? 'bg-primary' : 'bg-secondary',
                    )}
                  >
                    <Icon
                      name={xong ? 'check' : ICON_NHIEM_VU[nv.loai]}
                      size={22}
                      color={xong ? colors.onPrimary : colors.text}
                    />
                  </View>
                  <View className="min-w-0 flex-1">
                    <Text className="text-base font-bold">{nv.tieuDe}</Text>
                    <Text variant="muted" className="text-sm" numberOfLines={1}>
                      {moTaTienDoNhiemVu(nv, thanhVien, laNhom)}
                    </Text>
                  </View>
                  <View className="items-end gap-1.5">
                    <NhanNhiemVuNhom trangThai={trangThai} gio={nv.gio} />
                    {laNhom && daLam.length > 0 ? (
                      <View accessibilityLabel={`${daLam.length} người đã check-in`}>
                        <AvatarChong nguoi={daLam} toiDa={3} />
                      </View>
                    ) : null}
                  </View>
                </View>
              );
            })}
          </View>
        )}
      </View>

      {choDuyet.length > 0 ? (
        <KhungThe
          icon="calendar-check"
          tieuDe={`Check-in chờ duyệt (${choDuyet.length})`}
          lienKet={onDuyetNhanh ? { nhan: 'Duyệt nhanh', onPress: onDuyetNhanh } : undefined}
        >
          <View>
            {choDuyet.map((c, i) => (
              <View key={c.id} className={cn(i > 0 && 'border-t border-border')}>
                <DongCheckIn checkIn={c} hienTenHocVien={laNhom} onXem={() => onXemCheckIn(c)} />
              </View>
            ))}
          </View>
        </KhungThe>
      ) : null}

      {hoatDong.length > 0 ? (
        <KhungThe icon="clock" tieuDe="Hoạt động gần đây">
          <View className="gap-2">
            {hoatDong.map((h) => (
              <View key={h.id} className="flex-row items-center gap-3 rounded-xl bg-secondary p-3">
                <AnhDaiDien uri={h.nguoi.anh} ten={h.nguoi.ten} className="size-8" />
                <View className="min-w-0 flex-1">
                  <Text className="text-sm" numberOfLines={2}>
                    {h.noiDung}
                  </Text>
                  <Text variant="muted" className="text-xs">
                    {h.thoiDiem}
                  </Text>
                </View>
              </View>
            ))}
          </View>
        </KhungThe>
      ) : null}

      {daXuLy.length > 0 ? (
        <KhungThe icon="check-circle" tieuDe="Check-in gần đây">
          <View>
            {daXuLy.map((c, i) => (
              <View key={c.id} className={cn(i > 0 && 'border-t border-border')}>
                <DongCheckIn checkIn={c} hienTenHocVien={laNhom} />
              </View>
            ))}
          </View>
        </KhungThe>
      ) : null}
    </View>
  );
}
