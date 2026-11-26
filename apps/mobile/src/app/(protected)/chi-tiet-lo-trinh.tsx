import { useLocalSearchParams } from 'expo-router';
import { ChiTietLoTrinhScreen } from '@/features/lo-trinh';

export default function ChiTietLoTrinhRoute() {
  const { id } = useLocalSearchParams<{ id?: string }>();
  return <ChiTietLoTrinhScreen id={id} />;
}
