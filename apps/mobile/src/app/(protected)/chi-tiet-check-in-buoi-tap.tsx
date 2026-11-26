import { useLocalSearchParams } from 'expo-router';
import { ChiTietCheckInBuoiTapScreen } from '@/features/lo-trinh';

export default function ChiTietCheckInBuoiTapRoute() {
  const { loTrinhId, nhiemVuId } = useLocalSearchParams<{
    loTrinhId?: string;
    nhiemVuId?: string;
  }>();
  return <ChiTietCheckInBuoiTapScreen loTrinhId={loTrinhId} nhiemVuId={nhiemVuId} />;
}
