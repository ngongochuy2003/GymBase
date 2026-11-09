import { selectImageVariant } from '../src/utils/variant-selector';
import { ImageVariantUrls } from '../src/types/media.types';

describe('selectImageVariant', () => {
  const mockUrls: ImageVariantUrls = {
    thumb: 'https://cdn.gymbase.vn/media/avatar/hash/thumb.webp',
    medium: 'https://cdn.gymbase.vn/media/avatar/hash/medium.webp',
    large: 'https://cdn.gymbase.vn/media/avatar/hash/large.webp',
  };

  it('chọn thumb khi chiều rộng hiển thị <= 200px (Avatar, list icon)', () => {
    expect(selectImageVariant(mockUrls, 80)).toBe(mockUrls.thumb);
    expect(selectImageVariant(mockUrls, 200)).toBe(mockUrls.thumb);
  });

  it('chọn medium khi chiều rộng hiển thị từ 201px đến 720px (Card bài tập, Post preview)', () => {
    expect(selectImageVariant(mockUrls, 360)).toBe(mockUrls.medium);
    expect(selectImageVariant(mockUrls, 720)).toBe(mockUrls.medium);
  });

  it('chọn large khi chiều rộng hiển thị > 720px (Detail view, full screen zoom)', () => {
    expect(selectImageVariant(mockUrls, 721)).toBe(mockUrls.large);
    expect(selectImageVariant(mockUrls, 1080)).toBe(mockUrls.large);
  });

  it('fallback an toàn khi một số URL biến thể bị khuyết', () => {
    const partialUrls: ImageVariantUrls = {
      thumb: '',
      medium: 'https://cdn.gymbase.vn/medium.webp',
      large: '',
    };
    expect(selectImageVariant(partialUrls, 100)).toBe('https://cdn.gymbase.vn/medium.webp');
  });
});
