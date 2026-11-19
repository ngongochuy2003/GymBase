import type { ReactNode } from 'react';
import { Pressable, StyleSheet, Text, View, type StyleProp, type ViewStyle } from 'react-native';
import { Icon } from '../icons';
import { theme } from '../theme/tokens';
import { RemoteImage } from './remote-image';

export interface HeaderAction {
  id: string;
  icon: string;
  label?: string;
  /** Chấm xanh thông báo. */
  badge?: boolean;
}

export interface AppHeaderProps {
  rightActions?: HeaderAction[];
  avatarUri?: string;
  /** Chỉ hiển thị avatar khi prop này bật (mặc định false). */
  showAvatar?: boolean;
  onActionPress?(id: string): void;
  onAvatarPress?(): void;
  renderAction?(action: HeaderAction): ReactNode;
  style?: StyleProp<ViewStyle>;
}

export function AppHeader({
  rightActions = [],
  avatarUri,
  showAvatar = false,
  onActionPress,
  onAvatarPress,
  renderAction,
  style,
}: AppHeaderProps) {
  return (
    <View style={[styles.header, style]}>
      <View style={styles.brand}>
        <Text style={styles.brandText}>GymBase</Text>
      </View>
      <View style={styles.actions}>
        {rightActions.map((action) =>
          renderAction ? (
            renderAction(action)
          ) : (
            <Pressable
              key={action.id}
              accessibilityRole="button"
              accessibilityLabel={action.label ?? action.id}
              onPress={() => onActionPress?.(action.id)}
              hitSlop={8}
            >
              <Icon name={action.icon} size={24} />
              {action.badge ? <View testID={`badge-${action.id}`} style={styles.badge} /> : null}
            </Pressable>
          ),
        )}
        {showAvatar && (avatarUri || onAvatarPress) ? (
          <Pressable
            accessibilityRole="button"
            accessibilityLabel="Ảnh đại diện"
            onPress={onAvatarPress}
            style={styles.avatarWrapper}
          >
            <RemoteImage uri={avatarUri} circle style={styles.avatar} />
          </Pressable>
        ) : null}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  header: {
    height: 38,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingTop: 4,
    paddingBottom: 4,
  },
  brand: { flexDirection: 'row', alignItems: 'center', gap: theme.spacing.sm },
  logo: { fontSize: 26, fontWeight: '700', color: theme.colors.primary, fontStyle: 'italic' },
  brandText: {
    fontSize: 21.5,
    fontWeight: '700',
    color: theme.colors.primary,
  },
  actions: { flexDirection: 'row', alignItems: 'center', gap: 18 },
  badge: {
    position: 'absolute',
    top: 0,
    right: 1,
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: '#E5484D',
    borderWidth: 1.5,
    borderColor: theme.colors.background,
  },
  avatarWrapper: { width: 34, height: 34, borderRadius: 17, overflow: 'hidden' },
  avatar: { width: 34, height: 34, borderRadius: 17 },
});
