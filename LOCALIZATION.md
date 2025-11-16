# Localization Guide

AvaloniaXKCD supports internationalization (i18n) and localization (l10n) across both the C# Avalonia UI and TypeScript/Lit browser components.

## Supported Languages

- **English (en)** - Default/source language
- **Spanish (es)** - Español

## Architecture

### C# Layer (Avalonia UI)

The C# application uses .NET `.resx` resource files for localization:

- **Resource Files**: Located in `/src/AvaloniaXKCD/Resources/`
  - `Strings.resx` - English (default)
  - `Strings.es.resx` - Spanish
  - Resources are embedded in the `AvaloniaXKCD.Core` assembly
  
- **Localization Service**: `ILocalizationService` provides runtime locale switching
  - Base abstract class: `LocalizationService` in `AvaloniaXKCD.Exports`
  - Desktop implementation: Uses OS locale detection (Windows/Linux)
  - Browser implementation: Uses JavaScript interop to get browser locale and retrieve localized strings
  - Automatically detects system locale on startup with fallback to English
  - Syncs locale changes bidirectionally between C# and TypeScript
  
- **XAML Binding**: Use the `{l10n:Localize Key=ResourceKey}` markup extension

### TypeScript/Lit Layer (Browser Components)

Browser components use `@lit/localize` for runtime localization with additional DOM translation support:

- **Configuration**: `lit-localize.json` in `/src/AvaloniaXKCD.Site/`
- **Locale Files**: Located in `/src/AvaloniaXKCD.Site/src/localization/locales/`
  - `en.ts` - Empty (source locale)
  - `es.ts` - Spanish translations
- **XLIFF Translation Files**: Located in `/src/AvaloniaXKCD.Site/xliff/`
  - `es.xlf` - Spanish translation catalog
- **Localization Infrastructure**:
  - `locale.ts` - Configures Lit localization and auto-detects browser locale
  - `localized-string.ts` - Custom element for managing localized strings with static `getString()` method
  - `dom-translator.ts` - Translates static HTML content using `data-i18n` attributes
  - `locale-codes.ts` - Defines source and target locales
- **Usage**: 
  - Lit components: Use `msg()` with IDs: `msg('Text', { id: 'Key_Name' })`
  - Static HTML: Add `data-i18n="Key_Name"` attribute
  - Components with `@localized()` decorator automatically re-render on locale change
- **Integration**: `main.ts` initializes DOM translation on page load and locale changes

### Synchronization

The C# and TypeScript layers stay synchronized via JavaScript interop:

- **Browser to C#**: `getLocale()` in `interop.ts` returns current browser locale
- **Browser string retrieval**: `getString(key)` in `interop.ts` returns localized string from TypeScript layer
- **C# to Browser**: `setLocale(locale)` updates Lit components when C# changes locale
- **Browser implementation**: `BrowserLocalizationService` uses interop to retrieve strings from TypeScript instead of C# ResourceManager
- **Desktop**: Detects OS locale using platform APIs (Windows/Linux) and uses .resx files directly
- **Locale detection**: Browser version automatically detects from `navigator.languages` or `navigator.language` on load

## For Developers

### Adding a New Localized String (C#)

1. **Add to English resource file** (`Strings.resx`):
```xml
<data name="YourKey_Name" xml:space="preserve">
  <value>Your English text</value>
  <comment>Description of where this is used</comment>
</data>
```

2. **Add translations** to each target locale file (e.g., `Strings.es.resx`):
```xml
<data name="YourKey_Name" xml:space="preserve">
  <value>Tu texto en español</value>
  <comment>Description of where this is used</comment>
</data>
```

3. **Use in XAML**:
```xml
<Button Content="{l10n:Localize Key=YourKey_Name}" />
```

4. **Use in C# code**:
```csharp
var localization = ExportContainer.Get<ILocalizationService>();
var text = localization.GetString("YourKey_Name");
```

### Adding a New Localized String (TypeScript/Lit)

#### For Lit Components

1. **Add to localized-string.ts** with a unique ID:
```typescript
private static strings: Record<string, () => string> = {
  'YourKey_Name': () => msg('Your English text', { id: 'YourKey_Name' }),
  // ... other strings
};
```

2. **Extract messages** (generates XLIFF files for translation):
```bash
cd src/AvaloniaXKCD.Site
npm run lit-localize:extract
```

3. **Translate** the generated XLIFF files in `/xliff/` directory (e.g., `es.xlf`)

4. **Build locales**:
```bash
npm run lit-localize:build
```

5. **Use in components**:
```typescript
import { getLocalizedString } from './localization/localized-string';

const text = getLocalizedString('YourKey_Name');
```

6. **Decorate component** to respond to locale changes:
```typescript
import { localized } from '@lit/localize';

@customElement('my-component')
@localized()
class MyComponent extends LitElement {
  // Component automatically re-renders on locale change
}
```

#### For Static HTML Content

1. **Add the string key** to `localized-string.ts` as above

2. **Add data-i18n attribute** to HTML elements:
```html
<p data-i18n="YourKey_Name"></p>
```

3. **For title or aria-label attributes**:
```html
<button data-i18n-title="Button_Tooltip">Click me</button>
```

4. **Extract and build** as described above

The `dom-translator.ts` will automatically translate these elements on page load and locale changes.

### Adding a New Language

#### C# Resources

1. Create a new resource file: `Strings.{locale}.resx`
   - Example: `Strings.fr.resx` for French
2. Copy all entries from `Strings.resx`
3. Translate the `<value>` elements
4. Update `LocalizationService.cs` to add the new locale to `supportedCultures` array:
```csharp
public static string[] supportedCultures = new[] { "en", "es", "fr" };
```

