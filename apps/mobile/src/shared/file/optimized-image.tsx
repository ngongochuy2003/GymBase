import React, { useMemo } from 'react';
import { StyleProp } from 'react-native';
import { Image, ImageProps, ImageStyle } from 'expo-image';
import { ImageVariantUrls } from './file.types';
import { selectImageVariant } from './variant-selector';

export interface OptimizedImageProps extends Omit<ImageProps, 'source'> {
  urls: ImageVariantUrls;
  blurhash?: string;
  displayWidth?: number;
  recyclingKey?: string;
  style?: StyleProp<ImageStyle>;
}

/**
 * Component hiển thị ảnh tối ưu từ 3 biến thể URL (thumb, medium, large)
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
  const selectedUri = useMemo(() => selectImageVariant(urls, displayWidth), [urls, displayWidth]);

  const placeholderSource = useMemo(() => (blurhash ? { blurhash } : undefined), [blurhash]);

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

export interface OptimizedFileImageProps extends Omit<ImageProps, 'source'> {
  fileId: string;
  apiBaseUrl?: string;
  blurhash?: string;
  recyclingKey?: string;
  style?: StyleProp<ImageStyle>;
}

/**
 * Component hiển thị tệp tin từ Backend GymBase qua FileId (/api/files/{id}/content)
 */
export const OptimizedFileImage: React.FC<OptimizedFileImageProps> = ({
  fileId,
  apiBaseUrl = '',
  blurhash,
  recyclingKey,
  style,
  contentFit = 'cover',
  transition = 200,
  cachePolicy = 'memory-disk',
  ...rest
}) => {
  const contentUrl = useMemo(() => {
    const cleanBase = apiBaseUrl.replace(/\/+$/, '');
    return cleanBase ? `${cleanBase}/api/files/${fileId}/content` : `/api/files/${fileId}/content`;
  }, [apiBaseUrl, fileId]);

  const placeholderSource = useMemo(() => (blurhash ? { blurhash } : undefined), [blurhash]);

  return (
    <Image
      source={{ uri: contentUrl }}
      placeholder={placeholderSource}
      recyclingKey={recyclingKey || contentUrl}
      contentFit={contentFit}
      transition={transition}
      cachePolicy={cachePolicy}
      style={style}
      {...rest}
    />
  );
};
