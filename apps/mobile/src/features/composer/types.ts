export interface MediaItem {
  id: string;
  uri: string;
  type?: 'image' | 'video';
  mimeType?: string;
  /** Dung lượng tệp (byte) do bộ chọn báo; để app kiểm giới hạn trước khi upload. */
  sizeBytes?: number;
}

export interface ComposerPayload {
  content: string;
  mediaUris: string[];
  mediaItems?: MediaItem[];
  topicIds: string[];
  visibilityId: string;
}
