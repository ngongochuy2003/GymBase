import {
  setCustomManipulator,
  uploadImage,
} from '../src/services/image-uploader';
import { ConfirmUploadResponse, UploadUrlResponse } from '../src/types/media.types';

// Mock XMLHttpRequest trong môi trường nodejs
class MockXMLHttpRequest {
  public upload = {
    onprogress: null as ((event: { lengthComputable: boolean; loaded: number; total: number }) => void) | null,
  };
  public status = 200;
  public statusText = 'OK';
  public onload: (() => void) | null = null;
  public onerror: (() => void) | null = null;
  public ontimeout: (() => void) | null = null;

  public static failCountBeforeSuccess = 0;
  public static currentAttempts = 0;
  public static shouldSimulateProgress = true;

  open(_method: string, _url: string) {
    MockXMLHttpRequest.currentAttempts++;
  }

  setRequestHeader(_header: string, _value: string) {}

  send(_body?: unknown) {
    setTimeout(() => {
      if (MockXMLHttpRequest.shouldSimulateProgress && this.upload.onprogress) {
        this.upload.onprogress({ lengthComputable: true, loaded: 50, total: 100 });
        this.upload.onprogress({ lengthComputable: true, loaded: 100, total: 100 });
      }

      if (MockXMLHttpRequest.currentAttempts <= MockXMLHttpRequest.failCountBeforeSuccess) {
        this.status = 500;
        this.statusText = 'Internal Server Error';
        if (this.onload) this.onload();
      } else {
        this.status = 200;
        this.statusText = 'OK';
        if (this.onload) this.onload();
      }
    }, 10);
  }

  abort() {
    this.status = 0;
  }
}

