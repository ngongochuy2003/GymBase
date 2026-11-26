import { Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';

/** Hàng chip chọn một giá trị lọc; `nhom` đặt tên cho nhãn trợ năng, ví dụ "Lọc loại Bữa ăn". */
export function ChipLoc<T extends string>({
  nhom,
  luaChon,
  chon,
  onChon,
}: {
  nhom: string;
  luaChon: readonly { gia: T; nhan: string }[];
  chon: T;
  onChon: (gia: NoInfer<T>) => void;
}) {
  return (
    <View accessibilityRole="radiogroup" className="flex-row flex-wrap gap-2">
      {luaChon.map((l) => {
        const dangChon = l.gia === chon;
        return (
          <Pressable
            key={l.gia}
            accessibilityRole="radio"
            accessibilityState={{ checked: dangChon }}
            accessibilityLabel={`Lọc ${nhom} ${l.nhan}`}
            onPress={() => onChon(l.gia)}
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
              {l.nhan}
            </Text>
          </Pressable>
        );
      })}
    </View>
  );
}
