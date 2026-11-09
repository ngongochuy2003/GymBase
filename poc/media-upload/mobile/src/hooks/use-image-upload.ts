import { useCallback, useRef, useState } from 'react';
import { uploadImage } from '../services/image-uploader';
import {
  ConfirmUploadResponse,
  MediaPurpose,
  UploadOptions,
  UploadState,
} from '../types/media.types';

export interface UseImageUploadReturn {
  state: UploadState;
  progress: number;
  error: Error | null;
  result: ConfirmUploadResponse | null;
  upload: (
    uri: string,
    purpose: MediaPurpose,
    options?: UploadOptions
  ) => Promise<ConfirmUploadResponse | null>;
  cancel: () => void;
  reset: () => void;
}

/**
 * Custom hook quản lý trạng thái upload ảnh phía Mobile.
 * Cung cấp thông tin tiến trình thực tế, lỗi và chức năng hủy luồng upload.
 */
export function useImageUpload(defaultOptions?: UploadOptions): UseImageUploadReturn {
  const [state, setState] = useState<UploadState>('idle');
  const [progress, setProgress] = useState<number>(0);
  const [error, setError] = useState<Error | null>(null);
  const [result, setResult] = useState<ConfirmUploadResponse | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);

  const cancel = useCallback(() => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
      abortControllerRef.current = null;
      setState('error');
      setError(new Error('Upload đã bị người dùng hủy.'));
    }
  }, []);

  const reset = useCallback(() => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
      abortControllerRef.current = null;
    }
    setState('idle');
    setProgress(0);
    setError(null);
    setResult(null);
  }, []);

  const upload = useCallback(
    async (
      uri: string,
      purpose: MediaPurpose,
      options?: UploadOptions
    ): Promise<ConfirmUploadResponse | null> => {
      reset();

      const controller = new AbortController();
      abortControllerRef.current = controller;

      try {
        setState('compressing');
        setProgress(5);

        const mergedOptions: UploadOptions = {
          ...defaultOptions,
          ...options,
          signal: controller.signal,
          onProgress: (percent) => {
            // Map tiến trình S3 PUT vào khoảng 10% - 90%
            const mappedProgress = Math.min(90, Math.max(10, Math.round(10 + percent * 0.8)));
            setProgress(mappedProgress);
            setState('uploading');
            options?.onProgress?.(mappedProgress);
          },
        };

        const res = await uploadImage(uri, purpose, mergedOptions);

        setState('confirming');
        setProgress(95);

        setResult(res);
        setState('done');
        setProgress(100);

        return res;
      } catch (err: unknown) {
        if (controller.signal.aborted) {
          // Bỏ qua nếu đã chủ động hủy
          return null;
        }

        const e = err instanceof Error ? err : new Error(String(err));
        setError(e);
        setState('error');
        return null;
      } finally {
        abortControllerRef.current = null;
      }
    },
    [defaultOptions, reset]
  );

  return {
    state,
    progress,
    error,
    result,
    upload,
    cancel,
    reset,
  };
}
