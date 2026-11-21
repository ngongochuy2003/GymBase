import axios, { AxiosError } from 'axios';
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
    afterEach(() => {
      jest.restoreAllMocks();
    });

    class MockFormData {
      _parts: [string, any][] = [];
      append(key: string, value: any) {
        this._parts.push([key, value]);
      }
    }

    beforeEach(() => {
      // @ts-ignore
      global.FormData = MockFormData;
      setCustomManipulator({
        manipulateAsync: jest.fn().mockResolvedValue({
          uri: 'file:///compressed.jpg',
          width: 1080,
          height: 1080,
        }),
      });
    });

    it('gửi multipart/form-data tới API và nhận kết quả FileUploadResult', async () => {
      const post = jest.spyOn(axios, 'post').mockResolvedValue({
        status: 200,
        data: {
          file_id: 'file-guid-12345',
          original_name: 'avatar.jpg',
          content_type: 'image/webp',
          size: 8192,
          purpose: 'avatar',
          provider: 'LOCAL',
          blurhash: 'L6PZfSi_.AyE_3t7t7R**0o#DgR4',
          content_url: '/api/files/file-guid-12345/content',
        },
      });

      const res = await uploadMultipartFile('file:///original.jpg', 'avatar', {
        apiBaseUrl: 'http://localhost:5020',
        authToken: 'test-token',
      });

      expect(post.mock.calls[0]?.[0]).toBe('http://localhost:5020/api/v1/mob/files');
      expect(res.file_id).toBe('file-guid-12345');
      expect(res.provider).toBe('LOCAL');
      expect(res.content_type).toBe('image/webp');
      expect(res.content_url).toBe('/api/files/file-guid-12345/content');
    });

    it('ném lỗi khi API trả về status 400', async () => {
      jest.spyOn(axios, 'post').mockRejectedValue(
        new AxiosError('Request failed', 'ERR_BAD_REQUEST', undefined, undefined, {
          status: 400,
          data: { error: 'Tệp tin tải lên rỗng.' },
          statusText: '',
          headers: {},
          config: {} as never,
        }),
      );

      await expect(uploadMultipartFile('file:///empty.jpg', 'avatar')).rejects.toThrow(
        'Upload file multipart thất bại (400)',
      );
    });
  });
});
