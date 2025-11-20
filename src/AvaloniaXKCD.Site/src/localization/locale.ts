import { configureLocalization, msg as litMsg, type LocaleModule } from '@lit/localize';
import { sourceLocale, targetLocales } from './locale-codes';
import { getLocalizedString } from './localized-string';
// Import the custom element to ensure it's registered
import './localized-string';

export const msg = litMsg;

const localizedTemplates = new Map(
  targetLocales.map((locale) => [locale, import(`./locales/${locale}.ts`)])
);

export const { getLocale, setLocale } = configureLocalization({
  sourceLocale,
  targetLocales,
  loadLocale: async (locale) => {
    const modulePromise = localizedTemplates.get(locale as (typeof targetLocales)[number]);
    if (!modulePromise) {
      throw new Error(`No locale module found for: ${locale}`);
    }
    return (await modulePromise) as Promise<LocaleModule>;
  },
});

const navigatorLang = navigator.languages?.[0] ?? navigator.language ?? sourceLocale;
const navigatorBaseLang = navigatorLang.split('-')[0];
const matchedLocale = targetLocales.find(
  (l) => l === navigatorLang || l.split('-')[0] === navigatorBaseLang
);
if (matchedLocale) {
  void setLocale(matchedLocale);
}

export const addLocaleChangeListener = (callback: () => void) => {
  window.addEventListener('lit-localize-status', (ev) => {
    if (ev?.detail?.status === 'ready') {
      callback();
    }
  });
};

export const getString = (key: string): string => {
  return getLocalizedString(key);
};
