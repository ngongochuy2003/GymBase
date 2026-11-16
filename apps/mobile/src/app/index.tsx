import { Redirect } from 'expo-router';
import { useSession } from '@/core/auth/session';

export default function EntryRoute() {
  const { status, user } = useSession();
  if (status !== 'authenticated') return <Redirect href="/welcome" />;
  return <Redirect href={user?.is_dang_nhap_lan_dau ? '/onboarding' : '/home'} />;
}
