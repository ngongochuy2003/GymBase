import { Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { router } from 'expo-router';
import { useSession } from '@/core/auth/session';
import { AppText, Button, Screen } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';

/** Hiện đúng một lần, ngay sau khi tài khoản vừa được tạo (cờ `pendingWelcome` của phiên). */
export function RegisterSuccessScreen() {
  const { completeWelcome } = useSession();
  const { colors } = useTheme();
  const go = (href: '/profile-setup' | '/today') => {
    completeWelcome();
    router.replace(href);
  };
  return (
    <Screen>
      <View className="items-center gap-4 pt-10">
        <View className="size-24 items-center justify-center rounded-full bg-secondary">
          <Ionicons name="checkmark" size={56} color={colors.primary} />
        </View>
        <Text className="text-center text-[28px] font-bold leading-9 text-foreground">
          Đăng ký thành công!
        </Text>
        <AppText muted>
          Tài khoản của bạn đã được tạo. Giờ bạn có thể hoàn thiện hồ sơ để cá nhân hóa trải nghiệm.
        </AppText>
      </View>
      <View className="gap-3 pt-6">
        <Button label="Thiết lập hồ sơ ngay" onPress={() => go('/profile-setup')} />
        <Button label="Bỏ qua, vào Trang chủ" secondary onPress={() => go('/today')} />
        <AppText muted>Bạn có thể thiết lập sau trong tab Cá nhân.</AppText>
      </View>
    </Screen>
  );
}
