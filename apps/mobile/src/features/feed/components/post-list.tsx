import { useState } from 'react';
import { View } from 'react-native';
import { theme } from '@/shared/theme/tokens';
import type { FeedPostData, PostActionTemplate } from '../types';
import { PostCard, type PostCardAction, type PostCardProps } from './post-card';

export interface PostListProps {
  posts: FeedPostData[];
  actionTemplates: PostActionTemplate[];
  onActionPress?(postId: string, actionId: string): void;
  renderAction?: PostCardProps['renderAction'];
}

/** Section `postList`: dựng PostCard và tự quản lý bật/tắt (tim, bookmark) từ mẫu action. */
export function PostList({ posts, actionTemplates, onActionPress, renderAction }: PostListProps) {
  const [overrides, setOverrides] = useState<Record<string, boolean>>({});
  const key = (postId: string, actionId: string) => `${postId}:${actionId}`;

  return (
    <View style={{ gap: theme.spacing.lg }}>
      {posts.map((post) => {
        const actions: PostCardAction[] = actionTemplates.map((template) => {
          const initial = post.initialActive?.[template.id] ?? false;
          const active = template.toggle
            ? (overrides[key(post.id, template.id)] ?? initial)
            : false;
          const base = post.counts?.[template.id];
          const count =
            template.showCount && base !== undefined
              ? base + (active === initial ? 0 : active ? 1 : -1)
              : undefined;
          return {
            id: template.id,
            icon: template.icon,
            activeIcon: template.activeIcon,
            align: template.align,
            active,
            count,
            onPress: () => {
              if (template.toggle)
                setOverrides((prev) => ({ ...prev, [key(post.id, template.id)]: !active }));
              onActionPress?.(post.id, template.id);
            },
          };
        });
        return (
          <PostCard
            key={post.id}
            author={post.author}
            timeLabel={post.timeLabel}
            content={post.content}
            imageUri={post.imageUri}
            actions={actions}
            renderAction={renderAction}
          />
        );
      })}
    </View>
  );
}
