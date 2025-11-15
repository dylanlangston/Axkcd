/*
This file is imported by the Avalonia App to enable interop between the C# and JS apps.
All exported objects and functions will be mapped the the "interop" module.
*/

import { setLocale as setLitLocale } from './localization/locale';

export function setTitle(title: string)
{
    document.title = title;
}

export function getLocale(): string
{
    // Get the browser's preferred language
    // Returns language code like "en", "es", "es-MX", etc.
    return navigator.language;
}

export function setLocale(locale: string)
{
    // Update Lit components' locale
    setLitLocale(locale);
}
