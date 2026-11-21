import axios, { AxiosError } from 'axios';
import { authRuntime } from '@/core/api/auth-api';
import { UploadError, uploadMultipartFile } from '@/shared/file/file-uploader';

const RESULT = {
  file_id: 'file-1',
  original_name: 'cc.pdf',
  content_type: 'application/pdf',
  size: 10,
  purpose: 'chung_chi_pt',
  provider: 'LOCAL',
  content_url: '/api/v1/mob/files/file-1/content',
};

class FakeFormData {
  parts: [string, unknown][] = [];
  append(key: string, value: unknown) {
    this.parts.push([key, value]);
  }
}

const ok = (data: unknown = RESULT) => ({ status: 200, data });

/** Lỗi axios có phản hồi của server, như khi server trả 401 hoặc 400. */
const httpError = (status: number, data: unknown = {}) =>
  new AxiosError('Request failed', 'ERR_BAD_REQUEST', undefined, undefined, {
    status,
    data,
    statusText: '',
    headers: {},
    config: {} as never,
  });

const PDF_URI = 'file:///cache/DocumentPicker/cc.pdf';

beforeEach(() => {
  globalThis.FormData = FakeFormData as unknown as typeof FormData;
  jest.spyOn(authRuntime.transport, 'getAccessToken').mockResolvedValue('old-access');
});

afterEach(() => {
  jest.restoreAllMocks();
});

describe('uploadMultipartFile: cách gửi', () => {
  it('gửi bằng axios (XMLHttpRequest), không dùng fetch toàn cục của Expo không nhận phần tệp { uri }', async () => {
    const post = jest.spyOn(axios, 'post').mockResolvedValue(ok());
    const fetchSpy = jest.fn();
    globalThis.fetch = fetchSpy;

    await uploadMultipartFile(PDF_URI, 'chung_chi_pt');

    expect(post).toHaveBeenCalledTimes(1);
    expect(fetchSpy).not.toHaveBeenCalled();
    const [url, , config] = post.mock.calls[0] as [
      string,
      unknown,
      { headers: Record<string, string> },
    ];
    expect(url).toMatch(/\/api\/v1\/mob\/files$/);
    expect(config.headers['Content-Type']).toBe('multipart/form-data');
    expect(config.headers.Authorization).toBe('Bearer old-access');
  });

  it('gửi nguyên tệp PDF với tên và kiểu PDF, không nén ảnh', async () => {
    const post = jest.spyOn(axios, 'post').mockResolvedValue(ok());

    await uploadMultipartFile(PDF_URI, 'chung_chi_pt', {
      contentType: 'application/pdf',
      fileName: 'PT_Level1_FI.pdf',
    });

    const sent = post.mock.calls[0]?.[1] as unknown as FakeFormData;
    expect(sent.parts).toEqual([
      ['purpose', 'chung_chi_pt'],
      ['file', { uri: PDF_URI, type: 'application/pdf', name: 'PT_Level1_FI.pdf' }],
    ]);
  });

  it('tệp có uri content:// vẫn gửi dạng { uri, type, name } chứ không đọc thành Blob', async () => {
    const uri = 'content://com.android.providers.downloads.documents/document/42';
    const post = jest.spyOn(axios, 'post').mockResolvedValue(ok());

    await uploadMultipartFile(uri, 'chung_chi_pt', {
      contentType: 'application/pdf',
      fileName: 'PT_Level1_FI.pdf',
    });

    const sent = post.mock.calls[0]?.[1] as unknown as FakeFormData;
    expect(sent.parts[1]).toEqual([
      'file',
      { uri, type: 'application/pdf', name: 'PT_Level1_FI.pdf' },
    ]);
  });

  it('mất mạng (không có phản hồi) thì ném lỗi gốc, không phải UploadError', async () => {
    jest.spyOn(axios, 'post').mockRejectedValue(new AxiosError('Network Error', 'ERR_NETWORK'));

    const error = await uploadMultipartFile(PDF_URI, 'chung_chi_pt').catch((e: unknown) => e);

    expect(error).toBeInstanceOf(AxiosError);
    expect(error).not.toBeInstanceOf(UploadError);
  });

  it('server trả lỗi khác 401 thì ném UploadError mang mã HTTP', async () => {
    jest.spyOn(axios, 'post').mockRejectedValue(httpError(400, { detail: 'Tệp không hợp lệ.' }));

    const error = await uploadMultipartFile(PDF_URI, 'chung_chi_pt').catch((e: unknown) => e);

    expect(error).toBeInstanceOf(UploadError);
    expect((error as UploadError).status).toBe(400);
    expect((error as UploadError).message).toContain('Upload file multipart thất bại (400)');
  });
});

describe('uploadMultipartFile: phiên đăng nhập', () => {
  it('server trả 401 thì làm mới access token một lần rồi gửi lại với token mới', async () => {
    const refresh = jest.spyOn(authRuntime.transport, 'refresh').mockResolvedValue('new-access');
    const post = jest
      .spyOn(axios, 'post')
      .mockRejectedValueOnce(httpError(401))
      .mockResolvedValueOnce(ok());

    const result = await uploadMultipartFile(PDF_URI, 'chung_chi_pt');

    expect(result.file_id).toBe('file-1');
    expect(refresh).toHaveBeenCalledTimes(1);
    expect(post).toHaveBeenCalledTimes(2);
    const header = (call: number) =>
      (post.mock.calls[call]?.[2] as { headers: Record<string, string> }).headers.Authorization;
    expect(header(0)).toBe('Bearer old-access');
    expect(header(1)).toBe('Bearer new-access');
  });

  it('làm mới không được (phiên đã bị thu hồi) thì đăng xuất và ném lỗi 401', async () => {
    jest.spyOn(authRuntime.transport, 'refresh').mockResolvedValue(null);
    const signedOut = jest.spyOn(authRuntime.transport, 'onUnauthorized').mockResolvedValue();
    jest.spyOn(axios, 'post').mockRejectedValue(httpError(401));

    const error = await uploadMultipartFile(PDF_URI, 'chung_chi_pt').catch((e: unknown) => e);

    expect(error).toBeInstanceOf(UploadError);
    expect((error as UploadError).status).toBe(401);
    expect(signedOut).toHaveBeenCalledTimes(1);
  });

  it('người gọi tự truyền authToken thì không làm mới khi gặp 401', async () => {
    const refresh = jest.spyOn(authRuntime.transport, 'refresh');
    jest.spyOn(axios, 'post').mockRejectedValue(httpError(401));

    await expect(
      uploadMultipartFile(PDF_URI, 'chung_chi_pt', { authToken: 'tu-truyen' }),
    ).rejects.toMatchObject({ status: 401 });

    expect(refresh).not.toHaveBeenCalled();
  });
});
