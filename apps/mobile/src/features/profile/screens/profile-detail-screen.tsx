import { router } from 'expo-router';
import { AppText, Button, Card, Screen } from '@/shared/components/ui';

// TODO(PROFILE): màn giữ chỗ; thay bằng màn 03A "Thiết lập chi tiết hồ sơ" và nối API hồ sơ (ticket #30).
export function ProfileDetailScreen() {
  return (
    <Screen bottomInset>
      <AppText title>Thiết lập hồ sơ</AppText>
      <Card>
        <AppText muted>Phần nhập chi tiết hồ sơ sắp có.</AppText>
      </Card>
      <Button label="Quay lại" secondary onPress={() => router.back()} />
    </Screen>
  );
}
