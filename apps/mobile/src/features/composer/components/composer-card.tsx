import { Pressable, StyleSheet, Text, TextInput, View } from 'react-native';
import { Icon } from '@/shared/icons';
import { theme } from '@/shared/theme/tokens';
import { RemoteImage, Surface } from '@/shared/ui-kit';
import type { MediaItem } from '../types';
import { MediaGrid, type MediaGridProps } from './media-grid';
import { ToolbarActions, type ToolbarAction } from './toolbar-actions';

export interface ComposerCardProps {
  author: { name: string; avatarUri?: string };
  visibility: { label: string; icon: string };
  onVisibilityPress?(): void;
  text: string;
  onTextChange(text: string): void;
  placeholder?: string;
  media: MediaItem[];
  maxMedia?: number;
  onAddMedia(): void;
  onRemoveMedia(id: string): void;
  toolbarActions: ToolbarAction[];
  renderMediaItem?: MediaGridProps['renderItem'];
}

export function ComposerCard({
  author,
  visibility,
  onVisibilityPress,
  text,
  onTextChange,
  placeholder = 'Chia sẻ buổi tập hôm nay...',
  media,
  maxMedia = 10,
  onAddMedia,
  onRemoveMedia,
  toolbarActions,
  renderMediaItem,
}: ComposerCardProps) {
  return (
    <Surface>
      <View style={styles.head}>
        <RemoteImage uri={author.avatarUri} circle style={styles.avatar} />
        <View style={{ gap: 2 }}>
          <Text style={styles.name}>{author.name}</Text>
          <Pressable
            accessibilityRole="button"
            accessibilityLabel={`Quyền xem: ${visibility.label}`}
            onPress={onVisibilityPress}
            style={styles.visibility}
          >
            <Icon name={visibility.icon} size={theme.iconSize.sm} color={theme.colors.muted} />
            <Text style={styles.visibilityText}>{visibility.label}</Text>
            <Icon name="chevron-down" size={theme.iconSize.sm} color={theme.colors.muted} />
          </Pressable>
        </View>
      </View>
      <TextInput
        accessibilityLabel="Nội dung bài viết"
        multiline
        placeholder={placeholder}
        placeholderTextColor={theme.colors.muted}
        value={text}
        onChangeText={onTextChange}
        style={styles.input}
      />
      <MediaGrid
        items={media}
        max={maxMedia}
        onAdd={onAddMedia}
        onRemove={onRemoveMedia}
        renderItem={renderMediaItem}
      />
      <ToolbarActions actions={toolbarActions} />
    </Surface>
  );
}

const styles = StyleSheet.create({
  head: { flexDirection: 'row', alignItems: 'center', gap: theme.spacing.md },
  avatar: { width: 48, height: 48 },
  name: {
    fontSize: theme.font.size.md,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.text,
  },
  visibility: { flexDirection: 'row', alignItems: 'center', gap: 6 },
  visibilityText: { fontSize: theme.font.size.sm, color: theme.colors.muted },
  input: {
    minHeight: 96,
    fontSize: theme.font.size.lg,
    color: theme.colors.text,
    textAlignVertical: 'top',
    paddingVertical: theme.spacing.sm,
  },
});
