import { useTranslation } from 'react-i18next';

export default function LanguageSwitcher() {
  const { i18n } = useTranslation();

  const toggleLanguage = () => {
    const newLang = i18n.language === 'en' ? 'bg' : 'en';
    i18n.changeLanguage(newLang);
    localStorage.setItem('language', newLang);
  };

  return (
    <button
      onClick={toggleLanguage}
      className="px-3 py-1.5 text-sm font-medium rounded-lg bg-lavender/30 text-primary-dark dark:text-[#bdb9bc] hover:bg-lavender/50 transition-colors"
    >
      {i18n.language === 'en' ? 'BG' : 'EN'}
    </button>
  );
}
