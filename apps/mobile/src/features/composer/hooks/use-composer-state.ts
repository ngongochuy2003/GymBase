import { useCallback, useMemo, useRef, useState } from 'react';
import * as ImagePicker from 'expo-image-picker';
import type { ComposerPayload, MediaItem } from '../types';

export const MAX_MEDIA = 10;

export interface ComposerStateOptions {
  initialMedia?: MediaItem[];
  initialTopicIds?: string[];
  initialVisibilityId: string;
  maxMedia?: number;
  onPermissionDenied?(message: string): void;
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
      quality: 0.8,
    });

    if (result.canceled || !result.assets || result.assets.length === 0) {
      return;
    }

    const newItems: MediaItem[] = result.assets.map((asset) => {
      const n = counter.current++;
      return {
        id: `media-${Date.now()}-${n}`,
        uri: asset.uri,
      };
    });

    setMedia((prev) => [...prev, ...newItems].slice(0, maxMedia));
  }, [media.length, maxMedia, onPermissionDenied]);

  const removeMedia = useCallback(
    (id: string) => setMedia((prev) => prev.filter((m) => m.id !== id)),
    [],
  );

  const appendText = useCallback((value: string) => setText((prev) => prev + value), []);

  const payload = useMemo<ComposerPayload>(
    () => ({ content: text.trim(), mediaUris: media.map((m) => m.uri), topicIds, visibilityId }),
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
