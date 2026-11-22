import { TextClassContext } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { useTheme } from '@/shared/theme/theme';
import { cva, type VariantProps } from 'class-variance-authority';
import { ActivityIndicator, Pressable } from 'react-native';

// Giao diện GymBase: nút cao tối thiểu 50px, có viền, bo 14 (hoặc `pill` bo tròn hẳn).
const buttonVariants = cva(
  'group shrink-0 flex-row items-center justify-center gap-2.5 border shadow-none',
  {
    variants: {
      variant: {
        default: 'border-primary bg-primary active:opacity-75',
        destructive: 'border-destructive bg-destructive active:opacity-75',
        outline: 'border-input bg-card active:opacity-75',
        secondary: 'border-transparent bg-secondary active:opacity-75',
        ghost: 'border-transparent active:bg-accent',
        link: 'border-transparent',
      },
      size: {
        default: 'min-h-[50px] px-5 py-3.5',
        sm: 'min-h-10 px-3 py-2',
        icon: 'size-10',
      },
      shape: {
        default: 'rounded-[14px]',
        pill: 'rounded-full',
      },
    },
    defaultVariants: { variant: 'default', size: 'default', shape: 'default' },
  },
);

const buttonTextVariants = cva('text-base font-bold text-foreground', {
  variants: {
    variant: {
      default: 'text-primary-foreground',
      destructive: 'text-destructive-foreground',
      outline: '',
      secondary: 'text-secondary-foreground',
      ghost: '',
      link: 'text-primary group-active:underline',
    },
  },
  defaultVariants: { variant: 'default' },
});

type ButtonProps = React.ComponentProps<typeof Pressable> &
  React.RefAttributes<typeof Pressable> &
  VariantProps<typeof buttonVariants> & {
    /** Đang xử lý: hiện vòng xoay thay nội dung và khóa nút. */
    loading?: boolean;
  };

function Button({
  className,
  variant,
  size,
  shape,
  loading = false,
  disabled,
  children,
  ...props
}: ButtonProps) {
  const { colors } = useTheme();
  const inactive = Boolean(disabled) || loading;
  return (
    <TextClassContext.Provider value={buttonTextVariants({ variant })}>
      <Pressable
        accessibilityRole="button"
        accessibilityState={{ disabled: inactive, busy: loading }}
        disabled={inactive}
        className={cn(
          inactive && 'opacity-50',
          buttonVariants({ variant, size, shape }),
          className,
        )}
        {...props}
      >
        {loading ? (
          <ActivityIndicator
            color={
              variant === 'outline' || variant === 'secondary' ? colors.primary : colors.onPrimary
            }
          />
        ) : (
          (children as React.ReactNode)
        )}
      </Pressable>
    </TextClassContext.Provider>
  );
}

export { Button, buttonTextVariants, buttonVariants };
export type { ButtonProps };
