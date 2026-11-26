import { useLocalSearchParams } from 'expo-router';
import { LoiMoiLoTrinhScreen } from '@/features/lo-trinh';

export default function LoiMoiLoTrinhRoute() {
  const { id } = useLocalSearchParams<{ id?: string }>();
  return <LoiMoiLoTrinhScreen id={id} />;
}
