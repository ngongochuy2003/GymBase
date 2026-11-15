import { Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { router } from 'expo-router';
import { useSession } from '@/core/auth/session';
import { AppText, Button, Screen } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';

/** Hiện sau lần đăng nhập đầu tiên, cho tới khi người dùng bấm một trong hai nút (cờ `is_dang_nhap_lan_dau`). */
export function OnboardingScreen() {
  const { completeOnboarding } = useSession();
  const { colors } = useTheme();
  const go = async (href: '/profile-setup' | '/today') => {
    await completeOnboarding();
    router.replace(href);
  };
  return (
    <Screen>
      <View className="items-center gap-4 pt-10">
        <View className="size-24 items-center justify-center rounded-full bg-secondary">
          <Ionicons name="checkmark" size={56} color={colors.primary} />
        </View>
        <Text className="text-center text-[28px] font-bold leading-9 text-foreground">
          Đăng nhập thành công!
        </Text>
        <AppText muted>
          Chào mừng bạn đến với GymBase. Hãy thiết lập hồ sơ để cá nhân hóa trải nghiệm.
        </AppText>
      </View>
      <View className="gap-3 pt-6">
        <Button label="Thiết lập hồ sơ ngay" onPress={() => void go('/profile-setup')} />
        <Button label="Bỏ qua, vào Trang chủ" secondary onPress={() => void go('/today')} />
        <AppText muted>Bạn có thể thiết lập sau trong tab Cá nhân.</AppText>
      </View>
    </Screen>
  );
}
