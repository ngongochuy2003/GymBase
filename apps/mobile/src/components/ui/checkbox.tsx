import { cn } from '@/lib/utils';
import { useTheme } from '@/shared/theme/theme';
import { Ionicons } from '@expo/vector-icons';
import * as CheckboxPrimitive from '@rn-primitives/checkbox';

const DEFAULT_HIT_SLOP = 8;

function Checkbox({
  className,
  invalid,
  ...props
}: React.ComponentProps<typeof CheckboxPrimitive.Root> & { invalid?: boolean }) {
  const { colors } = useTheme();
  return (
    <CheckboxPrimitive.Root
      className={cn(
        'size-6 shrink-0 overflow-hidden rounded-md border',
        invalid ? 'border-destructive' : 'border-input',
        props.checked && 'border-primary',
        props.disabled && 'opacity-50',
        className,
      )}
      hitSlop={DEFAULT_HIT_SLOP}
      {...props}
    >
      <CheckboxPrimitive.Indicator className="h-full w-full items-center justify-center bg-primary">
        <Ionicons name="checkmark" size={16} color={colors.onPrimary} />
      </CheckboxPrimitive.Indicator>
    </CheckboxPrimitive.Root>
  );
}

export { Checkbox };
