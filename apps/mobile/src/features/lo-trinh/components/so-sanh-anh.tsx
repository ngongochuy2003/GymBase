import type { ReactNode } from 'react';
import { Image, Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { AnhCoTheMoc } from '../types';

/** Một cột: nhãn tuần phía trên và khung tỉ lệ 3:4. `nhan` rỗng thì giữ chỗ để hai cột thẳng hàng. */
function CotAnh({ nhan, children }: { nhan?: string; children: ReactNode }) {
  return (
    <View className="flex-1 gap-2">
      <Text variant="muted" className="text-center text-sm font-medium">
        {nhan ?? ' '}
      </Text>
      <View className="aspect-[3/4]">{children}</View>
    </View>
  );
}

/** Ô thêm ảnh nét đứt, hai ô xếp dọc chia đều chiều cao của cột. */
function OThemAnh({ icon, nhan, onPress }: { icon: string; nhan: string; onPress(): void }) {
  const { colors } = useTheme();
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={nhan}
      onPress={onPress}
      className="flex-1 items-center justify-center gap-1 rounded-xl border border-dashed border-input px-2"
    >
      <Icon name={icon} size={26} color={colors.text} />
      <Text className="text-center text-sm">{nhan}</Text>
    </Pressable>
  );
}

/**
 * Ảnh mốc trước (trái) và ảnh mới (phải) cạnh nhau để chụp cùng góc. Chưa có ảnh mới thì cột phải là hai ô "Chụp ảnh" và
 * "Chọn từ thư viện" xếp dọc; chọn xong thì ảnh mới thay chỗ hai ô đó, hai ảnh ngang bằng nhau, nhãn "Tuần n" đứng trên mỗi
 * ảnh (`tuanMoi` là tuần của ảnh mới). Chưa có mốc trước thì cột trái báo đây là mốc đầu tiên. Không tự mở camera hay
 * thư viện: màn gọi lo `onChup` và `onThuVien`.
 */
export function SoSanhAnh({
  anhMocTruoc,
  tuanMoi,
  anh,
  nhanAnh,
  onChup,
  onThuVien,
  onXoa,
}: {
  anhMocTruoc?: AnhCoTheMoc;
  tuanMoi: number;
  anh?: string;
  /** Nhãn truy cập của ảnh mới đã thêm. */
  nhanAnh: string;
  onChup(): void;
  onThuVien(): void;
  onXoa(): void;
}) {
  const { colors } = useTheme();
  return (
    <View className="flex-row gap-3">
      <CotAnh nhan={anhMocTruoc ? `Tuần ${anhMocTruoc.tuan}` : undefined}>
        {anhMocTruoc ? (
          <Image
            source={{ uri: anhMocTruoc.uri }}
            accessibilityLabel={`Ảnh cơ thể tuần ${anhMocTruoc.tuan}`}
            resizeMode="cover"
            className="size-full rounded-xl bg-secondary"
          />
        ) : (
          <View className="size-full items-center justify-center gap-2 rounded-xl bg-secondary px-3">
            <Icon name="image" size={28} color={colors.muted} />
            <Text variant="muted" className="text-center text-sm">
              Đây là mốc đầu tiên
            </Text>
          </View>
        )}
      </CotAnh>

      <CotAnh nhan={anh ? `Tuần ${tuanMoi}` : undefined}>
        {anh ? (
          <View className="size-full overflow-hidden rounded-xl bg-secondary">
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
          <View className="size-full gap-3">
            <OThemAnh icon="camera" nhan="Chụp ảnh" onPress={onChup} />
            <OThemAnh icon="image" nhan="Chọn từ thư viện" onPress={onThuVien} />
          </View>
        )}
      </CotAnh>
    </View>
  );
}
