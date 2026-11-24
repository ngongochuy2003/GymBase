import { useCallback, useMemo, useRef, useState } from 'react';
import * as ImagePicker from 'expo-image-picker';
import { isVideoUrl, kiemTraVideoKhiChon } from '@/shared/file';
import type { ComposerPayload, MediaItem } from '../types';

export const MAX_MEDIA = 10;

export interface ComposerStateOptions {
  initialMedia?: MediaItem[];
  initialTopicIds?: string[];
  initialVisibilityId: string;
  maxMedia?: number;
  onPermissionDenied?(message: string): void;
  /** Có tệp bị loại ngay lúc chọn (video quá lớn); `message` nói rõ lý do. */
  onMediaRejected?(message: string): void;
}

/**
 * Toàn bộ state của màn Đăng bài.
 * Khi thêm ảnh/video sẽ yêu cầu cấp quyền truy cập thư viện ảnh của thiết bị.
 */
export function useComposerState({
  initialMedia = [],
  initialTopicIds = [],
  initialVisibilityId,
  maxMedia = MAX_MEDIA,
  onPermissionDenied,
  onMediaRejected,
}: ComposerStateOptions) {
  const [text, setText] = useState('');
  const [media, setMedia] = useState<MediaItem[]>(initialMedia);
  const [topicIds, setTopicIds] = useState<string[]>(initialTopicIds);
  const [visibilityId, setVisibilityId] = useState(initialVisibilityId);
  const counter = useRef(0);

  const addMedia = useCallback(async () => {
    if (media.length >= maxMedia) return;

    // Yêu cầu quyền truy cập thư viện ảnh thiết bị
    const permission = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (!permission.granted || permission.status !== 'granted') {
      onPermissionDenied?.(
        'GymBase cần quyền truy cập thư viện ảnh để bạn có thể chọn ảnh hoặc video.',
      );
      return;
    }

    const remaining = maxMedia - media.length;
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ['images', 'videos'],
      allowsMultipleSelection: true,
      selectionLimit: remaining,
      quality: 1,
    });

    if (result.canceled || !result.assets || result.assets.length === 0) {
      return;
    }

    // Video quá lớn bị loại ngay lúc chọn, khỏi chờ tới lúc bấm Đăng mới biết.
    let rejectedMessage: string | null = null;
    const accepted = result.assets.filter((asset) => {
      const isVideo =
        asset.type === 'video' || isVideoUrl(asset.uri) || asset.mimeType?.startsWith('video/');
      const message = isVideo ? kiemTraVideoKhiChon(asset.fileSize) : null;
      if (message) rejectedMessage = message;
      return !message;
    });
    if (rejectedMessage) onMediaRejected?.(rejectedMessage);
    if (accepted.length === 0) return;

    const newItems: MediaItem[] = accepted.map((asset) => {
      const n = counter.current++;
      const isVideo =
        asset.type === 'video' ||
        isVideoUrl(asset.uri) ||
        asset.mimeType?.startsWith('video/');
      return {
        id: `media-${Date.now()}-${n}`,
        uri: asset.uri,
        type: isVideo ? 'video' : 'image',
        mimeType: asset.mimeType ?? (isVideo ? 'video/mp4' : 'image/jpeg'),
        sizeBytes: asset.fileSize,
      };
    });

    setMedia((prev) => [...prev, ...newItems].slice(0, maxMedia));
  }, [media.length, maxMedia, onPermissionDenied, onMediaRejected]);

  const removeMedia = useCallback(
    (id: string) => setMedia((prev) => prev.filter((m) => m.id !== id)),
    [],
  );

  const appendText = useCallback((value: string) => setText((prev) => prev + value), []);

  const payload = useMemo<ComposerPayload>(
    () => ({
      content: text.trim(),
      mediaUris: media.map((m) => m.uri),
      mediaItems: media,
      topicIds,
      visibilityId,
    }),
    [text, media, topicIds, visibilityId],
  );

  const canSubmit = payload.content.length > 0 || payload.mediaUris.length > 0;

  const reset = useCallback(() => {
    setText('');
    setMedia([]);
    setTopicIds(initialTopicIds);
  }, [initialTopicIds]);

  return {
    text,
    media,
    topicIds,
    visibilityId,
    maxMedia,
    setText,
    appendText,
    addMedia,
    removeMedia,
    setTopicIds,
    setVisibilityId,
    payload,
    canSubmit,
    reset,
  };
}

export type ComposerState = ReturnType<typeof useComposerState>;
