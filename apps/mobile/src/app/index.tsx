import { Redirect } from 'expo-router';
import { useSession } from '@/core/auth/session';

export default function EntryRoute() {
  const { status } = useSession();
  return <Redirect href={status === 'demo' ? '/home' : '/login'} />;
}
