import type { PropsWithChildren } from 'react';
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

export function Screen({ children }: PropsWithChildren) {
  return (
    <SafeAreaView className="flex-1 bg-background" edges={['top', 'left', 'right']}>
      <ScrollView
        keyboardShouldPersistTaps="handled"
        contentContainerClassName="flex-grow items-center p-6 pb-10"
      >
        <View className="w-full max-w-[680px] gap-5">{children}</View>
      </ScrollView>
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

export function TextField({
  label,
  error,
  ...props
}: TextInputProps & { label: string; error?: string }) {
  const { colors } = useTheme();
  return (
    <View className="gap-2">
      <AppText>{label}</AppText>
      <TextInput
        {...props}
        accessibilityLabel={label}
        placeholderTextColor={colors.muted}
        className={cn(
          'min-h-[52px] rounded-xl border bg-card px-4 text-base text-foreground',
          error ? 'border-destructive' : 'border-border',
        )}
      />
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
