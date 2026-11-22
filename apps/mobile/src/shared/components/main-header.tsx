import { useState } from 'react';
import { Image, Pressable, Text, View } from 'react-native';
import { Icon } from '@/shared/icons';
import { useTheme } from '../theme/theme';

const LOGO_SANG = require('../../../assets/images/logo.png') as number;
const LOGO_TOI = require('../../../assets/images/logo-dark.png') as number;

export interface MainHeaderProps {
  avatarUri?: string;
  /** Tên dùng lấy chữ cái đầu khi chưa có ảnh đại diện. */
  ten?: string;
  badgeThongBao?: boolean;
  onSearch?(): void;
  onNotifications?(): void;
  onAvatar?(): void;
}

/** Thanh đầu của màn tab gốc: logo, tìm kiếm, chuông thông báo, ảnh đại diện. */
// TODO(UI): AppHeader trong ui-kit còn dùng hệ token cũ (chỉ sáng); khi ui-kit chuyển sang NativeWind thì gộp vào đó.
export function MainHeader({
  avatarUri,
  ten = '',
  badgeThongBao = false,
  onSearch,
  onNotifications,
  onAvatar,
}: MainHeaderProps) {
  const { colors, isDark } = useTheme();
  const [loiAnh, setLoiAnh] = useState(false);
  const chuCai = (ten.trim().split(/\s+/).pop()?.[0] ?? '?').toUpperCase();
  return (
    <View className="flex-row items-center justify-between px-4 py-2">
      <Image
        source={isDark ? LOGO_TOI : LOGO_SANG}
        accessibilityLabel="GymBase"
        resizeMode="contain"
        // Ảnh require() có kích thước gốc ghi đè class trên web, nên đặt cỡ bằng style.
        style={{ height: 32, width: 131 }}
      />
      <View className="flex-row items-center gap-5">
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Tìm kiếm"
          hitSlop={8}
          onPress={onSearch}
        >
          <Icon name="search" size={24} color={colors.text} />
        </Pressable>
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Thông báo"
          hitSlop={8}
          onPress={onNotifications}
        >
          <Icon name="bell" size={24} color={colors.text} />
          {badgeThongBao ? (
            <View className="absolute right-0.5 top-0 h-2 w-2 rounded-full border border-background bg-destructive" />
          ) : null}
        </Pressable>
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Ảnh đại diện"
          onPress={onAvatar}
          className="h-10 w-10 overflow-hidden rounded-full"
        >
          {avatarUri && !loiAnh ? (
            <Image
              source={{ uri: avatarUri }}
              resizeMode="cover"
              onError={() => setLoiAnh(true)}
              className="h-10 w-10"
            />
          ) : (
            <View className="h-10 w-10 items-center justify-center bg-secondary">
              <Text className="text-sm font-bold text-primary">{chuCai}</Text>
            </View>
          )}
        </Pressable>
      </View>
    </View>
  );
}
