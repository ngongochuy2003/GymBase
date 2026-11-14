import { useState, type PropsWithChildren, type ReactNode } from 'react';
import { Ionicons } from '@expo/vector-icons';
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  Text,
  TextInput,
  View,
  type TextInputProps,
} from 'react-native';
import { cssInterop } from 'nativewind';
import { SafeAreaView } from 'react-native-safe-area-context';
import { cn } from '@/lib/utils';
import { useTheme } from '../theme/theme';

cssInterop(SafeAreaView, { className: 'style' });

export function Screen({ children, footer }: PropsWithChildren<{ footer?: ReactNode }>) {
  return (
    <SafeAreaView
      className="flex-1 bg-background"
      edges={footer ? ['top', 'left', 'right', 'bottom'] : ['top', 'left', 'right']}
    >
      <ScrollView
        keyboardShouldPersistTaps="handled"
        contentContainerClassName="flex-grow items-center p-6 pb-10"
      >
        <View className="w-full max-w-[680px] gap-5">{children}</View>
      </ScrollView>
      {footer}
    </SafeAreaView>
  );
}

export function AppText({
  children,
  muted = false,
  title = false,
}: PropsWithChildren<{ muted?: boolean; title?: boolean }>) {
  return (
    <Text
      className={cn(
        'text-[15px] leading-[23px]',
        muted ? 'text-muted-foreground' : 'text-foreground',
        title && 'text-[28px] font-bold leading-9 tracking-[-0.8px]',
      )}
    >
      {children}
    </Text>
  );
}

export function Card({ children }: PropsWithChildren) {
  return (
    <View className="gap-3.5 rounded-[20px] border border-border bg-card p-5">{children}</View>
  );
}

export function Button({
  label,
  onPress,
  secondary = false,
  disabled = false,
  loading = false,
}: {
  label: string;
  onPress(): void;
  secondary?: boolean;
  disabled?: boolean;
  loading?: boolean;
}) {
  const { colors } = useTheme();
  const inactive = disabled || loading;
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={label}
      accessibilityState={{ disabled: inactive, busy: loading }}
      disabled={inactive}
      onPress={onPress}
      className={cn(
        'min-h-[50px] items-center justify-center rounded-[14px] border border-border px-5 py-3.5',
        secondary ? 'bg-card' : 'bg-primary',
        inactive ? 'opacity-50' : 'active:opacity-75',
      )}
    >
      {loading ? (
        <ActivityIndicator color={secondary ? colors.primary : colors.onPrimary} />
      ) : (
        <Text
          className={cn(
            'text-[15px] font-bold',
            secondary ? 'text-foreground' : 'text-primary-foreground',
          )}
        >
          {label}
        </Text>
      )}
    </Pressable>
  );
}

type IconName = keyof typeof Ionicons.glyphMap;

export function TextField({
  label,
  error,
  icon,
  passwordToggle = false,
  ...props
}: TextInputProps & {
  label: string;
  error?: string;
  /** Biểu tượng bên trái ô nhập. */
  icon?: IconName;
  /** Thêm nút hiện/ẩn nội dung, dùng cho ô mật khẩu. */
  passwordToggle?: boolean;
}) {
  const { colors } = useTheme();
  const [visible, setVisible] = useState(false);
  const toggleLabel = `${visible ? 'Ẩn' : 'Hiện'} ${label.toLowerCase()}`;
  return (
    <View className="gap-2">
      <AppText>{label}</AppText>
      <View
        className={cn(
          'min-h-[52px] flex-row items-center gap-3 rounded-xl border bg-card px-4',
          error ? 'border-destructive' : 'border-border',
        )}
      >
        {icon ? <Ionicons name={icon} size={20} color={colors.muted} /> : null}
        <TextInput
          {...props}
          secureTextEntry={passwordToggle ? !visible : props.secureTextEntry}
          accessibilityLabel={label}
          placeholderTextColor={colors.muted}
          className="flex-1 py-3 text-base text-foreground"
        />
        {passwordToggle ? (
          <Pressable
            accessibilityRole="button"
            accessibilityLabel={toggleLabel}
            hitSlop={8}
            onPress={() => setVisible((current) => !current)}
          >
            <Ionicons
              name={visible ? 'eye-off-outline' : 'eye-outline'}
              size={20}
              color={colors.muted}
            />
          </Pressable>
        ) : null}
      </View>
      {error ? (
        <Text accessibilityRole="alert" className="text-destructive">
          {error}
        </Text>
      ) : null}
    </View>
  );
}

/** Ô tick; `label` là tên cho trình đọc màn hình, `children` là nội dung hiển thị (có thể chứa liên kết). */
export function Checkbox({
  checked,
  onChange,
  label,
  error,
  children,
}: {
  checked: boolean;
  onChange(checked: boolean): void;
  label: string;
  error?: string;
  children: ReactNode;
}) {
  const { colors } = useTheme();
  return (
    <View className="gap-2">
      <View className="flex-row items-start gap-3">
        <Pressable
          accessibilityRole="checkbox"
          accessibilityLabel={label}
          accessibilityState={{ checked }}
          hitSlop={8}
          onPress={() => onChange(!checked)}
          className={cn(
            'mt-0.5 size-6 items-center justify-center rounded-md border',
            checked ? 'border-primary bg-primary' : error ? 'border-destructive' : 'border-border',
          )}
        >
          {checked ? <Ionicons name="checkmark" size={16} color={colors.onPrimary} /> : null}
        </Pressable>
        <Text className="flex-1 text-[15px] leading-[23px] text-foreground">{children}</Text>
      </View>
      {error ? (
        <Text accessibilityRole="alert" className="text-destructive">
          {error}
        </Text>
      ) : null}
    </View>
  );
}

export function LoadingState() {
  const { colors } = useTheme();
  return (
    <View className="items-center gap-3 p-8">
      <ActivityIndicator color={colors.primary} />
      <AppText muted>Đang tải dữ liệu…</AppText>
    </View>
  );
}

export function EmptyState({ message }: { message: string }) {
  return (
    <Card>
      <AppText muted>{message}</AppText>
    </Card>
  );
}

export function ErrorState({ message, onRetry }: { message: string; onRetry(): void }) {
  return (
    <Card>
      <Text accessibilityRole="alert" className="text-destructive">
        {message}
      </Text>
      <Button label="Thử lại" onPress={onRetry} secondary />
    </Card>
  );
}
