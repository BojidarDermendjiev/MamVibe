import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';

export type Theme = 'light' | 'dark';

const STORAGE_KEY = 'mamvibe_theme';

export const DARK = {
  bg:           '#1a1825',
  card:         '#2d2a42',
  section:      '#201d30',
  border:       'rgba(255,255,255,0.08)',
  text:         '#ffffff',
  text2:        '#aaaaaa',
  text3:        'rgba(255,255,255,0.45)',
  input:        '#2d2a42',
  inputBorder:  'rgba(255,255,255,0.14)',
  tabBar:       '#1a1825',
  tabBarBorder: 'rgba(255,255,255,0.08)',
  header:       '#1a1825',
};

export const LIGHT = {
  bg:           '#fafafa',
  card:         '#ffffff',
  section:      '#FAF3EE',
  border:       '#f0f0f0',
  text:         '#1a1a1a',
  text2:        '#888888',
  text3:        '#aaaaaa',
  input:        '#ffffff',
  inputBorder:  '#e8e8e8',
  tabBar:       '#ffffff',
  tabBarBorder: '#f0f0f0',
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
