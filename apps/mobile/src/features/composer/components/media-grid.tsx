import { Pressable, StyleSheet, Text, View } from 'react-native';
import { Icon } from '@/shared/icons';
import { theme } from '@/shared/theme/tokens';
import { RemoteImage } from '@/shared/ui-kit';
import type { MediaItem } from '../types';

export interface MediaGridProps {
  items: MediaItem[];
  max?: number;
  onAdd(): void;
  onRemove(id: string): void;
  /** Ghi đè cách vẽ một thumbnail. */
  renderItem?(item: MediaItem, remove: () => void): React.ReactNode;
}

export function MediaGrid({ items, max = 10, onAdd, onRemove, renderItem }: MediaGridProps) {
  return (
    <View style={styles.grid}>
      {items.map((item) => {
        const remove = () => onRemove(item.id);
        if (renderItem) return <View key={item.id}>{renderItem(item, remove)}</View>;
        return (
          <View key={item.id} style={styles.tile}>
            <RemoteImage uri={item.uri} accessibilityLabel="Ảnh đã chọn" style={styles.fill} />
            <Pressable
              accessibilityRole="button"
              accessibilityLabel="Xóa ảnh"
              onPress={remove}
              hitSlop={8}
              style={styles.remove}
            >
              <Icon name="close" size={16} color={theme.colors.onPrimary} />
            </Pressable>
          </View>
        );
      })}
      {items.length < max ? (
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Thêm ảnh/video"
          onPress={onAdd}
          style={[styles.tile, styles.add]}
        >
          <Icon name="add-square" size={32} color={theme.colors.muted} />
          <Text style={styles.addText}>Thêm ảnh/video</Text>
          <Text style={styles.addText}>(tối đa {max})</Text>
        </Pressable>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  grid: { flexDirection: 'row', flexWrap: 'wrap', gap: theme.spacing.md },
  tile: { width: '46%', aspectRatio: 1.38, borderRadius: theme.radius.image, overflow: 'hidden' },
  fill: { width: '100%', height: '100%' },
  remove: {
    position: 'absolute',
    top: theme.spacing.sm,
    right: theme.spacing.sm,
    width: 28,
    height: 28,
    borderRadius: 14,
    backgroundColor: theme.colors.overlay,
    alignItems: 'center',
    justifyContent: 'center',
  },
  add: {
    borderWidth: 1.5,
    borderStyle: 'dashed',
    borderColor: theme.colors.border,
    backgroundColor: theme.colors.surfaceMuted,
    alignItems: 'center',
    justifyContent: 'center',
    gap: 2,
  },
  addText: { fontSize: theme.font.size.sm, color: theme.colors.muted },
});
