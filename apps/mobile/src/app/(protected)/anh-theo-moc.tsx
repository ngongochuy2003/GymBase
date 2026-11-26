import { useLocalSearchParams } from 'expo-router';
import { AnhTheoMocScreen } from '@/features/lo-trinh';

export default function AnhTheoMocRoute() {
  const { id } = useLocalSearchParams<{ id?: string }>();
  return <AnhTheoMocScreen id={id} />;
}
