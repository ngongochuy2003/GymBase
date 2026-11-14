export interface MediaItem {
  id: string;
  uri: string;
}

export interface ComposerPayload {
  content: string;
  mediaUris: string[];
  topicIds: string[];
  visibilityId: string;
}
