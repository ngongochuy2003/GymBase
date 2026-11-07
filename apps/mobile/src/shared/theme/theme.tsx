import { createContext, useContext, useState, type PropsWithChildren } from 'react';
import { useColorScheme } from 'react-native';

const light = {
  background: '#F3F5EF',
  surface: '#FFFFFF',
  text: '#192820',
  muted: '#66746B',
  border: '#DDE4DB',
  primary: '#286544',
  onPrimary: '#FFFFFF',
  accent: '#D9F28B',
  hero: '#183C2C',
  heroText: '#F5F9EF',
  error: '#AE3441',
};
const dark: typeof light = {
  background: '#101B15',
  surface: '#1B2B21',
  text: '#F0F6ED',
  muted: '#A8B8AC',
  border: '#344B3C',
  primary: '#A9DA85',
  onPrimary: '#183C2C',
  accent: '#D9F28B',
  hero: '#234D36',
  heroText: '#F5F9EF',
  error: '#FF9EA9',
};
export const spacing = { xs: 4, sm: 8, md: 16, lg: 24, xl: 32 } as const;
type Theme = { colors: typeof light; isDark: boolean; toggle(): void };
const Context = createContext<Theme | null>(null);

export function ThemeProvider({ children }: PropsWithChildren) {
  const system = useColorScheme();
  const [override, setOverride] = useState<boolean | null>(null);
  const isDark = override ?? system === 'dark';
  return (
    <Context.Provider
      value={{ colors: isDark ? dark : light, isDark, toggle: () => setOverride(!isDark) }}
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
