import type { ReactNode } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { Icon } from '@/shared/icons';
import { theme } from '@/shared/theme/tokens';
import { Surface } from '@/shared/ui-kit';

export interface VisibilityOption {
  id: string;
  label: string;
  description: string;
  icon: string;
}

export interface VisibilitySelectorProps {
  title?: string;
  subtitle?: string;
  options: VisibilityOption[];
  value: string;
  onChange(id: string): void;
  renderItem?(option: VisibilityOption, selected: boolean, select: () => void): ReactNode;
}

export function VisibilitySelector({
  title = 'Quyền xem',
  subtitle = 'Ai có thể xem bài viết này?',
  options,
  value,
  onChange,
  renderItem,
}: VisibilitySelectorProps) {
  return (
    <Surface>
      <View style={{ gap: 2 }}>
        <Text style={styles.title}>{title}</Text>
        <Text style={styles.subtitle}>{subtitle}</Text>
      </View>
      <View style={{ gap: theme.spacing.xs }}>
        {options.map((option) => {
          const selected = option.id === value;
          const select = () => onChange(option.id);
          if (renderItem)
            return <View key={option.id}>{renderItem(option, selected, select)}</View>;
          return (
            <Pressable
              key={option.id}
              accessibilityRole="radio"
              accessibilityLabel={option.label}
              accessibilityState={{ selected }}
              onPress={select}
              style={[styles.option, selected && { backgroundColor: theme.colors.highlight }]}
            >
              <Icon name={option.icon} size={theme.iconSize.lg} />
              <View style={styles.text}>
                <Text style={styles.label}>{option.label}</Text>
                <Text style={styles.subtitle}>{option.description}</Text>
              </View>
              <View style={[styles.radio, selected && { borderColor: theme.colors.primary }]}>
                {selected ? <View style={styles.dot} /> : null}
              </View>
            </Pressable>
          );
        })}
      </View>
    </Surface>
  );
}

const styles = StyleSheet.create({
  title: {
    fontSize: theme.font.size.md,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.text,
  },
  subtitle: { fontSize: theme.font.size.sm, color: theme.colors.muted },
  option: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.md,
    padding: theme.spacing.md,
    borderRadius: theme.radius.image,
  },
  text: { flex: 1, gap: 2 },
  label: {
    fontSize: theme.font.size.md,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.text,
  },
  radio: {
    width: 24,
    height: 24,
    borderRadius: 12,
    borderWidth: 2,
    borderColor: theme.colors.muted,
    alignItems: 'center',
    justifyContent: 'center',
  },
  dot: { width: 12, height: 12, borderRadius: 6, backgroundColor: theme.colors.primary },
});
