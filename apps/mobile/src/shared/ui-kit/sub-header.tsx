import {
  Pressable,
  StyleSheet,
  Text,
  View,
  type StyleProp,
  type ViewStyle,
} from 'react-native';
import { Icon } from '../icons';
import { theme } from '../theme/tokens';

export interface SubHeaderConfig {
  title: string;
  onBack?(): void;
  action?: { label: string; onPress(): void; disabled?: boolean };
  style?: StyleProp<ViewStyle>;
  titleSize?: number;
  backIconSize?: number;
}

/** Thanh phụ dưới AppHeader: nút back, tiêu đề, nút hành động dạng pill. */
export function SubHeader({
  title,
  onBack,
  action,
  style,
  titleSize,
  backIconSize,
}: SubHeaderConfig) {
  const iconSize = backIconSize ?? theme.iconSize.lg - 2;
  const resolvedTitleSize = titleSize ?? theme.font.size.xl - 2;

  return (
    <View style={[styles.subHeader, style]}>
      <Pressable
        accessibilityRole="button"
        accessibilityLabel="Quay lại"
        onPress={onBack}
        hitSlop={8}
      >
        <Icon name="back" size={iconSize} />
      </Pressable>
      <Text
        accessibilityRole="header"
        style={[styles.title, { fontSize: resolvedTitleSize }]}
      >
        {title}
      </Text>
      {action ? (
        <Pressable
          accessibilityRole="button"
          accessibilityLabel={action.label}
          accessibilityState={{ disabled: action.disabled }}
          disabled={action.disabled}
          onPress={action.onPress}
          style={[styles.pill, action.disabled && { opacity: 0.5 }]}
        >
          <Text style={styles.pillText}>{action.label}</Text>
        </Pressable>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  subHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.lg,
    paddingHorizontal: theme.spacing.lg - 2,
    paddingTop: theme.spacing.md + 4, 
    paddingBottom: theme.spacing.sm - 3,
  },
  title: {
    flex: 1,
    fontSize: theme.font.size.xl,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.text,
  },
  pill: {
    backgroundColor: theme.colors.primary,
    paddingHorizontal: 28,
    paddingVertical: 12,
    borderRadius: theme.radius.chip,
  },
  pillText: {
    color: theme.colors.onPrimary,
    fontWeight: theme.font.weight.bold,
    fontSize: theme.font.size.md,
  },
});
