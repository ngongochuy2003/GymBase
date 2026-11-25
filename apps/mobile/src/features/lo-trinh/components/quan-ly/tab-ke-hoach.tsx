import { View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import type { ChiTietQuanLy } from '../../types';
import { buoiTapCuaTuan, tenThu } from '../../utils/lich-tap';
import { ChonTuan } from '../chon-tuan';
import { KhungThe } from '../khung-the';

/**
 * Tab Kế hoạch của chi tiết lộ trình PT (PT-03), cũng dùng ở màn lời mời của học viên; chỉ xem: các bữa ăn đang bật (giờ, kcal, món) và lịch tập của tuần hiện
 * tại (thứ, giờ, nhóm cơ, bài tập). Sửa kế hoạch ở nút "Chỉnh sửa kế hoạch" cuối màn.
 */
export function TabKeHoach({
  chiTiet,
  chonTuan,
}: {
  chiTiet: Pick<ChiTietQuanLy, 'keHoach' | 'tuanHienTai'>;
  /** Có thì khối Lịch tập thêm hàng chip để xem tuần khác; `tuanHienTai` là tuần đang chọn. */
  chonTuan?: { soTuan: number; onDoiTuan(tuan: number): void };
}) {
  const { keHoach, tuanHienTai } = chiTiet;
  const buaAn = keHoach.buaAn.filter((b) => b.bat);
  const buoiTap = [...buoiTapCuaTuan(keHoach.lichTap, tuanHienTai)].sort((a, b) => a.thu - b.thu);

  return (
    <View className="gap-4">
      <KhungThe icon="meal" tieuDe="Kế hoạch dinh dưỡng" chuPhu={`${buaAn.length} bữa mỗi ngày`}>
        {buaAn.length === 0 ? (
          <Text variant="muted" className="text-sm">
            Chưa bật bữa ăn nào.
          </Text>
        ) : (
          <View>
            {buaAn.map((b, i) => (
              <View key={b.id} className={cn('gap-0.5 py-3', i > 0 && 'border-t border-border')}>
                <View className="flex-row items-center justify-between">
                  <Text className="text-base font-bold">{b.ten}</Text>
                  <Text variant="muted" className="text-sm">
                    {b.gio}
                    {b.kcal ? ` • ~${b.kcal} kcal` : ''}
                  </Text>
                </View>
                <Text variant="muted" className="text-sm leading-5">
                  {b.moTa || 'Chưa đặt món'}
                </Text>
              </View>
            ))}
          </View>
        )}
      </KhungThe>

      <KhungThe icon="dumbbell" tieuDe="Lịch tập" tieuDePhu={`(Tuần ${tuanHienTai})`}>
        {chonTuan ? (
          <ChonTuan
            lich={keHoach.lichTap}
            soTuan={chonTuan.soTuan}
            tuanChon={tuanHienTai}
            onDoiTuan={chonTuan.onDoiTuan}
          />
        ) : null}
        {buoiTap.length === 0 ? (
          <Text variant="muted" className="text-sm">
            Tuần này không có buổi tập.
          </Text>
        ) : (
          <View>
            {buoiTap.map((b, i) => (
              <View key={b.thu} className={cn('gap-1 py-3', i > 0 && 'border-t border-border')}>
                <View className="flex-row items-center justify-between">
                  <Text className="text-base font-bold">
                    {tenThu(b.thu)} • {b.ten}
                  </Text>
                  <Text variant="muted" className="text-sm">
                    {b.gio}
                  </Text>
                </View>
                <Text variant="muted" className="text-sm">
                  {[b.nhomCo, b.thoiLuongPhut ? `${b.thoiLuongPhut} phút` : undefined]
                    .filter(Boolean)
                    .join(' • ') || 'Chưa đặt nhóm cơ'}
                </Text>
                {b.baiTap && b.baiTap.length > 0 ? (
                  <View className="mt-1 gap-0.5">
                    {b.baiTap.map((bt) => (
                      <Text key={bt.id} className="text-sm leading-5">
                        • {bt.ten} — {bt.soSet} set × {bt.soLan}
                      </Text>
                    ))}
                  </View>
                ) : (
                  <Text className="text-sm text-warning">Chưa đặt bài tập</Text>
                )}
              </View>
            ))}
          </View>
        )}
      </KhungThe>
    </View>
  );
}
