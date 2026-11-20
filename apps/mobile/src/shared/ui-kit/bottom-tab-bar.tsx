import type { ReactNode } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Icon } from '../icons';
import { theme } from '../theme/tokens';

export interface TabItem {
  id: string;
  label: string;
  icon: string;
  /** Icon khi đang chọn (ví dụ bản đặc). */
  activeIcon?: string;
  /** Nút giữa nổi, tròn xanh. */
  isCenter?: boolean;
  badge?: boolean;
  /** Đường dẫn điều hướng; không có thì tab chưa có màn hình. */
  href?: string;
}

export interface BottomTabBarProps {
  tabs: TabItem[];
  activeId?: string;
  onChange?(tab: TabItem): void;
  renderIcon?(tab: TabItem, active: boolean, color: string): ReactNode;
}

export function BottomTabBar({ tabs, activeId, onChange, renderIcon }: BottomTabBarProps) {
  const insets = useSafeAreaInsets();
  const bottomInset = insets.bottom;

  return (
    <View
      style={[
        styles.bar,
        bottomInset > 0 && {
          paddingBottom: 8 + bottomInset,
          height: 64 + bottomInset,
        },
      ]}
      accessibilityRole="tablist"
    >
      {tabs.map((tab) => {
        const active = tab.id === activeId;
        const color = tab.isCenter
          ? theme.colors.onPrimary
          : active
            ? theme.colors.primary
            : theme.colors.muted;
        const iconName = active && tab.activeIcon ? tab.activeIcon : tab.icon;
        return (
          <Pressable
            key={tab.id}
            accessibilityRole="tab"
            accessibilityLabel={tab.label}
            accessibilityState={{ selected: active }}
            onPress={() => onChange?.(tab)}
            style={styles.item}
          >
            {tab.isCenter ? (
              <View style={styles.center}>
                {renderIcon ? (
                  renderIcon(tab, active, color)
                ) : (
                  <Icon name={iconName} size={22} color={color} />
                )}
              </View>
            ) : (
              <>
                <View>
                  {renderIcon ? (
                    renderIcon(tab, active, color)
                  ) : (
                    <Icon name={iconName} size={24} color={color} />
                  )}
                  {tab.badge ? <View style={styles.badge} /> : null}
                </View>
                <Text style={[styles.label, { color }]}>{tab.label}</Text>
              </>
            )}
          </Pressable>
        );
      })}
    </View>
  );
}

const styles = StyleSheet.create({
  bar: {
    height: 64,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-around',
    backgroundColor: theme.colors.card,
    paddingTop: 7,
    paddingBottom: 8,
    paddingHorizontal: 16,
    shadowColor: '#000',
    shadowOpacity: 0.06,
    shadowRadius: 12,
    shadowOffset: { width: 0, height: -3 },
    elevation: 8,
  },
  item: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 3 },
  label: { fontSize: 11.5, fontWeight: theme.font.weight.medium },
  center: {
    width: 42,
    height: 42,
    borderRadius: 14,
    backgroundColor: '#455442',
    alignItems: 'center',
    justifyContent: 'center',
  },
  badge: {
    position: 'absolute',
    top: -2,
    right: -4,
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: '#E5484D',
  },
});
