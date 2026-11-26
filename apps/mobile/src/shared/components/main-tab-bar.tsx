import { Pressable, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Icon } from '@/shared/icons';
import { MAIN_TABS, useTabNavigation } from '@/shared/navigation';
import { useTheme } from '../theme/theme';

const THU_TU_TAB = ['home', 'route', 'compose', 'messages', 'profile'] as const;

/** Thanh tab dưới của các màn tab gốc; `activeId` là id trong `MAIN_TABS`. */
// TODO(UI): BottomTabBar trong ui-kit còn dùng hệ token cũ (chỉ sáng); khi ui-kit chuyển sang NativeWind thì gộp vào đó.
export function MainTabBar({ activeId }: { activeId: string }) {
  const { colors } = useTheme();
  const insets = useSafeAreaInsets();
  const onTabPress = useTabNavigation();
  return (
    <View
      accessibilityRole="tablist"
      className="flex-row items-center justify-around border-t border-border bg-card px-4 pt-2"
      style={{ paddingBottom: 8 + insets.bottom }}
    >
      {THU_TU_TAB.map((id) => {
        const tab = MAIN_TABS[id];
        if (!tab) return null;
        const dangChon = id === activeId;
        const mau = dangChon ? colors.primary : colors.muted;
        return (
          <Pressable
            key={id}
            accessibilityRole="tab"
            accessibilityLabel={tab.label}
            accessibilityState={{ selected: dangChon }}
            onPress={() => onTabPress(tab)}
            className="flex-1 items-center justify-center gap-1"
          >
            {tab.isCenter ? (
              <View className="h-11 w-11 items-center justify-center rounded-2xl bg-primary-dark">
                <Icon name={tab.icon} size={22} color={colors.onPrimary} />
              </View>
            ) : (
              <>
                <Icon
                  name={dangChon && tab.activeIcon ? tab.activeIcon : tab.icon}
                  size={24}
                  color={mau}
                />
                <Text
                  className={
                    dangChon
                      ? 'text-xs font-medium text-primary'
                      : 'text-xs font-medium text-muted-foreground'
                  }
                >
                  {tab.label}
                </Text>
              </>
            )}
          </Pressable>
        );
      })}
    </View>
  );
}
