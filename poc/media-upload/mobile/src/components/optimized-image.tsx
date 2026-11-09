import React, { useMemo } from 'react';
import { StyleProp } from 'react-native';
import { Image, ImageProps, ImageStyle } from 'expo-image';
import { ImageVariantUrls } from '../types/media.types';
import { selectImageVariant } from '../utils/variant-selector';

export interface OptimizedImageProps extends Omit<ImageProps, 'source'> {
  /**
   * Bộ 3 URL biến thể { thumb, medium, large } từ Backend trả về
   */
  urls: ImageVariantUrls;

  /**
   * Chuỗi Blurhash đại diện cho ảnh, dùng làm placeholder mượt mà khi đang tải
   */
  blurhash?: string;

  /**
   * Chiều rộng hiển thị dự kiến (dp) để tự động chọn biến thể tối ưu dung lượng
   */
  displayWidth?: number;

  /**
   * Khóa tái sử dụng cho FlatList / FlashList giúp cuộn danh sách mượt mà không bị giật
   */
  recyclingKey?: string;

  /**
   * Kiểu dáng style tùy biến
   */
  style?: StyleProp<ImageStyle>;
}

/**
 * Component hiển thị ảnh tối ưu hóa cao cho GymBase Mobile.
 * - Tự động chọn biến thể ảnh (thumb, medium, large) phù hợp với kích thước hiển thị.
 * - Hiển thị placeholder Blurhash tức thì, chuyển cảnh mượt mà khi ảnh thật tải xong.
 * - Tận dụng bộ nhớ đệm kép của expo-image: Memory Cache + Disk Cache.
 * - Hỗ trợ recyclingKey cho danh sách dài (avatar PT, danh sách bài tập, nhật ký ảnh).
 */
export const OptimizedImage: React.FC<OptimizedImageProps> = ({
  urls,
  blurhash,
  displayWidth = 360,
  recyclingKey,
  style,
  contentFit = 'cover',
  transition = 200,
  cachePolicy = 'memory-disk',
  ...rest
}) => {
  const selectedUri = useMemo(
    () => selectImageVariant(urls, displayWidth),
    [urls, displayWidth]
  );

  const placeholderSource = useMemo(
    () => (blurhash ? { blurhash } : undefined),
    [blurhash]
  );

  return (
    <Image
      source={{ uri: selectedUri }}
      placeholder={placeholderSource}
      recyclingKey={recyclingKey || selectedUri}
      contentFit={contentFit}
      transition={transition}
      cachePolicy={cachePolicy}
      style={style}
      {...rest}
    />
  );
};
