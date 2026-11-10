import { useCallback, useMemo, useRef, useState } from 'react';
import type { ComposerPayload, MediaItem } from '../types';

export const MAX_MEDIA = 10;

export interface ComposerStateOptions {
  initialMedia?: MediaItem[];
  initialTopicIds?: string[];
  initialVisibilityId: string;
  mediaPool: string[];
  maxMedia?: number;
}

/**
 * Toàn bộ state của màn Đăng bài. Thay hook này bằng bản gọi API sau này mà không đổi UI.
 */
export function useComposerState({
  initialMedia = [],
  initialTopicIds = [],
  initialVisibilityId,
  mediaPool,
  maxMedia = MAX_MEDIA,
}: ComposerStateOptions) {
  const [text, setText] = useState('');
  const [media, setMedia] = useState<MediaItem[]>(initialMedia);
  const [topicIds, setTopicIds] = useState<string[]>(initialTopicIds);
  const [visibilityId, setVisibilityId] = useState(initialVisibilityId);
  const counter = useRef(0);

  const addMedia = useCallback(() => {
    setMedia((prev) => {
      if (prev.length >= maxMedia || mediaPool.length === 0) return prev;
      const n = counter.current++;
      return [...prev, { id: `media-${n}`, uri: mediaPool[n % mediaPool.length] as string }];
    });
  }, [maxMedia, mediaPool]);

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
