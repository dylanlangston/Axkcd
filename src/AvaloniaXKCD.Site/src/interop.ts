/*
This file is imported by the Avalonia App to enable interop between the C# and JS apps.
All exported objects and functions will be mapped the the "interop" module.
*/

import {
  getLocale as getLitLocale,
  setLocale as setLitLocale,
  getString as getLitString,
  addLocaleChangeListener,
} from './localization/locale';

export function setTitle(title: string) {
  document.title = title;
}

export function getLocale(): string {
  return getLitLocale();
}

export async function setLocale(locale: string) {
  return await setLitLocale(locale);
}

export function addLocaleChangeHandler(cb: () => void) {
  addLocaleChangeListener(cb);
}

export function getString(key: string): string {
  return getLitString(key);
}
