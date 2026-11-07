import { useState } from 'react';
import { useSession } from '@/core/auth/session';
import { AppText, Button, Card, Screen } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';

export function ProfileScreen() {
  const { user, signOut } = useSession();
  const { isDark, toggle } = useTheme();
  const [busy, setBusy] = useState(false);
  return (
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
  );
}