describe('image-uploader', () => {
  const originalFetch = global.fetch;
  const originalXHR = (global as unknown as { XMLHttpRequest: unknown }).XMLHttpRequest;

  beforeEach(() => {
    (global as unknown as { XMLHttpRequest: unknown }).XMLHttpRequest = MockXMLHttpRequest;
    MockXMLHttpRequest.failCountBeforeSuccess = 0;
    MockXMLHttpRequest.currentAttempts = 0;
    MockXMLHttpRequest.shouldSimulateProgress = true;

    // Mock Image Manipulator
    setCustomManipulator({
      manipulateAsync: jest.fn().mockResolvedValue({
        uri: 'file:///mock-compressed.jpg',
        width: 1920,
        height: 1080,
      }),
    });
  });

  afterEach(() => {
    global.fetch = originalFetch;
    (global as unknown as { XMLHttpRequest: unknown }).XMLHttpRequest = originalXHR;
  });

  it('hoàn thành toàn bộ luồng uploadImage (nén -> xin url -> PUT -> confirm) và báo progress', async () => {
    const mockUploadUrlRes: UploadUrlResponse = {
      upload_id: 'mock-upload-123',
      presigned_put_url: 'https://s3.mock/upload/staging.tmp',
      staging_key: 'staging/avatar/2026/11/mock.tmp',
      expires_in_seconds: 900,
    };

    const mockConfirmRes: ConfirmUploadResponse = {
      media_id: 'media-abc-789',
      content_hash: 'sha256-dummy-hash',
      urls: {
        thumb: 'https://cdn.gymbase.vn/thumb.webp',
        medium: 'https://cdn.gymbase.vn/medium.webp',
        large: 'https://cdn.gymbase.vn/large.webp',
      },
      width: 1920,
      height: 1080,
      blurhash: 'L6PZfSi_.AyE_3t7t7R**0o#DgR4',
      total_size_bytes: 45000,
      is_deduplicated: false,
    };

    // Mock fetch
    global.fetch = jest.fn((url: RequestInfo | URL) => {
      const urlStr = url.toString();
      if (urlStr.includes('/mock-compressed.jpg')) {
        return Promise.resolve({
          blob: () => Promise.resolve({ size: 1024 * 50, type: 'image/jpeg' }),
        } as unknown as Response);
      }
      if (urlStr.includes('/api/v1/media/upload-url')) {
        return Promise.resolve({
          ok: true,
          status: 200,
          json: () => Promise.resolve(mockUploadUrlRes),
        } as unknown as Response);
      }
      if (urlStr.includes('/api/v1/media/confirm')) {
        return Promise.resolve({
          ok: true,
          status: 200,
          json: () => Promise.resolve(mockConfirmRes),
        } as unknown as Response);
      }
      return Promise.reject(new Error(`Unhandled URL: ${urlStr}`));
    });

    const progressReports: number[] = [];
    const result = await uploadImage('file:///original.jpg', 'avatar', {
      apiBaseUrl: 'http://localhost:5000',
      onProgress: (percent) => progressReports.push(percent),
    });

    expect(result).toEqual(mockConfirmRes);
    expect(progressReports).toContain(50);
    expect(progressReports).toContain(100);
    expect(MockXMLHttpRequest.currentAttempts).toBe(1);
  });

  it('tự động retry với exponential backoff khi mạng lỗi và thành công ở lần thử tiếp theo', async () => {
    // Giả lập lỗi 2 lần đầu, thành công ở lần 3
    MockXMLHttpRequest.failCountBeforeSuccess = 2;

    const mockUploadUrlRes: UploadUrlResponse = {
      upload_id: 'mock-retry-id',
      presigned_put_url: 'https://s3.mock/upload/retry.tmp',
      staging_key: 'staging/gym_photo/2026/11/retry.tmp',
      expires_in_seconds: 900,
    };

    const mockConfirmRes: ConfirmUploadResponse = {
      media_id: 'media-retry-done',
      content_hash: 'sha256-retry',
      urls: { thumb: '', medium: '', large: '' },
      width: 800,
      height: 600,
      blurhash: 'L6PZfS',
      total_size_bytes: 30000,
      is_deduplicated: false,
    };

    global.fetch = jest.fn((url: RequestInfo | URL) => {
      const urlStr = url.toString();
      if (urlStr.includes('/mock-compressed.jpg')) {
        return Promise.resolve({
          blob: () => Promise.resolve({ size: 1024 * 20, type: 'image/jpeg' }),
        } as unknown as Response);
      }
      if (urlStr.includes('/api/v1/media/upload-url')) {
        return Promise.resolve({
          ok: true,
          status: 200,
          json: () => Promise.resolve(mockUploadUrlRes),
        } as unknown as Response);
      }
      if (urlStr.includes('/api/v1/media/confirm')) {
        return Promise.resolve({
          ok: true,
          status: 200,
          json: () => Promise.resolve(mockConfirmRes),
        } as unknown as Response);
      }
      return Promise.reject(new Error(`Unhandled URL: ${urlStr}`));
    });

    const result = await uploadImage('file:///original.jpg', 'gym_photo', {
      apiBaseUrl: 'http://localhost:5000',
      maxRetries: 3,
      initialRetryDelayMs: 20, // Delay ngắn cho test
    });

    expect(result.media_id).toBe('media-retry-done');
    expect(MockXMLHttpRequest.currentAttempts).toBe(3); // 2 lần lỗi + 1 lần thành công
  });

  it('hủy upload lập tức khi kích hoạt AbortController', async () => {
    const controller = new AbortController();

    global.fetch = jest.fn((url: RequestInfo | URL) => {
      const urlStr = url.toString();
      if (urlStr.includes('/mock-compressed.jpg')) {
        return Promise.resolve({
          blob: () => Promise.resolve({ size: 1024 * 20, type: 'image/jpeg' }),
        } as unknown as Response);
      }
      if (urlStr.includes('/api/v1/media/upload-url')) {
        // Hủy ngay sau khi nhận upload URL
        controller.abort();
        return Promise.resolve({
          ok: true,
          status: 200,
          json: () => Promise.resolve({
            upload_id: 'u-1',
            presigned_put_url: 'https://s3.mock/abort',
            staging_key: 'staging/avatar/mock.tmp',
            expires_in_seconds: 900,
          }),
        } as unknown as Response);
      }
      return Promise.reject(new Error('Should not reach here'));
    });

    await expect(
      uploadImage('file:///original.jpg', 'avatar', {
        apiBaseUrl: 'http://localhost:5000',
        signal: controller.signal,
      })
    ).rejects.toThrow('Upload cancelled by user');
  });
});
