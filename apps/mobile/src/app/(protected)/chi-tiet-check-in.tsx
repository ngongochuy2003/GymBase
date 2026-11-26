import { useLocalSearchParams } from 'expo-router';
import { ChiTietCheckInBuaAnScreen } from '@/features/lo-trinh';

export default function ChiTietCheckInRoute() {
  const { loTrinhId, nhiemVuId } = useLocalSearchParams<{
    loTrinhId?: string;
    nhiemVuId?: string;
  }>();
  return <ChiTietCheckInBuaAnScreen loTrinhId={loTrinhId} nhiemVuId={nhiemVuId} />;
}
