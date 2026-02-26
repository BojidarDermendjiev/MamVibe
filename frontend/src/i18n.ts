import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import en from './locales/en.json';
import bg from './locales/bg.json';

i18n.use(initReactI18next).init({
  resources: {
    en: { translation: en },
    bg: { translation: bg },
  },
  lng: localStorage.getItem('language') || 'en',
  fallbackLng: 'en',
  // escapeValue: false is correct here — React's JSX already escapes all
  // interpolated values, so double-escaping would corrupt special characters.
  // Never interpolate raw user-supplied strings into translation keys.
  interpolation: { escapeValue: false },
});

export default i18n;
