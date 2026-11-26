import { useLocalSearchParams } from 'expo-router';
import { DatBuoiTapScreen } from '@/features/lo-trinh';

export default function DatBuoiTapRoute() {
  const { tuan, thu } = useLocalSearchParams<{ tuan?: string; thu?: string }>();
  return <DatBuoiTapScreen tuan={tuan} thu={thu} />;
}
