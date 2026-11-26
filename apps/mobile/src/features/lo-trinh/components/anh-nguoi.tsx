import { useState } from 'react';
import { Image, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';

function chuCaiDau(ten: string): string {
  const tu = ten.trim().split(/\s+/);
  return (tu[tu.length - 1]?.[0] ?? '?').toUpperCase();
}

/** Ảnh vuông bo góc (ảnh bìa); lỗi tải hoặc không có ảnh thì hiện khối màu nhạt kèm chữ cái đầu. */
export function AnhVuong({
  uri,
  ten,
  className,
}: {
  uri?: string;
  ten: string;
  className?: string;
}) {
  const [loi, setLoi] = useState(false);
  if (!uri || loi) {
    return (
      <View className={cn('items-center justify-center bg-secondary', className)}>
        <Text className="text-2xl font-bold text-primary">{chuCaiDau(ten)}</Text>
      </View>
    );
  }
  return (
    <Image
      source={{ uri }}
      accessibilityLabel={ten}
      resizeMode="cover"
      onError={() => setLoi(true)}
      className={cn('bg-secondary', className)}
    />
  );
}

/** Ảnh đại diện tròn. */
export function AnhDaiDien({
  uri,
  ten,
  className,
}: {
  uri?: string;
  ten: string;
  className?: string;
}) {
  const [loi, setLoi] = useState(false);
  if (!uri || loi) {
    return (
      <View className={cn('items-center justify-center rounded-full bg-secondary', className)}>
        <Text className="text-xs font-bold text-primary">{chuCaiDau(ten)}</Text>
      </View>
    );
  }
  return (
    <Image
      source={{ uri }}
      accessibilityLabel={ten}
      resizeMode="cover"
      onError={() => setLoi(true)}
      className={cn('rounded-full bg-secondary', className)}
    />
  );
}
