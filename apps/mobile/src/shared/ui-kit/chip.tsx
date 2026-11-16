import { Pressable, StyleSheet, Text } from 'react-native';
import { Icon } from '../icons';
import { theme } from '../theme/tokens';

export interface ChipProps {
  label: string;
  icon?: string;
  selected?: boolean;
  /** Hiện dấu ✓ khi chọn (dùng cho chọn chủ đề). */
  showCheck?: boolean;
  /** `outlined`: nền card + viền (bộ lọc); `soft`: nền nhạt không viền (chủ đề). */
  tone?: 'outlined' | 'soft';
  onPress?(): void;
}

export function Chip({
  label,
  icon,
  selected = false,
  showCheck = false,
  tone = 'outlined',
  onPress,
}: ChipProps) {
  const fg = selected ? theme.colors.onPrimary : theme.colors.text;
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={label}
      accessibilityState={{ selected }}
      onPress={onPress}
      style={[
        styles.chip,
        selected
          ? { backgroundColor: theme.colors.primary, borderColor: theme.colors.primary }
          : tone === 'soft'
            ? { backgroundColor: theme.colors.surfaceMuted, borderColor: theme.colors.surfaceMuted }
            : { backgroundColor: theme.colors.card, borderColor: theme.colors.border },
      ]}
    >
      {icon ? <Icon name={icon} size={theme.iconSize.xs-1} color={fg} /> : null}
      <Text style={[styles.label, { color: fg }]}>{label}</Text>
      {selected && showCheck ? <Icon name="check" size={theme.iconSize.sm-1} color={fg} /> : null}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  chip: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.sm-1,
    paddingHorizontal: theme.spacing.md-3,
    paddingVertical: 8-3,
    borderRadius: theme.radius.chip,
    borderWidth: 1,
  },
  label: { fontSize: theme.font.size.xss, fontWeight: theme.font.weight.medium },
});
