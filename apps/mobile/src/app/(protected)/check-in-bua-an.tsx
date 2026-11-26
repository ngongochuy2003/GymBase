import { useLocalSearchParams } from 'expo-router';
import { CheckInBuaAnScreen } from '@/features/lo-trinh';

export default function CheckInBuaAnRoute() {
  const { loTrinhId, nhiemVuId } = useLocalSearchParams<{
    loTrinhId?: string;
    nhiemVuId?: string;
  }>();
  return <CheckInBuaAnScreen loTrinhId={loTrinhId} nhiemVuId={nhiemVuId} />;
}
