import { Image, Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';

/** Hàng ảnh dùng lưới 3 ô như check-in bữa ăn để ô ảnh và ô thêm ảnh cùng cỡ ở mọi màn. */
const SO_O_ANH = 3;
const O_ANH = 'aspect-square flex-1';
const O_THEM_ANH = cn(
  O_ANH,
  'items-center justify-center gap-2 rounded-xl border border-dashed border-input',
);

/**
 * Chọn đúng một ảnh: chưa có ảnh thì hiện hai ô nét đứt "Chụp ảnh" và "Chọn từ thư viện"; có ảnh rồi thì hiện ảnh kèm nút
 * xóa và ẩn hai ô kia. Không tự mở camera hay thư viện: màn gọi lo `onChup` và `onThuVien`.
 */
export function ChonMotAnh({
  anh,
  nhanAnh,
  onChup,
  onThuVien,
  onXoa,
}: {
  anh?: string;
  /** Nhãn truy cập của ảnh đã thêm, ví dụ "Ảnh buổi tập đã thêm". */
  nhanAnh: string;
  onChup(): void;
  onThuVien(): void;
  onXoa(): void;
}) {
  const { colors } = useTheme();
  return (
    <View className="flex-row gap-3">
      {anh ? (
        <View className={cn(O_ANH, 'overflow-hidden rounded-xl bg-secondary')}>
          <Image
            source={{ uri: anh }}
            accessibilityLabel={nhanAnh}
            resizeMode="cover"
            className="size-full"
          />
          <Pressable
            accessibilityRole="button"
            accessibilityLabel="Xóa ảnh"
            hitSlop={8}
            onPress={onXoa}
            className="absolute right-1.5 top-1.5 size-6 items-center justify-center rounded-full bg-background/90"
          >
            <Icon name="close" size={14} color={colors.text} />
          </Pressable>
        </View>
      ) : (
        <>
          <Pressable
            accessibilityRole="button"
            accessibilityLabel="Chụp ảnh"
            onPress={onChup}
            className={O_THEM_ANH}
          >
            <Icon name="camera" size={26} color={colors.text} />
            <Text className="text-sm">Chụp ảnh</Text>
          </Pressable>
          <Pressable
            accessibilityRole="button"
            accessibilityLabel="Chọn từ thư viện"
            onPress={onThuVien}
            className={O_THEM_ANH}
          >
            <Icon name="image" size={26} color={colors.text} />
            <Text className="text-center text-sm">Chọn từ thư viện</Text>
          </Pressable>
        </>
      )}
      {/* Giữ ô ảnh và ô thêm ảnh đúng cỡ của lưới 3 ô. */}
      {Array.from({ length: SO_O_ANH - (anh ? 1 : 2) }, (_, i) => (
        <View key={`trong-${i}`} className={O_ANH} />
      ))}
    </View>
  );
}
