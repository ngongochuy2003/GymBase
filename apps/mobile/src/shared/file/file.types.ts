export type FilePurpose = 'avatar' | 'gym_photo' | 'progress' | 'post';

export type MediaPurpose = FilePurpose;

export interface FileUploadResult {
  file_id: string;
  original_name: string;
  content_type: string;
  size: number;
  purpose: string;
  provider: string;
  blurhash?: string;
  content_url: string;
}

export interface ImageVariantUrls {
  thumb: string;
  medium: string;
  large: string;
}

export interface ConfirmUploadResponse {
  file_id: string;
  content_hash: string;
  urls: ImageVariantUrls;
  width: number;
  height: number;
  blurhash: string;
  total_size_bytes: number;
  is_deduplicated: boolean;
}

export type UploadState = 'idle' | 'compressing' | 'uploading' | 'confirming' | 'done' | 'error';

export interface UploadOptions {
  apiBaseUrl?: string;
  authToken?: string;
  onProgress?: (percent: number) => void;
  signal?: AbortSignal;
  maxRetries?: number;
  initialRetryDelayMs?: number;
}
