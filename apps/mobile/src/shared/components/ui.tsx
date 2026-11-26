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
import { Button as UiButton } from '@/components/ui/button';
import { Card as UiCard } from '@/components/ui/card';
import { Checkbox as UiCheckbox } from '@/components/ui/checkbox';
import { Input } from '@/components/ui/input';
import { Text as UiText } from '@/components/ui/text';
import { useTheme } from '../theme/theme';
import { FieldLabel } from './field-label';

type IconName = keyof typeof Ionicons.glyphMap;

cssInterop(SafeAreaView, { className: 'style' });

export function Screen({
  children,
  bottomInset = false,
}: PropsWithChildren<{ bottomInset?: boolean }>) {
  return (
    <SafeAreaView
      className="flex-1 bg-background"
      edges={bottomInset ? ['top', 'bottom', 'left', 'right'] : ['top', 'left', 'right']}
    >
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
    <UiText
      variant={title ? 'h1' : muted ? 'muted' : 'default'}
      className={cn(title && muted && 'text-muted-foreground', className)}
    >
      {children}
    </UiText>
  );
}

export function Card({ children }: PropsWithChildren) {
  return <UiCard>{children}</UiCard>;
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
  const textColor = secondary ? colors.text : colors.onPrimary;
  return (
    <UiButton
      accessibilityLabel={label}
      disabled={disabled}
      loading={loading}
      onPress={onPress}
      variant={secondary ? 'outline' : 'default'}
      shape={pill ? 'pill' : 'default'}
    >
      {leading}
      <UiText>{label}</UiText>
      {trailingIcon ? <Ionicons name={trailingIcon} size={18} color={textColor} /> : null}
    </UiButton>
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
  containerClassName,
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
  /** Class thêm vào khung ô, ghi đè kích thước mặc định (ví dụ `h-11 gap-2 px-3` cho ô gọn trong form). */
  containerClassName?: string;
}) {
  const { colors } = useTheme();
  const [visible, setVisible] = useState(false);
  const toggleLabel = `${visible ? 'Ẩn' : 'Hiện'} ${label.toLowerCase()}`;
  return (
    <View className="gap-2">
      {hideLabel ? null : <FieldLabel label={label} />}
      <View
        className={cn(
          'flex-row gap-3 rounded-xl border bg-card px-4',
          // Ô nhiều dòng (giới thiệu bản thân...) cao hơn và chữ bắt đầu từ trên; ô một dòng cố định 52px, chữ căn giữa.
          props.multiline ? 'min-h-[132px] items-start py-3' : 'h-[52px] items-center',
          containerClassName,
          error ? 'border-destructive' : 'border-input',
        )}
      >
        {icon ? <Ionicons name={icon} size={20} color={colors.muted} /> : null}
        <Input
          {...props}
          ref={inputRef}
          placeholder={props.placeholder ?? (hideLabel ? label : undefined)}
          secureTextEntry={passwordToggle ? !visible : props.secureTextEntry}
          accessibilityLabel={label}
          placeholderTextColor={colors.muted}
          // Khung viền, nền, chiều cao do View bọc ngoài lo (để chứa biểu tượng), nên bỏ những thứ đó của Input.
          className="h-auto flex-1 self-stretch rounded-none border-0 bg-transparent px-0 py-0"
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
  return (
    <View className="gap-2">
      <View className="flex-row items-start gap-3">
        <UiCheckbox
          accessibilityLabel={label}
          checked={checked}
          onCheckedChange={onChange}
          invalid={Boolean(error)}
          className="mt-0.5"
        />
        <UiText className="flex-1">{children}</UiText>
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
