import { useLocalSearchParams } from 'expo-router';
import { DatBuaAnScreen } from '@/features/lo-trinh';

export default function DatBuaAnRoute() {
  const { buaId } = useLocalSearchParams<{ buaId?: string }>();
  return <DatBuaAnScreen buaId={buaId} />;
}
