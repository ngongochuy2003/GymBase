import { useState } from 'react';
import { useSession } from '@/core/auth/session';
import { AppText, Button, Card, Screen } from '@/shared/components/ui';
import { pickTabs, useTabNavigation } from '@/shared/navigation';
import { useTheme } from '@/shared/theme/theme';
import { BottomTabBar } from '@/shared/ui-kit';

const profileTabs = pickTabs(['home', 'route', 'compose', 'messages', 'profile']);

export function ProfileScreen() {
  const { user, signOut } = useSession();
  const { isDark, toggle } = useTheme();
  const onTabPress = useTabNavigation();
  const [busy, setBusy] = useState(false);
  return (
    <Screen
      footer={
        <BottomTabBar tabs={profileTabs} activeId="profile" onChange={onTabPress} />
      }
    >
      <AppText title>Không gian của bạn</AppText>
      <Card>
        <AppText title>{user?.ho_ten}</AppText>
        <AppText muted>{user?.email ?? user?.so_dien_thoai ?? ''}</AppText>
      </Card>
      <Card>
        <AppText title>Giao diện</AppText>
        <AppText muted>Hiện tại: {isDark ? 'Tối' : 'Sáng'}</AppText>
        <Button
          label={isDark ? 'Chuyển sang giao diện sáng' : 'Chuyển sang giao diện tối'}
          onPress={toggle}
          secondary
        />
      </Card>
      <Button
        label="Đăng xuất"
        loading={busy}
        onPress={() => {
          setBusy(true);
          void signOut()
            .catch(() => {
              /* Session is already revoked locally. */
            })
            .finally(() => setBusy(false));
        }}
      />
    </Screen>
  );
}
