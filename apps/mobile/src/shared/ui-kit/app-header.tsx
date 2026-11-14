import type { ReactNode } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
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
  onActionPress?(id: string): void;
  onAvatarPress?(): void;
  renderAction?(action: HeaderAction): ReactNode;
}

export function AppHeader({
  rightActions = [],
  avatarUri,
  onActionPress,
  onAvatarPress,
  renderAction,
}: AppHeaderProps) {
  return (
    <View style={styles.row}>
      <View style={styles.brand}>
        <Text style={styles.logo}>G</Text>
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
              <Icon name={action.icon} size={theme.iconSize.lg} />
              {action.badge ? <View testID={`badge-${action.id}`} style={styles.badge} /> : null}
            </Pressable>
          ),
        )}
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Ảnh đại diện"
          onPress={onAvatarPress}
        >
          <RemoteImage uri={avatarUri} circle style={styles.avatar} />
        </Pressable>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  row: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' },
  brand: { flexDirection: 'row', alignItems: 'center', gap: theme.spacing.sm },
  logo: { fontSize: 34, fontWeight: '900', color: theme.colors.primary, fontStyle: 'italic' },
  brandText: {
    fontSize: theme.font.size.xl,
    fontWeight: theme.font.weight.bold,
    fontStyle: 'italic',
    color: theme.colors.text,
  },
  actions: { flexDirection: 'row', alignItems: 'center', gap: theme.spacing.xl },
  badge: {
    position: 'absolute',
    top: 0,
    right: 1,
    width: 9,
    height: 9,
    borderRadius: 5,
    backgroundColor: theme.colors.primary,
    borderWidth: 1.5,
    borderColor: theme.colors.background,
  },
  avatar: { width: 44, height: 44 },
});
