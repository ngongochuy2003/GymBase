import { useLocalSearchParams } from 'expo-router';
import { DuyetNhanhScreen } from '@/features/lo-trinh';

export default function DuyetNhanhRoute() {
  const { loTrinhId } = useLocalSearchParams<{ loTrinhId?: string }>();
  return <DuyetNhanhScreen loTrinhId={loTrinhId} />;
}
