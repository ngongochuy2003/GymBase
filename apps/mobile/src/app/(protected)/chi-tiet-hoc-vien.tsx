import { useLocalSearchParams } from 'expo-router';
import { ChiTietLoTrinhHocVienScreen } from '@/features/lo-trinh';

export default function ChiTietHocVienRoute() {
  const { id } = useLocalSearchParams<{ id?: string }>();
  return <ChiTietLoTrinhHocVienScreen id={id} />;
}
