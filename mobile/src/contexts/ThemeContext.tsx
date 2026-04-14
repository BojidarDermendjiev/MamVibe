import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';

export type Theme = 'light' | 'dark';

const STORAGE_KEY = 'mamvibe_theme';

export const DARK = {
  bg:           '#2a1c1b',
  card:         '#3a2624',
  section:      '#321e1d',
  border:       'rgba(212,147,143,0.15)',
  text:         '#ffffff',
  text2:        '#8eaa89',
  text3:        'rgba(142,170,137,0.55)',
  input:        '#3a2624',
  inputBorder:  'rgba(212,147,143,0.25)',
  tabBar:       '#2a1c1b',
  tabBarBorder: 'rgba(212,147,143,0.15)',
  header:       '#2a1c1b',
};

export const LIGHT = {
  bg:           '#fefcf9',
  card:         '#ffffff',
  section:      '#f5ede5',
  border:       '#e8d8cc',
  text:         '#1a1a1a',
  text2:        '#8eaa89',
  text3:        'rgba(142,170,137,0.6)',
  input:        '#ffffff',
  inputBorder:  '#e8d8cc',
  tabBar:       '#ffffff',
  tabBarBorder: '#e8d8cc',
  header:       '#ffffff',
};

export type Colors = typeof DARK;

interface ThemeContextValue {
  theme: Theme;
  isDark: boolean;
  colors: Colors;
  toggleTheme: () => void;
}

const ThemeContext = createContext<ThemeContextValue>({
  theme: 'dark',
  isDark: true,
  colors: DARK,
  toggleTheme: () => {},
});

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setTheme] = useState<Theme>('dark');

  useEffect(() => {
    AsyncStorage.getItem(STORAGE_KEY).then((v) => {
      if (v === 'light' || v === 'dark') setTheme(v);
    });
  }, []);

  const toggleTheme = () => {
    const next: Theme = theme === 'dark' ? 'light' : 'dark';
    setTheme(next);
    AsyncStorage.setItem(STORAGE_KEY, next);
  };

  return (
    <ThemeContext.Provider value={{ theme, isDark: theme === 'dark', colors: theme === 'dark' ? DARK : LIGHT, toggleTheme }}>
      {children}
    </ThemeContext.Provider>
  );
}

export function useTheme() {
  return useContext(ThemeContext);
}
