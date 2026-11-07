import { Link } from 'expo-router';
import { AppText, Screen } from '@/shared/components/ui';
export default function NotFound() {
  return (
    <Screen>
      <AppText title>Không tìm thấy màn hình</AppText>
      <Link href="/">Về trang chính</Link>
    </Screen>
  );
}
