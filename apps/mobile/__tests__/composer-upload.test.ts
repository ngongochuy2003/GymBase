import { getMobClient } from '@/core/api/mobile-client';
import { createComposerRepository } from '@/features/composer/api/composer-api';
import * as uploader from '@/shared/file/file-uploader';

jest.mock('@/core/api/mobile-client', () => ({ getMobClient: jest.fn() }));

const result = (file_id: string) => ({
  file_id,
  original_name: 'a.jpg',
  content_type: 'image/jpeg',
  size: 1,
  purpose: 'post',
});

describe('composer: đăng bài có ảnh/video', () => {
  const post = jest.fn();
  const del = jest.fn();
  const client = { post } as never;

  beforeEach(() => {
    jest.resetAllMocks();
    (getMobClient as jest.Mock).mockReturnValue({ delete: del });
    del.mockResolvedValue({});
    post.mockResolvedValue({ data: { id: 'bai-1' } });
  });

  const payload = {
    content: 'Buổi tập hôm nay',
    visibilityId: 'public',
    topicIds: ['workout'],
    mediaUris: [],
    mediaItems: [
      { id: '1', uri: 'file:///a.jpg', type: 'image' as const },
      { id: '2', uri: 'https://cdn.test/mau.jpg' },
      { id: '3', uri: 'file:///b.mov', type: 'video' as const, mimeType: 'video/quicktime' },
    ],
  };

  it('tải tệp từ thiết bị lên rồi gửi file_ids theo đúng thứ tự, URL http giữ ở hinh_anh_urls', async () => {
    const upload = jest
      .spyOn(uploader, 'uploadMultipartFile')
      .mockResolvedValueOnce(result('f1'))
      .mockResolvedValueOnce(result('f2'));

    await createComposerRepository(client).createPost(payload);

    expect(upload).toHaveBeenCalledTimes(2);
    expect(upload.mock.calls[0]).toEqual(['file:///a.jpg', 'post', { contentType: undefined }]);
    expect(upload.mock.calls[1]).toEqual(['file:///b.mov', 'post', { contentType: 'video/quicktime' }]);
    const body = post.mock.calls[0]![1];
    expect(post.mock.calls[0]![0]).toBe('/baiviet');
    expect(body.file_ids).toEqual(['f1', 'f2']);
    expect(body.hinh_anh_urls).toEqual(['https://cdn.test/mau.jpg']);
    expect(del).not.toHaveBeenCalled();
  });

  it('đăng bài lỗi thì xóa các tệp đã tải lên', async () => {
    jest
      .spyOn(uploader, 'uploadMultipartFile')
      .mockResolvedValueOnce(result('f1'))
      .mockResolvedValueOnce(result('f2'));
    post.mockRejectedValue(new Error('Máy chủ lỗi'));

    await expect(createComposerRepository(client).createPost(payload)).rejects.toThrow('Máy chủ lỗi');

    expect(del.mock.calls.map((c) => c[0])).toEqual(['/files/f1', '/files/f2']);
  });

  it('upload tệp thứ hai lỗi thì xóa tệp thứ nhất và không đăng bài', async () => {
    jest
      .spyOn(uploader, 'uploadMultipartFile')
      .mockResolvedValueOnce(result('f1'))
      .mockRejectedValueOnce(new Error('Mất mạng'));

    await expect(createComposerRepository(client).createPost(payload)).rejects.toThrow('Mất mạng');

    expect(post).not.toHaveBeenCalled();
    expect(del.mock.calls.map((c) => c[0])).toEqual(['/files/f1']);
  });

  it('bài chỉ có URL http không upload gì', async () => {
    const upload = jest.spyOn(uploader, 'uploadMultipartFile');

    await createComposerRepository(client).createPost({
      content: 'x',
      visibilityId: 'public',
      topicIds: [],
      mediaUris: ['https://cdn.test/mau.jpg'],
    });

    expect(upload).not.toHaveBeenCalled();
    expect(post.mock.calls[0]![1]).toMatchObject({ hinh_anh_urls: ['https://cdn.test/mau.jpg'], file_ids: [] });
  });
});
