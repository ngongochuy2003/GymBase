import type { ReactNode } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
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
  return (
    <View style={styles.bar} accessibilityRole="tablist">
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
                  <Icon name={iconName} size={30} color={color} />
                )}
              </View>
            ) : (
              <>
                <View>
                  {renderIcon ? (
                    renderIcon(tab, active, color)
                  ) : (
                    <Icon name={iconName} size={theme.iconSize.lg} color={color} />
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
    flexDirection: 'row',
    alignItems: 'flex-end',
    justifyContent: 'space-around',
    backgroundColor: theme.colors.card,
    borderTopLeftRadius: theme.radius.card + 8,
    borderTopRightRadius: theme.radius.card + 8,
    paddingTop: theme.spacing.lg,
    paddingBottom: theme.spacing.md,
    paddingHorizontal: theme.spacing.xs,
    shadowColor: '#000',
    shadowOpacity: 0.06,
    shadowRadius: 12,
    shadowOffset: { width: 0, height: -3 },
    elevation: 8,
  },
  item: { flex: 1, alignItems: 'center', justifyContent: 'flex-end', gap: 4, minHeight: 48 },
  label: { fontSize: theme.font.size.xs, fontWeight: theme.font.weight.medium },
  center: {
    width: 58,
    height: 58,
    borderRadius: 20,
    backgroundColor: theme.colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: theme.spacing.sm,
  },
  badge: {
    position: 'absolute',
    top: -2,
    right: -4,
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: theme.colors.primary,
  },
});
