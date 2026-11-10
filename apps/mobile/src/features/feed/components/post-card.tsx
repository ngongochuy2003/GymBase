import type { ReactNode } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { Icon } from '@/shared/icons';
import { theme } from '@/shared/theme/tokens';
import { RemoteImage, Surface } from '@/shared/ui-kit';

export interface PostCardAction {
  id: string;
  icon: string;
  activeIcon?: string;
  count?: number;
  active?: boolean;
  align?: 'start' | 'end';
  label?: string;
  onPress(): void;
}

export interface PostCardProps {
  author: { name: string; avatarUri?: string };
  timeLabel: string;
  content: string;
  imageUri?: string;
  actions: PostCardAction[];
  onMorePress?(): void;
  /** Ghi đè cách vẽ từng action. */
  renderAction?(action: PostCardAction): ReactNode;
}

function formatCount(value: number): string {
  return value.toLocaleString('en-US');
}

function DefaultAction({ action }: { action: PostCardAction }) {
  const name = action.active && action.activeIcon ? action.activeIcon : action.icon;
  const color = action.active && action.id === 'like' ? theme.colors.like : theme.colors.text;
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={action.label ?? action.id}
      accessibilityState={{ selected: action.active }}
      onPress={action.onPress}
      hitSlop={8}
      style={styles.action}
    >
      <Icon name={name} size={theme.iconSize.lg} color={color} />
      {action.count !== undefined ? (
        <Text style={styles.count}>{formatCount(action.count)}</Text>
      ) : null}
    </Pressable>
  );
}

export function PostCard({
  author,
  timeLabel,
  content,
  imageUri,
  actions,
  onMorePress,
  renderAction,
}: PostCardProps) {
  const draw = (action: PostCardAction) => (
    <View key={action.id}>
      {renderAction ? renderAction(action) : <DefaultAction action={action} />}
    </View>
  );
  return (
    <Surface>
      <View style={styles.head}>
        <RemoteImage uri={author.avatarUri} circle style={styles.avatar} />
        <View style={styles.meta}>
          <Text style={styles.name}>{author.name}</Text>
          <Text style={styles.time}>{timeLabel}</Text>
        </View>
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Tùy chọn"
          onPress={onMorePress}
          hitSlop={8}
        >
          <Icon name="more" size={theme.iconSize.lg} />
        </Pressable>
      </View>
      <Text style={styles.content}>{content}</Text>
      {imageUri ? (
        <RemoteImage uri={imageUri} accessibilityLabel="Ảnh bài viết" style={styles.image} />
      ) : null}
      <View style={styles.actions}>
        <View style={styles.group}>{actions.filter((a) => a.align !== 'end').map(draw)}</View>
        <View style={styles.group}>{actions.filter((a) => a.align === 'end').map(draw)}</View>
      </View>
    </Surface>
  );
}

const styles = StyleSheet.create({
  head: { flexDirection: 'row', alignItems: 'center', gap: theme.spacing.md },
  avatar: { width: 46, height: 46 },
  meta: { flex: 1 },
  name: {
    fontSize: theme.font.size.md,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.text,
  },
  time: { fontSize: theme.font.size.sm, color: theme.colors.muted },
  content: { fontSize: theme.font.size.md, lineHeight: 23, color: theme.colors.text },
  image: { width: '100%', aspectRatio: 16 / 10 },
  actions: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' },
  group: { flexDirection: 'row', alignItems: 'center', gap: theme.spacing.xl },
  action: { flexDirection: 'row', alignItems: 'center', gap: theme.spacing.sm },
  count: { fontSize: theme.font.size.md, color: theme.colors.text },
});
