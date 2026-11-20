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

/** Thanh subheader chip danh mục cuộn ngang, tách nhau ra thêm một chút và cho phép vuốt sang phải xem thêm. */
export function FilterChips({ items, activeId, onChange, renderItem }: FilterChipsProps) {
  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      contentContainerStyle={styles.scrollContent}
      style={styles.container}
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
  container: {
    width: '100%',
    backgroundColor: theme.colors.background,
  },
  scrollContent: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    paddingHorizontal: 10,
    paddingTop: 4,
    paddingBottom: 6,
  },
});
