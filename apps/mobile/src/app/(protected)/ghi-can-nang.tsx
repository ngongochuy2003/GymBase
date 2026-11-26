import { useLocalSearchParams } from 'expo-router';
import { GhiCanNangScreen } from '@/features/lo-trinh';

export default function GhiCanNangRoute() {
  const { loTrinhId, nhiemVuId } = useLocalSearchParams<{
    loTrinhId?: string;
    nhiemVuId?: string;
  }>();
  return <GhiCanNangScreen loTrinhId={loTrinhId} nhiemVuId={nhiemVuId} />;
}
