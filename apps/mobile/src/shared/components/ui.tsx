import {
  useState,
  type ComponentRef,
  type PropsWithChildren,
  type ReactNode,
  type Ref,
} from 'react';
import { Ionicons } from '@expo/vector-icons';
import {
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
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

type IconName = keyof typeof Ionicons.glyphMap;

cssInterop(SafeAreaView, { className: 'style' });

/** Khung màn hình có cuộn và né bàn phím. */
export function Screen({ children }: PropsWithChildren) {
  return (
    <SafeAreaView className="flex-1 bg-background" edges={['top', 'left', 'right']}>
      {/* iOS đẩy nội dung lên theo bàn phím; Android đã tự co cửa sổ (adjustResize) nên chỉ cần cuộn tới ô đang gõ. */}
      <KeyboardAvoidingView
        className="flex-1"
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      >
        <ScrollView
          keyboardShouldPersistTaps="handled"
          automaticallyAdjustKeyboardInsets
          contentContainerClassName="flex-grow items-center p-6 pb-10"
        >
          <View className="w-full max-w-[680px] gap-5">{children}</View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

/**
 * Khung màn hình không cuộn, vừa khít màn hình: các khối có chiều cao riêng (logo, cụm nút) giữ nguyên, khối `flex-1` ăn phần
 * còn lại. Chừa vùng an toàn cả bốn phía (thanh home trên iPhone, thanh điều hướng trên Android). Dùng cho màn không có ô
 * nhập, như màn chào mừng.
 */
export function FixedScreen({ children }: PropsWithChildren) {
  return (
    <SafeAreaView className="flex-1 bg-background" edges={['top', 'bottom', 'left', 'right']}>
      <View className="w-full max-w-[680px] flex-1 gap-5 self-center p-6 pb-4">{children}</View>
    </SafeAreaView>
  );
}

export function AppText({
  children,
  muted = false,
  title = false,
  className,
}: PropsWithChildren<{ muted?: boolean; title?: boolean; className?: string }>) {
  return (
    <Text
      className={cn(
        'text-[15px] leading-[23px]',
        muted ? 'text-muted-foreground' : 'text-foreground',
        title && 'text-[28px] font-bold leading-9 tracking-[-0.8px]',
        className,
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
  pill = false,
  leading,
  trailingIcon,
}: {
  label: string;
  onPress(): void;
  secondary?: boolean;
  disabled?: boolean;
  loading?: boolean;
  /** Bo tròn hẳn, dùng cho nút ở các màn auth theo thiết kế. */
  pill?: boolean;
  /** Nội dung đặt trước nhãn, ví dụ logo Google. */
  leading?: ReactNode;
  /** Biểu tượng cuối nhãn, ví dụ mũi tên `>`. */
  trailingIcon?: IconName;
}) {
  const { colors } = useTheme();
  const inactive = disabled || loading;
  const textColor = secondary ? colors.text : colors.onPrimary;
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={label}
      accessibilityState={{ disabled: inactive, busy: loading }}
      disabled={inactive}
      onPress={onPress}
      className={cn(
        'min-h-[50px] items-center justify-center border border-border px-5 py-3.5',
        pill ? 'rounded-full' : 'rounded-[14px]',
        secondary ? 'bg-card' : 'bg-primary',
        inactive ? 'opacity-50' : 'active:opacity-75',
      )}
    >
      {loading ? (
        <ActivityIndicator color={secondary ? colors.primary : colors.onPrimary} />
      ) : (
        <View className="flex-row items-center justify-center gap-2.5">
          {leading}
          <Text
            className={cn(
              'text-[15px] font-bold',
              secondary ? 'text-foreground' : 'text-primary-foreground',
            )}
          >
            {label}
          </Text>
          {trailingIcon ? <Ionicons name={trailingIcon} size={18} color={textColor} /> : null}
        </View>
      )}
    </Pressable>
  );
}

export function TextField({
  label,
  error,
  icon,
  passwordToggle = false,
  hideLabel = false,
  trailing,
  inputRef,
  errorAction,
  ...props
}: TextInputProps & {
  /** Tên ô: hiện phía trên ô, và luôn là nhãn truy cập cho trình đọc màn hình. */
  label: string;
  error?: string;
  /** Kiểu thiết kế auth: không hiện nhãn phía trên, `label` làm placeholder (trừ khi truyền `placeholder`). */
  hideLabel?: boolean;
  /** Nội dung cuối ô, ví dụ biểu tượng (i). */
  trailing?: ReactNode;
  /** Ref của ô nhập, để form đưa con trỏ vào ô lỗi (`field.ref` của react-hook-form). */
  inputRef?: Ref<ComponentRef<typeof TextInput>>;
  /** Nút hành động nối ngay sau câu lỗi, dạng `<Text>` có `onPress` (ví dụ liên kết "Đăng nhập"). */
  errorAction?: ReactNode;
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
      {hideLabel ? null : <AppText>{label}</AppText>}
      <View
        className={cn(
          'min-h-[52px] flex-row items-center gap-3 rounded-xl border bg-card px-4',
          error ? 'border-destructive' : 'border-border',
        )}
      >
        {icon ? <Ionicons name={icon} size={20} color={colors.muted} /> : null}
        <TextInput
          {...props}
          ref={inputRef}
          placeholder={props.placeholder ?? (hideLabel ? label : undefined)}
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
        {trailing}
      </View>
      {error ? (
        <Text accessibilityRole="alert" className="text-destructive">
          {error}
          {errorAction ? ' ' : null}
          {errorAction}
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
