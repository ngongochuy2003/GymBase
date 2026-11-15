import { Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { router } from 'expo-router';
import { useSession } from '@/core/auth/session';
import { AppText, Button, Screen } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';
import { AuthHeader } from '../components/auth-header';
import { SuccessBadge } from '../components/success-badge';

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
      <AuthHeader />
      <View className="items-center gap-4 pt-6">
        <SuccessBadge />
        <Text className="text-center text-[28px] font-bold leading-9 text-foreground">
          Đăng nhập thành công!
        </Text>
        <AppText muted className="text-center">
          Chào mừng bạn đến với GymBase. Hãy thiết lập hồ sơ để cá nhân hóa trải nghiệm.
        </AppText>
      </View>
      <View className="flex-row items-center gap-3 rounded-2xl bg-secondary p-4">
        <Ionicons name="information-circle-outline" size={22} color={colors.muted} />
        <Text className="flex-1 text-sm leading-5 text-muted-foreground">
          Bạn có thể thiết lập sau trong tab Cá nhân.
        </Text>
      </View>
      <View className="gap-3 pt-2">
        <Button
          pill
          label="Thiết lập hồ sơ ngay"
          trailingIcon="chevron-forward"
          onPress={() => void go('/profile-setup')}
        />
        <Button
          pill
          label="Bỏ qua, vào Trang chủ"
          secondary
          trailingIcon="chevron-forward"
          onPress={() => void go('/today')}
        />
      </View>
    </Screen>
  );
}
