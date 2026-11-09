export type MediaPurpose = 'avatar' | 'gym_photo' | 'progress_photo' | 'post_image';

export interface UploadUrlRequest {
  purpose: MediaPurpose;
  content_type: string;
  size_bytes: number;
}

export interface UploadUrlResponse {
  upload_id: string;
  presigned_put_url: string;
  staging_key: string;
  expires_in_seconds: number;
}

export interface ConfirmUploadRequest {
  upload_id: string;
  staging_key: string;
  purpose: MediaPurpose;
}

export interface ImageVariantUrls {
  thumb: string;
  medium: string;
  large: string;
}

export interface ConfirmUploadResponse {
  media_id: string;
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
