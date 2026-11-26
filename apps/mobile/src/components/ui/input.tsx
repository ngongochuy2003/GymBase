import { cn } from '@/lib/utils';
import type { ComponentRef } from 'react';
import { TextInput } from 'react-native';

/** Ô nhập một dòng cao 52px, viền `input`, bo 12. Ô nhiều dòng truyền thêm `multiline` và class chiều cao. */
function Input({
  className,
  style,
  ...props
}: React.ComponentProps<typeof TextInput> & React.RefAttributes<ComponentRef<typeof TextInput>>) {
  return (
    <TextInput
      className={cn(
        'h-[52px] w-full min-w-0 rounded-xl border border-input bg-card px-4 text-base text-foreground',
        props.editable === false && 'opacity-50',
        className,
      )}
      // iOS: lineHeight (do `text-base` thêm vào) làm chữ một dòng lệch khỏi giữa ô nên bỏ đi.
      style={[style, { lineHeight: undefined }]}
      {...props}
    />
  );
}

export { Input };
