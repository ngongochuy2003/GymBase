import { useState } from 'react';
import { Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import type { CheckInQuanLy, LoaiCheckIn, TrangThaiDuyet } from '../../types';
import { ChipLoc } from '../chip-loc';
import { KhungThe } from '../khung-the';
import { DongCheckIn } from './thanh-phan';

type Loc = 'tat_ca' | TrangThaiDuyet;
type LocLoai = 'tat_ca' | LoaiCheckIn;

const LOAI: readonly { gia: LocLoai; nhan: string }[] = [
  { gia: 'tat_ca', nhan: 'Mọi loại' },
  { gia: 'bua_an', nhan: 'Bữa ăn' },
  { gia: 'buoi_tap', nhan: 'Buổi tập' },
];

const BO_LOC: readonly { gia: Loc; nhan: string }[] = [
  { gia: 'tat_ca', nhan: 'Tất cả' },
  { gia: 'cho_duyet', nhan: 'Chờ duyệt' },
  { gia: 'da_duyet', nhan: 'Đã duyệt' },
  { gia: 'can_bo_sung', nhan: 'Cần bổ sung' },
];

/**
 * Tab Check-in của chi tiết lộ trình PT (PT-03): mọi check-in bữa ăn và buổi tập của lộ trình (cân nặng, ảnh cơ thể không
 * qua duyệt nên không có ở đây, N16), lọc theo trạng thái duyệt. Lộ trình nhóm ghi tên học viên ở mỗi dòng.
 */
export function TabCheckIn({
  checkIn,
  laNhom,
  onMo,
  loaiBanDau = 'tat_ca',
}: {
  checkIn: readonly CheckInQuanLy[];
  laNhom: boolean;
  onMo(checkIn: CheckInQuanLy): void;
  /** Loại lọc sẵn khi mở tab từ ô chỉ số tiến độ (N42). */
  loaiBanDau?: LocLoai;
}) {
  const [loc, setLoc] = useState<Loc>('tat_ca');
  const [loai, setLoai] = useState<LocLoai>(loaiBanDau);
  const theoLoai = checkIn.filter((c) => loai === 'tat_ca' || c.loai === loai);
  const danhSach = theoLoai.filter((c) => loc === 'tat_ca' || c.trangThai === loc);
  return (
    <View className="gap-4">
      <ChipLoc nhom="loại" luaChon={LOAI} chon={loai} onChon={setLoai} />
      <View accessibilityRole="radiogroup" className="flex-row flex-wrap gap-2">
        {BO_LOC.map((b) => {
          const dangChon = loc === b.gia;
          const so = theoLoai.filter((c) => b.gia === 'tat_ca' || c.trangThai === b.gia).length;
          return (
            <Pressable
              key={b.gia}
              accessibilityRole="radio"
              accessibilityState={{ checked: dangChon }}
              accessibilityLabel={`Lọc ${b.nhan}`}
              onPress={() => setLoc(b.gia)}
              className={cn(
                'rounded-full border px-3.5 py-1.5',
                dangChon ? 'border-transparent bg-primary' : 'border-border bg-card',
              )}
            >
              <Text
                className={cn(
                  'text-sm font-medium',
                  dangChon ? 'text-primary-foreground' : 'text-foreground',
                )}
              >
                {b.nhan} ({so})
              </Text>
            </Pressable>
          );
        })}
      </View>
      <KhungThe icon="calendar-check" tieuDe="Check-in" chuPhu={`${danhSach.length} bản`}>
        {danhSach.length === 0 ? (
          <Text variant="muted" className="text-sm">
            Không có check-in nào ở mục này.
          </Text>
        ) : (
          <View>
            {danhSach.map((c, i) => (
              <View key={c.id} className={cn(i > 0 && 'border-t border-border')}>
                <DongCheckIn checkIn={c} hienTenHocVien={laNhom} onMo={() => onMo(c)} />
              </View>
            ))}
          </View>
        )}
      </KhungThe>
    </View>
  );
}
