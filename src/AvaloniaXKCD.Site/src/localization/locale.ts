import { configureLocalization } from '@lit/localize';
import { sourceLocale, targetLocales } from './locale-codes';

export const { getLocale, setLocale } = configureLocalization({
  sourceLocale,
  targetLocales,
  loadLocale: (locale: string) => import(`./locales/${locale}.ts`),
});

// Initialize with browser locale if available
const browserLang = navigator.language.split('-')[0];
if (targetLocales.includes(browserLang)) {
  setLocale(browserLang);
}
