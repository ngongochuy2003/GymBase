import { useLocalSearchParams } from 'expo-router';
import { LichSuCheckInScreen } from '@/features/lo-trinh';

export default function LichSuCheckInRoute() {
  const { id, loai } = useLocalSearchParams<{ id?: string; loai?: string }>();
  return <LichSuCheckInScreen id={id} loai={loai} />;
}
