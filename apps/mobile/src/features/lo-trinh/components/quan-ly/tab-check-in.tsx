import { useState } from 'react';
import { Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import type { CheckInQuanLy, TrangThaiDuyet } from '../../types';
import { KhungThe } from '../khung-the';
import { DongCheckIn } from './thanh-phan';

type Loc = 'tat_ca' | TrangThaiDuyet;

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
}: {
  checkIn: readonly CheckInQuanLy[];
  laNhom: boolean;
  onMo(checkIn: CheckInQuanLy): void;
}) {
  const [loc, setLoc] = useState<Loc>('tat_ca');
  const danhSach = checkIn.filter((c) => loc === 'tat_ca' || c.trangThai === loc);
  return (
    <View className="gap-4">
      <View accessibilityRole="radiogroup" className="flex-row flex-wrap gap-2">
        {BO_LOC.map((b) => {
          const dangChon = loc === b.gia;
          const so = checkIn.filter((c) => b.gia === 'tat_ca' || c.trangThai === b.gia).length;
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
                <DongCheckIn
                  checkIn={c}
                  hienTenHocVien={laNhom}
                  // TODO(ROUTE): mở PT-04 (đánh giá hoặc xem lại check-in) khi có màn; thuộc luong-man-hinh.md, PT-04.
                  onMo={() => onMo(c)}
                />
              </View>
            ))}
          </View>
        )}
      </KhungThe>
    </View>
  );
}
