import type { ReactNode } from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { theme } from '@/shared/theme/tokens';
import { Chip, Surface } from '@/shared/ui-kit';

export interface TopicItem {
  id: string;
  label: string;
  icon?: string;
}

export interface TopicSelectorProps {
  title?: string;
  subtitle?: string;
  topics: TopicItem[];
  selectedIds: string[];
  /** Chọn nhiều thay vì chọn đơn. */
  multiple?: boolean;
  onChange(ids: string[]): void;
  renderItem?(topic: TopicItem, selected: boolean, toggle: () => void): ReactNode;
}

export function TopicSelector({
  title = 'Chủ đề bài viết',
  subtitle = 'Chọn chủ đề phù hợp để dễ dàng khám phá.',
  topics,
  selectedIds,
  multiple = false,
  onChange,
  renderItem,
}: TopicSelectorProps) {
  const toggle = (id: string) => {
    if (multiple) {
      onChange(
        selectedIds.includes(id) ? selectedIds.filter((x) => x !== id) : [...selectedIds, id],
      );
    } else {
      onChange(selectedIds[0] === id ? [] : [id]);
    }
  };
  return (
    <Surface>
      <View style={{ gap: 2 }}>
        <Text style={styles.title}>{title}</Text>
        <Text style={styles.subtitle}>{subtitle}</Text>
      </View>
      <View style={styles.row}>
        {topics.map((topic) => {
          const selected = selectedIds.includes(topic.id);
          return renderItem ? (
            <View key={topic.id}>{renderItem(topic, selected, () => toggle(topic.id))}</View>
          ) : (
            <Chip
              key={topic.id}
              label={topic.label}
              icon={topic.icon}
              selected={selected}
              showCheck
              tone="soft"
              onPress={() => toggle(topic.id)}
            />
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
  row: { flexDirection: 'row', flexWrap: 'wrap', gap: theme.spacing.sm },
});
