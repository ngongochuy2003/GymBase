import { useLocalSearchParams } from 'expo-router';
import { CheckInBuoiTapScreen } from '@/features/lo-trinh';

export default function CheckInBuoiTapRoute() {
  const { loTrinhId, nhiemVuId } = useLocalSearchParams<{
    loTrinhId?: string;
    nhiemVuId?: string;
  }>();
  return <CheckInBuoiTapScreen loTrinhId={loTrinhId} nhiemVuId={nhiemVuId} />;
}
