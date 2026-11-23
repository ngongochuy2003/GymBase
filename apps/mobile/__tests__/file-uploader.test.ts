import axios, { AxiosError } from 'axios';
import { getMobClient } from '@/core/api/mobile-client';
import {
  compressImageOnDevice,
  setCustomManipulator,
  UploadError,
  uploadMultipartFile,
} from '../src/shared/file';

jest.mock('@/core/api/mobile-client', () => ({ getMobClient: jest.fn() }));

const FILE_ID = 'file-guid-12345';
const FAR_FUTURE = new Date(Date.now() + 15 * 60_000).toISOString();

const FORM = {
  file_id: FILE_ID,
  upload: {
    url: 'http://minio.test/gymbase-files',
    method: 'POST' as const,
    file_field: 'file',
    het_han: FAR_FUTURE,
    fields: { key: `tmp/${FILE_ID}`, 'Content-Type': 'image/jpeg', policy: 'p', 'x-amz-signature': 's' },
  },
};

const CONFIRMED = (over: Record<string, unknown> = {}) => ({
  file_id: FILE_ID,
  loai_mime: 'image/jpeg',
  kich_thuoc_byte: 8192,
  muc_dich: 'avatar',
  trang_thai: 'da_tai_len',
  ...over,
});

class FakeFormData {
  parts: [string, unknown][] = [];
  append(key: string, value: unknown) {
    this.parts.push([key, value]);
  }
}

/** Lỗi axios có phản hồi của server (API hoặc storage). */
const httpError = (status: number, data: unknown = {}) =>
  new AxiosError('Request failed', 'ERR_BAD_REQUEST', undefined, undefined, {
    status,
    data,
    statusText: '',
    headers: {},
    config: {} as never,
  });

