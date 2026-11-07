import type { PropsWithChildren } from 'react';
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
  type TextInputProps,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTheme } from '../theme/theme';

export function Screen({ children }: PropsWithChildren) {
  const { colors } = useTheme();
  return (
    <SafeAreaView
      style={{ flex: 1, backgroundColor: colors.background }}
      edges={['top', 'left', 'right']}
    >
      <ScrollView keyboardShouldPersistTaps="handled" contentContainerStyle={styles.screen}>
        <View style={styles.content}>{children}</View>
      </ScrollView>
    </SafeAreaView>
  );
}

export function AppText({
  children,
  muted = false,
  title = false,
}: PropsWithChildren<{ muted?: boolean; title?: boolean }>) {
  const { colors } = useTheme();
  return (
    <Text
      style={[styles.text, { color: muted ? colors.muted : colors.text }, title && styles.title]}
    >
      {children}
    </Text>
  );
}

export function Card({ children }: PropsWithChildren) {
  const { colors } = useTheme();
  return (
    <View style={[styles.card, { backgroundColor: colors.surface, borderColor: colors.border }]}>
      {children}
    </View>
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
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={label}
      accessibilityState={{ disabled: disabled || loading, busy: loading }}
      disabled={disabled || loading}
      onPress={onPress}
      style={({ pressed }) => [
        styles.button,
        {
          backgroundColor: secondary ? colors.surface : colors.primary,
          borderColor: colors.border,
          opacity: disabled || loading ? 0.5 : pressed ? 0.75 : 1,
        },
      ]}
    >
      {loading ? (
        <ActivityIndicator color={secondary ? colors.primary : colors.onPrimary} />
      ) : (
        <Text style={[styles.buttonText, { color: secondary ? colors.text : colors.onPrimary }]}>
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
    <View style={{ gap: 8 }}>
      <AppText>{label}</AppText>
      <TextInput
        {...props}
        accessibilityLabel={label}
        placeholderTextColor={colors.muted}
        style={[
          styles.input,
          {
            color: colors.text,
            borderColor: error ? colors.error : colors.border,
            backgroundColor: colors.surface,
          },
        ]}
      />
      {error ? (
        <Text accessibilityRole="alert" style={{ color: colors.error }}>
          {error}
        </Text>
      ) : null}
    </View>
  );
}

export function LoadingState() {
  const { colors } = useTheme();
  return (
    <View style={styles.state}>
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
  const { colors } = useTheme();
  return (
    <Card>
      <Text accessibilityRole="alert" style={{ color: colors.error }}>
        {message}
      </Text>
      <Button label="Thử lại" onPress={onRetry} secondary />
    </Card>
  );
}

const styles = StyleSheet.create({
  screen: { flexGrow: 1, padding: 24, paddingBottom: 40, alignItems: 'center' },
  content: { width: '100%', maxWidth: 680, gap: 20 },
  text: { fontSize: 15, lineHeight: 23 },
  title: { fontSize: 28, lineHeight: 36, fontWeight: '700', letterSpacing: -0.8 },
  card: { padding: 20, borderRadius: 20, borderWidth: 1, gap: 14 },
  button: {
    minHeight: 50,
    paddingHorizontal: 20,
    paddingVertical: 14,
    borderRadius: 14,
    borderWidth: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  buttonText: { fontSize: 15, fontWeight: '700' },
  input: { minHeight: 52, borderWidth: 1, borderRadius: 12, paddingHorizontal: 16, fontSize: 16 },
  state: { padding: 32, alignItems: 'center', gap: 12 },
});
