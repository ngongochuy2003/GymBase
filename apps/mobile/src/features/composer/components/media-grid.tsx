import React, { useState } from 'react';
import {
  Modal,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { isVideoUrl } from '@/shared/file';
import { Icon } from '@/shared/icons';
import { theme } from '@/shared/theme/tokens';
import { PostVideoPlayer, RemoteImage } from '@/shared/ui-kit';
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
  const [previewVideoUri, setPreviewVideoUri] = useState<string | null>(null);

  return (
    <View style={styles.grid}>
      {items.map((item) => {
        const remove = () => onRemove(item.id);
        if (renderItem) return <View key={item.id}>{renderItem(item, remove)}</View>;

        const isVideo = item.type === 'video' || isVideoUrl(item.uri);

        return (
          <View key={item.id} style={styles.tile}>
            {/* Nội dung Media Thumbnail */}
            {isVideo && Platform.OS === 'web' ? (
              // @ts-ignore: React Native Web video element
              <video
                src={item.uri}
                preload="metadata"
                muted
                playsInline
                style={styles.webVideoThumbnail}
              />
            ) : isVideo ? (
              <PostVideoPlayer
                uri={item.uri}
                aspectRatio={undefined}
                autoPlay={false}
                loop={false}
                style={styles.fill}
              />
            ) : (
              <RemoteImage
                uri={item.uri}
                accessibilityLabel="Ảnh đã chọn"
                style={styles.fill}
              />
            )}

            {/* Lớp phủ tương tác cho Video: Bấm vào để xem trước video */}
            {isVideo && (
              <Pressable
                testID={`preview-video-${item.id}`}
                accessibilityRole="button"
                accessibilityLabel="Xem trước video đã chọn"
                onPress={() => setPreviewVideoUri(item.uri)}
                style={styles.videoOverlay}
              >
                <View style={styles.playBadge}>
                  <Icon name="play" size={20} color={theme.colors.onPrimary} />
                </View>
                <View style={styles.videoLabel}>
                  <Text style={styles.videoLabelText}>VIDEO</Text>
                </View>
              </Pressable>
            )}

            {/* Nút Xóa */}
            <Pressable
              accessibilityRole="button"
              accessibilityLabel={isVideo ? 'Xóa video' : 'Xóa ảnh'}
              onPress={remove}
              hitSlop={8}
              style={styles.remove}
            >
              <Icon name="close" size={16} color={theme.colors.onPrimary} />
            </Pressable>
          </View>
        );
      })}

      {/* Nút Thêm ảnh/video */}
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

      {/* Modal Xem trước Video đã chọn */}
      {previewVideoUri && (
        <Modal
          visible={Boolean(previewVideoUri)}
          transparent
          animationType="fade"
          onRequestClose={() => setPreviewVideoUri(null)}
          statusBarTranslucent
        >
          <View style={styles.modalBackdrop}>
            <SafeAreaView style={styles.modalSafeArea}>
              <View style={styles.modalHeader}>
                <Text style={styles.modalTitle}>Xem trước video</Text>
                <Pressable
                  testID="close-video-preview"
                  accessibilityRole="button"
                  accessibilityLabel="Đóng xem trước"
                  onPress={() => setPreviewVideoUri(null)}
                  hitSlop={12}
                  style={styles.modalCloseBtn}
                >
                  <Icon name="close" size={24} color={theme.colors.onPrimary} />
                </Pressable>
              </View>

              <View style={styles.modalPlayerContainer}>
                <PostVideoPlayer
                  uri={previewVideoUri}
                  aspectRatio={undefined}
                  autoPlay={true}
                  loop={true}
                  style={styles.modalVideoPlayer}
                />
              </View>
            </SafeAreaView>
          </View>
        </Modal>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  grid: { flexDirection: 'row', flexWrap: 'wrap', gap: theme.spacing.md },
  tile: {
    width: '46%',
    aspectRatio: 1.38,
    borderRadius: theme.radius.image,
    overflow: 'hidden',
    position: 'relative',
    backgroundColor: theme.colors.surfaceMuted,
  },
  fill: { width: '100%', height: '100%' },
  webVideoThumbnail: {
    width: '100%',
    height: '100%',
    objectFit: 'cover',
    backgroundColor: theme.colors.surfaceMuted,
  },
  videoOverlay: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    alignItems: 'center',
    justifyContent: 'center',
  },
  playBadge: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: theme.colors.overlay,
    alignItems: 'center',
    justifyContent: 'center',
  },
  videoLabel: {
    position: 'absolute',
    bottom: theme.spacing.sm,
    left: theme.spacing.sm,
    backgroundColor: theme.colors.overlay,
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 4,
  },
  videoLabelText: {
    fontSize: theme.font.size.xs,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.onPrimary,
    letterSpacing: 0.5,
  },
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
    zIndex: 10,
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

  // Modal styles
  modalBackdrop: {
    flex: 1,
    backgroundColor: 'rgba(0, 0, 0, 0.92)',
    justifyContent: 'center',
  },
  modalSafeArea: {
    flex: 1,
    justifyContent: 'space-between',
  },
  modalHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: theme.spacing.lg,
    paddingVertical: theme.spacing.md,
  },
  modalTitle: {
    fontSize: theme.font.size.md,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.onPrimary,
  },
  modalCloseBtn: {
    width: 36,
    height: 36,
    borderRadius: 18,
    backgroundColor: theme.colors.overlay,
    alignItems: 'center',
    justifyContent: 'center',
  },
  modalPlayerContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: theme.spacing.md,
    paddingBottom: theme.spacing.xl,
  },
  modalVideoPlayer: {
    width: '100%',
    height: '80%',
    maxHeight: 520,
    borderRadius: theme.radius.image,
  },
});