#### TypeScript/Lit

1. **Update locale codes** in `/src/AvaloniaXKCD.Site/src/localization/locale-codes.ts`:
```typescript
export const sourceLocale = 'en';
export const targetLocales = ['es', 'fr'] as const;
```

2. **Extract messages** to generate XLIFF file:
```bash
cd src/AvaloniaXKCD.Site
npm run lit-localize:extract
```

3. **Translate** the new XLIFF file (e.g., `xliff/fr.xlf`)

4. **Build locales**:
```bash
npm run lit-localize:build
```

5. **Update locale map** in `locale.ts` if needed (should auto-detect from `targetLocales`)

## For Translators

### Resource File Format (.resx)

Resource files are XML-based. Each translatable string has:
- **name**: The unique key (don't change this)
- **value**: The translated text (translate this)
- **comment**: Context/description (helps with translation)

Only translate the content inside `<value>` tags.

### Example Translation Workflow

1. Get the `.resx` file for your language
2. Open in a text editor or resx editor (e.g., ResXManager)
3. For each `<data>` entry, translate the `<value>` content
4. Preserve any placeholders like `{0}`, `{1}` in the same positions
5. Test by running the application with your locale

### XLIFF Format (Lit Components)

XLIFF files are in `/xliff/` directory. Each message has:
- **source**: Original English text (reference only)
- **target**: Your translation (edit this)

## Testing Localization

### Desktop Application

The application automatically detects your system locale:

- **Windows**: Uses Windows display language
- **Linux**: Uses `LANG` environment variable
- **Override**: Set via code or settings (no UI currently)

### Browser Application

The browser version detects locale from `navigator.language`:

1. Change your browser language settings
2. Reload the application
3. Verify the UI updates to the correct language

### Manual Locale Switching (Developer)

For testing, you can force a locale in code:

```csharp
var localization = ExportContainer.Get<ILocalizationService>();
localization.SetCulture("es"); // Switch to Spanish
```

## Current UI Coverage

### C# Localized Elements (Desktop and Browser)

- **Navigation Buttons**: First, Previous, Random, Explain, Go To, Next, Last
- **Dialogs**: Error (title and message), Quit, Continue, Cancel
- **Dialog Tooltips**: Error Title, Error Message
- **Context Menu**: Copy URL
- **Window Title**: "Axkcd" and dynamic title format "AXKCD: {0}"

### TypeScript Localized Elements (Browser Only)

- **Loading Messages**: All 34 humorous loading messages in `LoadingIndicator` component
- **Static HTML Content**:
  - Page title: "A(valonia)XKCD"
  - Page subtitle
  - Development banner text and tooltip
  - Footer text and technology tooltips

### Implementation Details

- **Desktop**: Uses .resx files directly via ResourceManager
- **Browser**: 
  - C# UI: Uses JavaScript interop to call TypeScript `getString()` function
  - Static HTML: Uses `data-i18n` attributes translated via `dom-translator.ts`
  - Dynamic components: Uses Lit's `@localized()` decorator for automatic updates

## Troubleshooting

### C# Strings Not Updating

- Ensure `.resx` files are set as `EmbeddedResource` in the project file with `LogicalName` set correctly
- Check that the resource file namespace matches: `AvaloniaXKCD.Resources.Strings`
- Verify resources are embedded in the correct assembly (`AvaloniaXKCD.Core`)
- Rebuild the project to regenerate resource files
- Check `LocalizationService` is loading resources from the correct assembly

### Browser Strings Not Loading

- **Desktop implementation**: Uses ResourceManager directly from .resx files
- **Browser implementation**: Uses JavaScript interop to retrieve strings from TypeScript layer
- Verify `BrowserLocalizationService` correctly overrides `GetString()` to call `GetBrowserString()`
- Check that `interop.ts` exports `getString(key)` function
- Ensure `localized-string.ts` contains the key you're looking for
- Check browser console for any JavaScript errors

### Lit Components Not Updating

- Ensure components use `@localized()` decorator (not `updateWhenLocaleChanges()`)
- Verify `msg()` calls include an `id` parameter: `msg('Text', { id: 'Key_Name' })`
- Check that `lit-localize:build` was run after translations
- Verify XLIFF files have `<target>` elements filled in
- Check that locale change events are firing (listen to `lit-localize-status` event)

### Static HTML Not Translating

- Ensure `initializeDOMTranslation()` is called in `main.ts`
- Verify elements have correct `data-i18n` or `data-i18n-title` attributes
- Check that the key exists in `localized-string.ts`
- Verify `dom-translator.ts` is imported and running

### Locale Not Detected

- **Browser**: Check `navigator.language` or `navigator.languages` in console
- **Desktop**: Verify system locale settings
- Ensure the detected locale (or its two-letter code) is in the `supportedCultures` array
- Check browser console for locale detection logs
- Verify fallback to English is working if locale is not supported

### Assembly Loading Issues

- If seeing "Could not load assembly" errors, verify the assembly name is correct
- For browser builds, ensure trimming is not removing required resources
- Check that `<EmbeddedResource>` items are properly configured in `.csproj` files

## Contributing Translations

We welcome translations! To contribute:

1. Fork the repository
2. Add or update resource files for your language
3. Test thoroughly in both Desktop and Browser targets
4. Submit a pull request with:
   - Updated `.resx` files
   - Updated XLIFF files (if browser components changed)
   - Screenshots showing the translated UI

## Resources

- [Avalonia Localization Guide](https://docs.avaloniaui.net/docs/guides/implementation-guides/localizing)
- [Lit Localization Guide](https://lit.dev/docs/localization/overview/)
- [.NET Globalization](https://learn.microsoft.com/en-us/dotnet/core/extensions/globalization)
