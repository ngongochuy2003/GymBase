export interface MediaItem {
  id: string;
  uri: string;
  type?: 'image' | 'video';
  mimeType?: string;
}

export interface ComposerPayload {
  content: string;
  mediaUris: string[];
  mediaItems?: MediaItem[];
  topicIds: string[];
  visibilityId: string;
}
