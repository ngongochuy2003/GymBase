import { Redirect } from 'expo-router';
import { useSession } from '@/core/auth/session';

export default function EntryRoute() {
  const { status, pendingWelcome } = useSession();
  if (status !== 'authenticated') return <Redirect href="/welcome" />;
  return <Redirect href={pendingWelcome ? '/register-success' : '/today'} />;
}
