import { router } from 'expo-router';
import { View } from 'react-native';
import { Button, FixedScreen } from '@/shared/components/ui';
import { Appear, Rise } from '@/shared/motion';
import { AuthHeader } from '../components/auth-header';
import { GoogleSignInButton } from '../components/google-sign-in-button';
import { LegalNotice } from '../components/legal-notice';
import { OrDivider } from '../components/or-divider';
import { WelcomeHero } from '../components/welcome-hero';

/** Logo hiện dần, hero hiện dần (trong `WelcomeHero`), rồi ba nhóm nút trồi lên lần lượt cách nhau 60ms. */
export function WelcomeScreen() {
  return (
    <FixedScreen>
      <Appear>
        <AuthHeader />
      </Appear>
      <WelcomeHero />
      <Rise delay={120}>
        <View className="gap-3">
          <Button pill label="Đăng ký tài khoản" onPress={() => router.push('/register')} />
          <Button pill label="Đăng nhập" secondary onPress={() => router.push('/login')} />
        </View>
      </Rise>
      <Rise delay={180} style={{ gap: 20 }}>
        <OrDivider />
        <GoogleSignInButton />
      </Rise>
      <Rise delay={240}>
        <LegalNotice />
      </Rise>
    </FixedScreen>
  );
}
