import { act, renderHook } from '@testing-library/react-native';
import { setCustomVideoCompressor } from '../src/shared/file';
import { useComposerState } from '../src/features/composer/hooks/use-composer-state';

const mockLaunchLibrary = jest.fn();
jest.mock('expo-image-picker', () => ({
  requestMediaLibraryPermissionsAsync: jest.fn(async () => ({ granted: true, status: 'granted' })),
  launchImageLibraryAsync: (...args: unknown[]) => mockLaunchLibrary(...args),
}));

const MB = 1024 * 1024;
const video = (name: string, fileSize?: number) => ({
  uri: `file:///clips/${name}.mp4`,
  type: 'video' as const,
  mimeType: 'video/mp4',
  fileSize,
});
const image = { uri: 'file:///photos/a.jpg', type: 'image' as const, mimeType: 'image/jpeg', fileSize: 30 * MB };

describe('Composer: chọn media', () => {
  const onMediaRejected = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    setCustomVideoCompressor(null);
  });

  async function chon(...assets: unknown[]) {
    mockLaunchLibrary.mockResolvedValue({ canceled: false, assets });
    const { result } = await renderHook(() => useComposerState({ initialVisibilityId: 'public', onMediaRejected }));
    await act(async () => {
      await result.current.addMedia();
    });
    return result;
  }

  it('loại video quá 100MB ngay lúc chọn và báo lý do, giữ các tệp hợp lệ', async () => {
    const result = await chon(video('qua-lon', 120 * MB), video('vua', 50 * MB));

    expect(result.current.media.map((m) => m.uri)).toEqual(['file:///clips/vua.mp4']);
    expect(onMediaRejected).toHaveBeenCalledTimes(1);
    expect(onMediaRejected.mock.calls[0]![0]).toContain('100MB');
  });

  it('lưu dung lượng video để uploader kiểm lại', async () => {
    const result = await chon(video('vua', 50 * MB));

    expect(result.current.media[0]).toMatchObject({ type: 'video', sizeBytes: 50 * MB });
    expect(onMediaRejected).not.toHaveBeenCalled();
  });

  it('không loại ảnh lớn (ảnh được nén lại trước khi gửi)', async () => {
    const result = await chon(image);

    expect(result.current.media).toHaveLength(1);
    expect(onMediaRejected).not.toHaveBeenCalled();
  });

  it('máy nén được video: video 300MB vẫn được chọn', async () => {
    setCustomVideoCompressor({ compress: jest.fn(), getFileSize: jest.fn() });

    const result = await chon(video('rat-lon', 300 * MB));

    expect(result.current.media).toHaveLength(1);
    expect(onMediaRejected).not.toHaveBeenCalled();
  });

  it('không có video nào hợp lệ thì không thêm gì', async () => {
    const result = await chon(video('qua-lon', 120 * MB));

    expect(result.current.media).toHaveLength(0);
  });
});
