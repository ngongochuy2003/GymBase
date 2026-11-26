import { Pressable, ScrollView, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import type { LichTapTheoTuan } from '../types';

/** Hàng chip cuộn ngang chọn tuần của lịch tập; tuần đã sửa riêng (N29) có chấm. */
export function ChonTuan({
  lich,
  soTuan,
  tuanChon,
  onDoiTuan,
}: {
  lich: LichTapTheoTuan;
  soTuan: number;
  tuanChon: number;
  onDoiTuan(tuan: number): void;
}) {
  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      contentContainerClassName="gap-2"
      accessibilityRole="tablist"
    >
      {Array.from({ length: soTuan }, (_, i) => i + 1).map((tuan) => {
        const chon = tuan === tuanChon;
        const rieng = tuan > 1 && lich.rieng[tuan] !== undefined;
        return (
          <Pressable
            key={tuan}
            accessibilityRole="tab"
            accessibilityState={{ selected: chon }}
            onPress={() => onDoiTuan(tuan)}
            className={cn(
              'flex-row items-center gap-1.5 rounded-full border px-3.5 py-1.5',
              chon ? 'border-primary bg-primary' : 'border-border bg-background',
            )}
          >
            <Text
              className={cn(
                'text-sm font-medium',
                chon ? 'text-primary-foreground' : 'text-foreground',
              )}
            >
              Tuần {tuan}
            </Text>
            {rieng ? (
              <View
                className={cn(
                  'h-1.5 w-1.5 rounded-full',
                  chon ? 'bg-primary-foreground' : 'bg-warning',
                )}
              />
            ) : null}
          </Pressable>
        );
      })}
    </ScrollView>
  );
}
