import {
  compressImageOnDevice,
  selectImageVariant,
  setCustomManipulator,
  uploadMultipartFile,
} from '../src/shared/file';

describe('File Module (Mobile)', () => {
  describe('selectImageVariant', () => {
    const urls = {
      thumb: 'https://cdn.gymbase.vn/thumb.webp',
      medium: 'https://cdn.gymbase.vn/medium.webp',
      large: 'https://cdn.gymbase.vn/large.webp',
    };

    it('chọn thumb khi chiều rộng hiển thị nhỏ (<= 120dp)', () => {
      expect(selectImageVariant(urls, 80)).toBe(urls.thumb);
    });

    it('chọn medium khi chiều rộng hiển thị vừa (<= 480dp)', () => {
      expect(selectImageVariant(urls, 360)).toBe(urls.medium);
    });

    it('chọn large khi chiều rộng hiển thị lớn (> 480dp)', () => {
      expect(selectImageVariant(urls, 720)).toBe(urls.large);
    });
  });

  describe('compressImageOnDevice', () => {
    it('thực hiện nén ảnh với adapter', async () => {
      const mockManipulate = jest.fn().mockResolvedValue({
        uri: 'file:///compressed.jpg',
        width: 1080,
        height: 1080,
      });
      const res = await compressImageOnDevice('file:///original.jpg', {
        manipulateAsync: mockManipulate,
      });

      expect(res.uri).toBe('file:///compressed.jpg');
      expect(mockManipulate).toHaveBeenCalledTimes(1);
    });
  });

  describe('uploadMultipartFile', () => {
    beforeEach(() => {
      setCustomManipulator({
        manipulateAsync: jest.fn().mockResolvedValue({
          uri: 'file:///compressed.jpg',
          width: 1080,
          height: 1080,
        }),
      });
    });

    it('gửi multipart/form-data tới API và nhận kết quả FileUploadResult', async () => {
      globalThis.fetch = jest.fn((url: RequestInfo | URL, init?: RequestInit) => {
        const urlStr = url.toString();
        if (urlStr.includes('/api/v1/mob/files')) {
          expect(init?.method).toBe('POST');
          return Promise.resolve({
            ok: true,
            status: 200,
            json: () =>
              Promise.resolve({
                file_id: 'file-guid-12345',
                original_name: 'avatar.jpg',
                content_type: 'image/webp',
                size: 8192,
                purpose: 'avatar',
                provider: 'LOCAL',
                blurhash: 'L6PZfSi_.AyE_3t7t7R**0o#DgR4',
                content_url: '/api/files/file-guid-12345/content',
              }),
          } as unknown as Response);
        }
        return Promise.reject(new Error(`Unhandled URL: ${urlStr}`));
      });

      const res = await uploadMultipartFile('file:///original.jpg', 'avatar', {
        apiBaseUrl: 'http://localhost:5020',
        authToken: 'test-token',
      });

      expect(res.file_id).toBe('file-guid-12345');
      expect(res.provider).toBe('LOCAL');
      expect(res.content_type).toBe('image/webp');
      expect(res.content_url).toBe('/api/files/file-guid-12345/content');
    });

    it('ném lỗi khi API trả về status 400', async () => {
      globalThis.fetch = jest.fn(() =>
        Promise.resolve({
          ok: false,
          status: 400,
          text: () => Promise.resolve('{"error":"Tệp tin tải lên rỗng."}'),
        } as unknown as Response),
      );

      await expect(uploadMultipartFile('file:///empty.jpg', 'avatar')).rejects.toThrow(
        'Upload file multipart thất bại (400)',
      );
    });
  });
});
