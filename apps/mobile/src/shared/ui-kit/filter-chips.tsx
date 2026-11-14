import type { ReactNode } from 'react';
import { ScrollView, StyleSheet } from 'react-native';
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

/** Hàng chip cuộn ngang, chọn một. */
export function FilterChips({ items, activeId, onChange, renderItem }: FilterChipsProps) {
  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      contentContainerStyle={styles.row}
      style={styles.scroll}
    >
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
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  scroll: { flexGrow: 0, marginHorizontal: -theme.spacing.xl },
  row: { gap: theme.spacing.sm, paddingHorizontal: theme.spacing.xl },
});
