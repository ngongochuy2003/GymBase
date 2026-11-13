import { createContext, useContext, type PropsWithChildren } from 'react';
import { useColorScheme } from 'nativewind';
import { palette, type Colors } from './palette';

export const spacing = { xs: 4, sm: 8, md: 16, lg: 24, xl: 32 } as const;
type Theme = { colors: Colors; isDark: boolean; toggle(): void };
const Context = createContext<Theme | null>(null);

/**
 * Chế độ sáng/tối do NativeWind giữ (mặc định theo hệ thống, `toggle` đổi tay). `className` dùng
 * token trong `global.css`; `colors` chỉ dành cho prop không nhận class.
 */
export function ThemeProvider({ children }: PropsWithChildren) {
  const { colorScheme, toggleColorScheme } = useColorScheme();
  const isDark = colorScheme === 'dark';
  return (
    <Context.Provider
      value={{ colors: isDark ? palette.dark : palette.light, isDark, toggle: toggleColorScheme }}
    >
      {children}
    </Context.Provider>
  );
}

export function useTheme() {
  const theme = useContext(Context);
  if (!theme) throw new Error('useTheme requires ThemeProvider');
  return theme;
}
