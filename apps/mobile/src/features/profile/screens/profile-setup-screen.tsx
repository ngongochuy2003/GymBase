import { useState } from 'react';
import { Pressable, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { router } from 'expo-router';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { AuthHeader } from '@/features/auth';
import { Screen } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';
import { cn } from '@/lib/utils';

type VaiTro = 'hoc_vien' | 'pt' | 'ca_hai';
type IconName = keyof typeof Ionicons.glyphMap;

const ROLES: readonly { value: VaiTro; icon: IconName; title: string; description: string }[] = [
  {
    value: 'hoc_vien',
    icon: 'barbell-outline',
    title: 'Tôi muốn tập luyện (Học viên)',
    description: 'Theo dõi tập luyện, xây dựng lộ trình, quản lý tiến độ.',
  },
  {
    value: 'pt',
    icon: 'person-outline',
    title: 'Tôi là PT',
    description: 'Tạo & quản lý học viên, xây dựng lịch tập, theo dõi kết quả.',
  },
  {
    value: 'ca_hai',
    icon: 'people-outline',
    title: 'Tôi muốn cả hai',
    description: 'Vừa tập luyện cho bản thân, vừa huấn luyện, quản lý học viên.',
  },
];

/** Màn 02A: chọn vai trò sử dụng, ngay sau onboarding. Chọn xong sang màn chi tiết kèm `loai`. */
export function ProfileSetupScreen() {
  const { colors } = useTheme();
  const [selected, setSelected] = useState<VaiTro | null>(null);
  return (
    <Screen bottomInset>
      <AuthHeader onBack={() => router.back()} />
      <View className="gap-3 py-4">
        <Text variant="h1" className="text-center">
          Bạn muốn sử dụng GymBase như thế nào?
        </Text>
        <Text variant="muted" className="text-center">
          Chọn vai trò phù hợp với nhu cầu của bạn.
        </Text>
      </View>
      <View accessibilityRole="radiogroup" className="gap-4">
        {ROLES.map((role) => {
          const checked = selected === role.value;
          return (
            <Pressable
              key={role.value}
              accessibilityRole="radio"
              accessibilityState={{ checked }}
              accessibilityLabel={`${role.title}. ${role.description}`}
              onPress={() => setSelected(role.value)}
              className={cn(
                'w-full flex-row items-center gap-4 rounded-2xl bg-card p-4',
                checked ? 'border-2 border-primary' : 'border border-border',
              )}
            >
              <View className="size-12 items-center justify-center rounded-full bg-secondary">
                <Ionicons name={role.icon} size={24} color={colors.text} />
              </View>
              <View className="flex-1 gap-2">
                <Text className="font-bold">{role.title}</Text>
                <Text variant="muted" className="text-sm leading-5">
                  {role.description}
                </Text>
              </View>
            </Pressable>
          );
        })}
      </View>
      <View className="flex-row items-center gap-3 rounded-2xl bg-secondary p-4">
        <Ionicons name="information-circle-outline" size={22} color={colors.muted} />
        <Text variant="muted" className="flex-1 text-sm leading-5">
          Một tài khoản, một hồ sơ chung.
        </Text>
      </View>
      <Button
        shape="pill"
        accessibilityLabel="Tiếp tục"
        disabled={selected === null}
        onPress={() => {
          // TODO(PROFILE-PT): hiện cả 3 vai trò dùng chung form học viên; sau này rẽ nhánh theo `loai`.
          if (selected) router.push(`/profile-setup/ca-nhan?loai=${selected}`);
        }}
      >
        <Text>Tiếp tục</Text>
        <Ionicons name="chevron-forward" size={18} color={colors.onPrimary} />
      </Button>
    </Screen>
  );
}
