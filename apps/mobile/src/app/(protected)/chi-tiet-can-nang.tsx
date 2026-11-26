import { useLocalSearchParams } from 'expo-router';
import { ChiTietCanNangScreen } from '@/features/lo-trinh';

export default function ChiTietCanNangRoute() {
  const { id } = useLocalSearchParams<{ id?: string }>();
  return <ChiTietCanNangScreen id={id} />;
}
