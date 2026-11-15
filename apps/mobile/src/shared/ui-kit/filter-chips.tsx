import type { ReactNode } from 'react';
import { StyleSheet, View } from 'react-native';
import { Chip } from './chip';
import { theme } from '../theme/tokens';

export interface FilterChipItem {
  id: string;
  label: string;
  icon?: string;
}

export interface FilterChipsProps {
  items: FilterChipItem[];
  activeId: string;
  onChange(id: string): void;
  /** Ghi đè cách vẽ từng chip. */
  renderItem?(item: FilterChipItem, active: boolean, select: () => void): ReactNode;
}

/** Thanh subheader chip cố định, không cuộn ngang, cách đều giữa các phần tử và 2 bên lề. */
export function FilterChips({ items, activeId, onChange, renderItem }: FilterChipsProps) {
  return (
    <View style={styles.container}>
      {items.map((item) => {
        const active = item.id === activeId;
        const select = () => onChange(item.id);
        return renderItem ? (
          renderItem(item, active, select)
        ) : (
          <Chip
            key={item.id}
            label={item.label}
            icon={item.icon}
            selected={active}
            onPress={select}
          />
        );
      })}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-evenly',
    width: '100%',
    paddingVertical: theme.spacing.xs,
    backgroundColor: theme.colors.background,
  },
});
