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
      {/* Giảm kích thước action đi 2px theo yêu cầu (từ 26px xuống 24px) */}
      <Icon name={name} size={theme.iconSize.lg - 4} color={color} />
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
    <Surface style={styles.card}>
      {/* 1. Header (Avatar, Tên, Thời gian, Nút tùy chọn): căn padding riêng 4-8px */}
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
          <Icon name="more" size={theme.iconSize.md} />
        </Pressable>
      </View>

      {/* 2. Nội dung Text bài viết: căn padding riêng 4-8px */}
      {content ? <Text style={styles.content}>{content}</Text> : null}

      {/* 3. Ảnh bài viết: full 100% chiều ngang màn hình, không bo góc */}
      {imageUri ? (
        <RemoteImage
          uri={imageUri}
          accessibilityLabel="Ảnh bài viết"
          style={styles.image}
          resizeMode="cover"
        />
      ) : null}

      {/* 4. Hàng nút Action (tim, comment, share, lưu): căn padding riêng 4-8px */}
      <View style={styles.actions}>
        <View style={styles.group}>{actions.filter((a) => a.align !== 'end').map(draw)}</View>
        <View style={styles.group}>{actions.filter((a) => a.align === 'end').map(draw)}</View>
      </View>
    </Surface>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: theme.colors.card,
    borderTopWidth: 0,
    borderLeftWidth: 0,
    borderRightWidth: 0,
    // Dòng gạch ngang ngăn cách giữa các card bài viết 2px
    borderBottomWidth: 2,
    borderBottomColor: theme.colors.border,
    paddingHorizontal: 0,
    paddingTop: theme.spacing.xs,
    paddingBottom: theme.spacing.sm-3,
    gap: theme.spacing.xs,
    borderRadius: 0,
    shadowOpacity: 0,
    elevation: 0,
  },
  head: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.md,
    paddingHorizontal: theme.spacing.sm, // Căn lề riêng 8px
    paddingTop: theme.spacing.xs,
  },
  avatar: { width: 36, height: 36, borderRadius: 18, overflow: 'hidden' },
  meta: { flex: 1 },
  name: {
    fontSize: theme.font.size.md,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.text,
  },
  time: { fontSize: theme.font.size.xs-2.3, color: theme.colors.muted },
  content: {
    fontSize: theme.font.size.md-1,
    lineHeight: 22,
    color: theme.colors.text,
    paddingHorizontal: theme.spacing.sm, // Căn lề riêng 8px
    paddingVertical: 2,
  },
  image: {
    width: '100%',
    aspectRatio: 11 / 14,
    borderRadius: 0,
  },
  actions: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: theme.spacing.sm+2, // Căn lề riêng 8px
    paddingTop: theme.spacing.xs,
    paddingBottom: 1,
  },
  group: { flexDirection: 'row', alignItems: 'center', gap: theme.spacing.lg },
  action: { flexDirection: 'row', alignItems: 'center', gap: theme.spacing.xs+2 },
  count: { fontSize: theme.font.size.sm, color: theme.colors.text },
});