describe('uploadMultipartFile (xin form, gửi lên storage, xác nhận)', () => {
  const apiPost = jest.fn();
  let storagePost: jest.SpyInstance;
  const manipulate = jest.fn();

  const storageCalls = () => storagePost.mock.calls as [string, FakeFormData, Record<string, any>][];

  beforeEach(() => {
    jest.resetAllMocks();
    globalThis.FormData = FakeFormData as unknown as typeof FormData;
    (getMobClient as jest.Mock).mockReturnValue({ post: apiPost });
    apiPost.mockImplementation(async (url: string) =>
      url.endsWith('/upload-url') ? { data: FORM } : { data: CONFIRMED() },
    );
    storagePost = jest.spyOn(axios, 'post').mockResolvedValue({ status: 204, data: '' });
    manipulate.mockResolvedValue({ uri: 'file:///compressed.jpg', width: 1080, height: 1080 });
    setCustomManipulator({ manipulateAsync: manipulate });
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  it('xin form, gửi tệp lên storage rồi xác nhận, trả file_id và dung lượng thật do backend đo', async () => {
    const res = await uploadMultipartFile('file:///original.jpg', 'avatar');

    expect(apiPost.mock.calls.map((c) => c[0])).toEqual(['/files/upload-url', `/files/${FILE_ID}/confirm`]);
    expect(apiPost.mock.calls[0]![1]).toMatchObject({
      muc_dich: 'avatar',
      loai_mime: 'image/jpeg',
      ten_tep: 'compressed.jpg',
    });
    expect(storagePost).toHaveBeenCalledTimes(1);
    expect(res).toEqual({
      file_id: FILE_ID,
      original_name: 'compressed.jpg',
      content_type: 'image/jpeg',
      size: 8192,
      purpose: 'avatar',
    });
  });

  it('gửi các field của policy trước, trường file đặt cuối, và không gắn Authorization lên storage', async () => {
    await uploadMultipartFile('file:///original.jpg', 'avatar');

    const [url, form, config] = storageCalls()[0]!;
    expect(url).toBe(FORM.upload.url);
    expect(form.parts.map(([name]) => name)).toEqual([...Object.keys(FORM.upload.fields), 'file']);
    expect(form.parts.at(-1)![1]).toEqual({ uri: 'file:///compressed.jpg', type: 'image/jpeg', name: 'compressed.jpg' });
    expect(JSON.stringify(config.headers ?? {})).not.toMatch(/authorization/i);
  });

  it('nén ảnh trước khi gửi', async () => {
    await uploadMultipartFile('file:///original.jpg', 'progress');

    expect(manipulate).toHaveBeenCalledWith('file:///original.jpg', [{ resize: { width: 2048 } }], {
      compress: 0.9,
      format: 'jpeg',
    });
  });

  it('báo tiến độ theo byte đã gửi và 100 khi xác nhận xong', async () => {
    storagePost.mockImplementation(async (_url, _form, config) => {
      config.onUploadProgress({ loaded: 50, total: 200 });
      return { status: 204 };
    });
    const onProgress = jest.fn();

    await uploadMultipartFile('file:///original.jpg', 'avatar', { onProgress });

    expect(onProgress.mock.calls.map((c) => c[0])).toEqual([25, 100]);
  });

  it('giữ nguyên PDF: không nén, dùng tên người dùng chọn và dung lượng đã biết', async () => {
    apiPost.mockImplementation(async (url: string) =>
      url.endsWith('/upload-url') ? { data: FORM } : { data: CONFIRMED({ loai_mime: 'application/pdf' }) },
    );

    await uploadMultipartFile('file:///cache/cc.pdf', 'chung_chi_pt', {
      contentType: 'application/pdf',
      fileName: 'Chung chi PT.pdf',
      sizeBytes: 4096,
    });

    expect(manipulate).not.toHaveBeenCalled();
    expect(apiPost.mock.calls[0]![1]).toMatchObject({
      muc_dich: 'chung_chi_pt',
      loai_mime: 'application/pdf',
      ten_tep: 'Chung chi PT.pdf',
      kich_thuoc_byte: 4096,
    });
    expect(storageCalls()[0]![1].parts.at(-1)![1]).toEqual({
      uri: 'file:///cache/cc.pdf',
      type: 'application/pdf',
      name: 'Chung chi PT.pdf',
    });
  });

  it('giữ nguyên video và khai đúng MIME', async () => {
    await uploadMultipartFile('file:///clips/buoi-tap.mov', 'post');

    expect(manipulate).not.toHaveBeenCalled();
    expect(apiPost.mock.calls[0]![1]).toMatchObject({ muc_dich: 'post', loai_mime: 'video/quicktime' });
  });

  it('khai dung lượng tối thiểu 1 byte khi chưa biết, để backend không từ chối vì 0', async () => {
    await uploadMultipartFile('file:///cache/cc.pdf', 'chung_chi_pt', { contentType: 'application/pdf' });

    expect(apiPost.mock.calls[0]![1]).toMatchObject({ kich_thuoc_byte: 1 });
  });

  describe('lỗi và thử lại', () => {
    it('thử lại khi mất mạng rồi thành công', async () => {
      storagePost.mockRejectedValueOnce(new Error('Network Error')).mockResolvedValue({ status: 204 });

      const res = await uploadMultipartFile('file:///original.jpg', 'avatar', { initialRetryDelayMs: 0 });

      expect(storagePost).toHaveBeenCalledTimes(2);
      expect(res.file_id).toBe(FILE_ID);
      expect(apiPost.mock.calls.filter((c) => c[0] === '/files/upload-url')).toHaveLength(1);
    });

    it('thử lại khi storage trả 5xx', async () => {
      storagePost.mockRejectedValueOnce(httpError(503)).mockResolvedValue({ status: 204 });

      await uploadMultipartFile('file:///original.jpg', 'avatar', { initialRetryDelayMs: 0 });

      expect(storagePost).toHaveBeenCalledTimes(2);
    });

    it('hết số lần thử thì báo UploadError status 0 (không có mạng)', async () => {
      storagePost.mockRejectedValue(new Error('Network Error'));

      const error = await uploadMultipartFile('file:///original.jpg', 'avatar', {
        maxRetries: 1,
        initialRetryDelayMs: 0,
      }).catch((e: unknown) => e);

      expect(storagePost).toHaveBeenCalledTimes(2);
      expect(error).toBeInstanceOf(UploadError);
      expect((error as UploadError).status).toBe(0);
    });

    it('storage từ chối (403) thì không thử lại và không gọi xác nhận', async () => {
      storagePost.mockRejectedValue(httpError(403));

      const error = await uploadMultipartFile('file:///original.jpg', 'avatar').catch((e: unknown) => e);

      expect(storagePost).toHaveBeenCalledTimes(1);
      expect(error).toBeInstanceOf(UploadError);
      expect((error as UploadError).status).toBe(403);
      expect((error as UploadError).code).toBe('STORAGE_REJECTED');
      expect(apiPost.mock.calls.map((c) => c[0])).toEqual(['/files/upload-url']);
    });

    it('form đã hết hạn thì xin form mới trước khi gửi', async () => {
      const expired = { ...FORM, upload: { ...FORM.upload, het_han: new Date(Date.now() - 1000).toISOString() } };
      apiPost.mockImplementation(async (url: string) =>
        url.endsWith('/upload-url') ? { data: apiPost.mock.calls.length === 1 ? expired : FORM } : { data: CONFIRMED() },
      );

      await uploadMultipartFile('file:///original.jpg', 'avatar');

      expect(apiPost.mock.calls.filter((c) => c[0] === '/files/upload-url')).toHaveLength(2);
      expect(storagePost).toHaveBeenCalledTimes(1);
    });

    it('lỗi từ API xin upload giữ mã lỗi và câu của server (hết hạn mức)', async () => {
      apiPost.mockRejectedValue(
        httpError(422, { detail: 'Bạn đã đạt số lượng tệp tối đa trong ngày.', code: 'FILE_QUOTA_EXCEEDED' }),
      );

      const error = await uploadMultipartFile('file:///original.jpg', 'avatar').catch((e: unknown) => e);

      expect(error).toBeInstanceOf(UploadError);
      expect(error).toMatchObject({
        status: 422,
        code: 'FILE_QUOTA_EXCEEDED',
        message: 'Bạn đã đạt số lượng tệp tối đa trong ngày.',
      });
      expect(storagePost).not.toHaveBeenCalled();
    });

    it('lỗi từ API xác nhận (nội dung không khớp) được báo về', async () => {
      apiPost.mockImplementation(async (url: string) => {
        if (url.endsWith('/upload-url')) return { data: FORM };
        throw httpError(400, { detail: 'Nội dung tệp không khớp.', code: 'FILE_CONTENT_MISMATCH' });
      });

      const error = await uploadMultipartFile('file:///original.jpg', 'avatar').catch((e: unknown) => e);

      expect(error).toMatchObject({ status: 400, code: 'FILE_CONTENT_MISMATCH' });
    });

    it('đã hủy thì không gọi API nào', async () => {
      const controller = new AbortController();
      controller.abort();

      await expect(
        uploadMultipartFile('file:///original.jpg', 'avatar', { signal: controller.signal }),
      ).rejects.toThrow('Upload cancelled by user');
      expect(apiPost).not.toHaveBeenCalled();
    });
  });
});

describe('compressImageOnDevice', () => {
  it('thực hiện nén ảnh với adapter', async () => {
    const mockManipulate = jest.fn().mockResolvedValue({
      uri: 'file:///compressed.jpg',
      width: 1080,
      height: 1080,
    });

    const res = await compressImageOnDevice('file:///original.jpg', { manipulateAsync: mockManipulate });

    expect(res.uri).toBe('file:///compressed.jpg');
    expect(mockManipulate).toHaveBeenCalledTimes(1);
  });
});
