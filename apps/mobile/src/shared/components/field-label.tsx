import { Text } from 'react-native';
import { cn } from '@/lib/utils';

const REQUIRED_SUFFIX = ' *';

/**
 * Nhãn của ô nhập. Nhãn kết thúc bằng " *" (ô bắt buộc) thì dấu `*` được tô đỏ, phần chữ giữ màu thường. Chỉ đổi cách
 * hiển thị: nhãn truy cập của ô vẫn là chuỗi đầy đủ như người gọi truyền vào.
 */
export function FieldLabel({ label, className }: { label: string; className?: string }) {
  const required = label.endsWith(REQUIRED_SUFFIX);
  const text = required ? label.slice(0, -REQUIRED_SUFFIX.length) : label;
  return (
    <Text className={cn('text-base leading-6 text-foreground', className)}>
      {text}
      {required ? <Text className="text-destructive">{REQUIRED_SUFFIX}</Text> : null}
    </Text>
  );
}
