import { useState } from 'react';
import { View } from 'react-native';
import { useSession } from '@/core/auth/session';
import { pickTabs, useTabNavigation } from '@/shared/navigation';
import { AppText, Button, Card, Screen } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';
import { BottomTabBar } from '@/shared/ui-kit';

const PROFILE_TABS = pickTabs(['home', 'discover', 'compose', 'messages', 'profile']);

export function ProfileScreen() {
  const { user, signOut } = useSession();
  const { isDark, toggle, colors } = useTheme();
  const onTabPress = useTabNavigation();
  const [busy, setBusy] = useState(false);
  return (
    <View style={{ flex: 1, backgroundColor: colors.background }}>
      <View style={{ flex: 1 }}>
        <Screen>
          <AppText title>Không gian của bạn</AppText>
          <Card>
            <AppText title>{user?.name}</AppText>
            <AppText muted>Người trải nghiệm · Phiên demo</AppText>
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
          <AppText muted>
            Đây là hồ sơ demo. Tính năng tài khoản, PT và dữ liệu thật sẽ được kết nối với backend ở
            giai đoạn tiếp theo.
          </AppText>
          <Button
            label="Thoát bản demo"
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
      </View>
      <BottomTabBar tabs={PROFILE_TABS} activeId="profile" onChange={onTabPress} />
    </View>
  );
}
