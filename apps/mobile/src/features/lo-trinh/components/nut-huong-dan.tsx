import { Linking, Pressable } from 'react-native';
import { Text } from '@/components/ui/text';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import { laLinkHopLe } from '../utils/link-huong-dan';

/** Nút "Xem hướng dẫn" của một bài tập, mở link ngoài app; link trống hoặc sai thì không hiện gì. */
export function NutHuongDan({ tenBai, link }: { tenBai: string; link?: string }) {
  const { colors } = useTheme();
  if (!laLinkHopLe(link)) return null;
  return (
    <Pressable
      accessibilityRole="link"
      accessibilityLabel={`Xem hướng dẫn ${tenBai}`}
      hitSlop={6}
      onPress={() => void Linking.openURL(link.trim())}
      className="flex-row items-center gap-1.5 self-start py-1"
    >
      <Icon name="play" size={16} color={colors.primary} />
      <Text className="text-sm font-bold text-primary">Xem hướng dẫn</Text>
    </Pressable>
  );
}
