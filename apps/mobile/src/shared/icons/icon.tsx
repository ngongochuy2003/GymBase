import { View } from 'react-native';
import { theme } from '../theme/tokens';
import { getIcon } from './icon-registry';

export interface IconProps {
  name: string;
  size?: number;
  color?: string;
}

/** Hiển thị icon theo tên đã đăng ký; tên lạ → vòng tròn viền để dễ phát hiện. */
export function Icon({ name, size = theme.iconSize.md, color = theme.colors.text }: IconProps) {
  const found = getIcon(name);
  if (!found) {
    return (
      <View
        testID={`icon-placeholder-${name}`}
        style={{
          width: size,
          height: size,
          borderRadius: size / 2,
          borderWidth: 1.5,
          borderColor: color,
        }}
      />
    );
  }
  const Glyph = found.family;
  return <Glyph name={found.glyph} size={size} color={color} />;
}
