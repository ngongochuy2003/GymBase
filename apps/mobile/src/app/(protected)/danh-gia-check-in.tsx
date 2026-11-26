import { useLocalSearchParams } from 'expo-router';
import { DanhGiaCheckInScreen } from '@/features/lo-trinh';

export default function DanhGiaCheckInRoute() {
  const { loTrinhId, checkInId } = useLocalSearchParams<{
    loTrinhId?: string;
    checkInId?: string;
  }>();
  return <DanhGiaCheckInScreen loTrinhId={loTrinhId} checkInId={checkInId} />;
}
