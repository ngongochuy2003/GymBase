import { useState } from 'react';
import { Text, View } from 'react-native';
import { router } from 'expo-router';
import { AppText, Button, Screen } from '@/shared/components/ui';

export function WelcomeScreen() {
  const [notice, setNotice] = useState<string | null>(null);
  // TODO(GOOGLE): nối "Đăng nhập với Google" khi làm đăng nhập Google trên mobile (#20).
  const comingSoon = () => setNotice('Tính năng này sắp có.');
  return (
    <Screen>
      <View className="flex-row items-center gap-3 py-3">
        <View className="size-11 items-center justify-center rounded-[14px] bg-primary">
          <Text className="text-[22px] font-black text-primary-foreground">G</Text>
        </View>
        <AppText title>GymBase</AppText>
      </View>
      {/* TODO(ASSET): thay khối này bằng ảnh hero và carousel trong thiết kế khi có asset. */}
      <View className="gap-5 rounded-[28px] bg-primary-dark p-7">
        <Text className="text-[34px] font-extrabold leading-[41px] tracking-[-1px] text-primary-foreground">
          Kết nối PT, theo dõi lộ trình và phát triển bản thân cùng GymBase
        </Text>
        <Text className="text-base leading-[25px] text-primary-foreground opacity-85">
          Một nơi để bạn và PT đồng hành trên từng buổi tập.
        </Text>
      </View>
      <View className="gap-3">
        <Button label="Đăng ký tài khoản" onPress={() => router.push('/register')} />
        <Button label="Đăng nhập" secondary onPress={() => router.push('/login')} />
        <AppText muted>Hoặc tiếp tục với</AppText>
        <Button label="Đăng nhập với Google" secondary onPress={comingSoon} />
        {notice ? (
          <Text accessibilityRole="alert" className="text-muted-foreground">
            {notice}
          </Text>
        ) : null}
      </View>
    </Screen>
  );
}
