import { router } from 'expo-router';
import { AppText, Button, Card, Screen } from '@/shared/components/ui';

// TODO(PROFILE): màn giữ chỗ; thay bằng bước chọn nhóm người dùng và thiết lập hồ sơ (ticket sau).
export function ProfileSetupScreen() {
  return (
    <Screen>
      <AppText title>Thiết lập hồ sơ</AppText>
      <Card>
        <AppText muted>
          Hồ sơ giúp GymBase biết bạn là học viên hay PT. Phần này sắp có, bạn có thể quay lại sau.
        </AppText>
      </Card>
      <Button label="Để sau" secondary onPress={() => router.replace('/home')} />
    </Screen>
  );
}
