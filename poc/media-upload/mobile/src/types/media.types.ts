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

// Chuẩn V1: File Storage (Local Storage & S3 Ready)
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

export interface TepTinMetadata {
  id: string;
  original_name: string;
  content_type: string;
  size: number;
  purpose: string;
  provider: string;
  status: string;
  blurhash?: string;
  created_at: string;
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
