import type { ReactNode } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { Icon } from '@/shared/icons';
import { theme } from '@/shared/theme/tokens';

export interface ToolbarAction {
  id: string;
  label: string;
  icon: string;
  onPress(): void;
}

export interface ToolbarActionsProps {
  actions: ToolbarAction[];
  renderItem?(action: ToolbarAction): ReactNode;
}

/** Thanh công cụ ngang, các mục ngăn cách bằng vạch dọc. */
export function ToolbarActions({ actions, renderItem }: ToolbarActionsProps) {
  return (
    <View style={styles.row}>
      {actions.map((action, index) => (
        <View key={action.id} style={styles.cell}>
          {index > 0 ? <View style={styles.divider} /> : null}
          {renderItem ? (
            renderItem(action)
          ) : (
            <Pressable
              accessibilityRole="button"
              accessibilityLabel={action.label}
              onPress={action.onPress}
              style={styles.item}
            >
              <Icon name={action.icon} size={theme.iconSize.md} />
              <Text style={styles.label}>{action.label}</Text>
            </Pressable>
          )}
        </View>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: 'row',
    borderTopWidth: 1,
    borderTopColor: theme.colors.border,
    paddingTop: theme.spacing.md,
  },
  cell: { flex: 1, flexDirection: 'row', alignItems: 'center' },
  divider: { width: 1, height: 28, backgroundColor: theme.colors.border },
  item: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: theme.spacing.sm,
    paddingVertical: theme.spacing.xs,
  },
  label: { fontSize: theme.font.size.sm, color: theme.colors.text },
});
