import { useLocalSearchParams } from 'expo-router';
import { GhiAnhCoTheScreen } from '@/features/lo-trinh';

export default function GhiAnhCoTheRoute() {
  const { loTrinhId, nhiemVuId } = useLocalSearchParams<{
    loTrinhId?: string;
    nhiemVuId?: string;
  }>();
  return <GhiAnhCoTheScreen loTrinhId={loTrinhId} nhiemVuId={nhiemVuId} />;
}
