import { useCallback, useState } from 'react';
import { uploadMultipartFile } from './file-uploader';
import { FilePurpose, FileUploadResult, UploadOptions, UploadState } from './file.types';

export interface UseFileUploadReturn {
  state: UploadState;
  progress: number;
  result: FileUploadResult | null;
  error: Error | null;
  upload: (
    localUri: string,
    purpose?: FilePurpose,
    options?: UploadOptions,
  ) => Promise<FileUploadResult>;
  reset: () => void;
}

export type UseImageUploadReturn = UseFileUploadReturn;

export function useFileUpload(): UseFileUploadReturn {
  const [state, setState] = useState<UploadState>('idle');
  const [progress, setProgress] = useState<number>(0);
  const [result, setResult] = useState<FileUploadResult | null>(null);
  const [error, setError] = useState<Error | null>(null);

  const reset = useCallback(() => {
    setState('idle');
    setProgress(0);
    setResult(null);
    setError(null);
  }, []);

  const upload = useCallback(
    async (
      localUri: string,
      purpose: FilePurpose = 'avatar',
      options?: UploadOptions,
    ): Promise<FileUploadResult> => {
      try {
        setState('compressing');
        setProgress(10);

        setState('uploading');
        const uploadRes = await uploadMultipartFile(localUri, purpose, {
          ...options,
          onProgress: (p) => {
            setProgress(p);
            options?.onProgress?.(p);
          },
        });

        setProgress(100);
        setState('done');
        setResult(uploadRes);
        return uploadRes;
      } catch (err: unknown) {
        const errorObj = err instanceof Error ? err : new Error(String(err));
        setError(errorObj);
        setState('error');
        throw errorObj;
      }
    },
    [],
  );

  return {
    state,
    progress,
    result,
    error,
    upload,
    reset,
  };
}

export const useImageUpload = useFileUpload;
