import { render, screen } from '@testing-library/react-native';
import { Image as ExpoImage } from 'expo-image';
import { mapBaiVietDtoToFeedPost, type BaiVietDto } from '@/features/feed';
import { ghiNhoThongTinAnh, layThongTinAnh, xoaCacheAnh } from '@/shared/file';
import { RemoteImage } from '@/shared/ui-kit/remote-image';

jest.mock('expo-image', () => {
  // eslint-disable-next-line @typescript-eslint/no-require-imports
  const { View } = require('react-native');
  return {
    Image: Object.assign(View, { clearMemoryCache: jest.fn(async () => true), clearDiskCache: jest.fn(async () => true) }),
  };
});

const URL_KY = 'https://minio.test/get/post/a.jpg?sig=1';
const BLURHASH = 'LEHV6nWB2yk8pyo0adR*.7kCMdnj';

describe('thông tin ảnh (file_id, blurhash)', () => {
  afterEach(() => xoaCacheAnh());

  it('nhớ và trả lại thông tin theo URL', () => {
    ghiNhoThongTinAnh(URL_KY, { fileId: 'f1', blurhash: BLURHASH });

    expect(layThongTinAnh(URL_KY)).toEqual({ fileId: 'f1', blurhash: BLURHASH });
    expect(layThongTinAnh('https://khac.test/x.jpg')).toBeUndefined();
  });

  it('chỉ giữ tối đa 1000 URL, bỏ mục cũ nhất', () => {
    for (let i = 0; i < 1001; i++) ghiNhoThongTinAnh(`https://minio.test/${i}.jpg`, { fileId: String(i) });

    expect(layThongTinAnh('https://minio.test/0.jpg')).toBeUndefined();
    expect(layThongTinAnh('https://minio.test/1.jpg')).toEqual({ fileId: '1' });
    expect(layThongTinAnh('https://minio.test/1000.jpg')).toEqual({ fileId: '1000' });
  });

  it('xoaCacheAnh xóa thông tin đã nhớ và cache ảnh trong bộ nhớ lẫn trên đĩa', async () => {
    ghiNhoThongTinAnh(URL_KY, { fileId: 'f1' });

    await xoaCacheAnh();

    expect(layThongTinAnh(URL_KY)).toBeUndefined();
    expect(ExpoImage.clearMemoryCache).toHaveBeenCalled();
    expect(ExpoImage.clearDiskCache).toHaveBeenCalled();
  });

  it('xoaCacheAnh không ném lỗi khi xóa cache thất bại', async () => {
    (ExpoImage.clearDiskCache as jest.Mock).mockRejectedValueOnce(new Error('native'));

    await expect(xoaCacheAnh()).resolves.toBeUndefined();
  });
});

describe('mapBaiVietDtoToFeedPost nhớ thông tin ảnh', () => {
  afterEach(() => xoaCacheAnh());

  const dto = (over: Partial<BaiVietDto>): BaiVietDto => ({
    id: 'p1',
    tac_gia_id: 'u1',
    tac_gia: { id: 'u1', ten_hien_thi: 'Minh', anh_dai_dien_url: null },
    noi_dung: 'x',
    loai_bai_viet: 'hinh_anh',
    che_do_hien_thi: 'cong_khai',
    trang_thai: 'da_dang',
    chu_de: null,
    hinh_anh_urls: [],
    so_luot_thich: 0,
    so_binh_luan: 0,
    da_thich: false,
    ngay_dang: null,
    ngay_tao: '2026-11-11T10:00:00Z',
    ngay_chinh_sua: null,
    ...over,
  });

  it('ghi nhớ file_id và blurhash theo đúng vị trí, bỏ qua ảnh ngoài không có thông tin', () => {
    mapBaiVietDtoToFeedPost(
      dto({
        hinh_anh_urls: ['https://cdn.test/ngoai.jpg', URL_KY],
        hinh_anh_meta: [
          { file_id: null, blurhash: null },
          { file_id: 'f1', blurhash: BLURHASH },
        ],
      }),
    );

    expect(layThongTinAnh('https://cdn.test/ngoai.jpg')).toBeUndefined();
    expect(layThongTinAnh(URL_KY)).toEqual({ fileId: 'f1', blurhash: BLURHASH });
  });

  it('API cũ không trả hinh_anh_meta thì không lỗi', () => {
    expect(() => mapBaiVietDtoToFeedPost(dto({ hinh_anh_urls: [URL_KY] }))).not.toThrow();
    expect(layThongTinAnh(URL_KY)).toBeUndefined();
  });
});

describe('RemoteImage với thông tin ảnh', () => {
  afterEach(() => xoaCacheAnh());

  it('có blurhash và file_id: dùng expo-image, cache theo file_id, placeholder là blurhash', async () => {
    await render(<RemoteImage uri={URL_KY} accessibilityLabel="Ảnh có hash" media={{ fileId: 'f1', blurhash: BLURHASH }} />);

    const image = screen.getByLabelText('Ảnh có hash');
    expect(image.props.source).toEqual({ uri: URL_KY, cacheKey: 'f1' });
    expect(image.props.placeholder).toEqual({ blurhash: BLURHASH });
    expect(image.props.contentFit).toBe('cover');
  });

  it('tự tra thông tin theo URL khi không truyền media', async () => {
    ghiNhoThongTinAnh(URL_KY, { fileId: 'f2', blurhash: BLURHASH });

    await render(<RemoteImage uri={URL_KY} accessibilityLabel="Ảnh tự tra" resizeMode="contain" />);

    const image = screen.getByLabelText('Ảnh tự tra');
    expect(image.props.source.cacheKey).toBe('f2');
    expect(image.props.contentFit).toBe('contain');
  });

  it('không có thông tin: vẫn dùng Image của React Native như cũ', async () => {
    await render(<RemoteImage uri="https://cdn.test/thuong.jpg" accessibilityLabel="Ảnh thường" />);

    const image = screen.getByLabelText('Ảnh thường');
    expect(image.props.source).toEqual({ uri: 'https://cdn.test/thuong.jpg' });
    expect(image.props.placeholder).toBeUndefined();
  });
});
