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
  
- **Localization Service**: `ILocalizationService` provides runtime locale switching
  - Automatically detects system locale on startup
  - Syncs locale changes to browser components (WebAssembly target)
  
- **XAML Binding**: Use the `{l10n:Localize Key=ResourceKey}` markup extension

### TypeScript/Lit Layer (Browser Components)

Browser components use `@lit/localize` for runtime localization:

- **Configuration**: `lit-localize.json` in `/src/AvaloniaXKCD.Site/`
- **Locale Files**: Located in `/src/AvaloniaXKCD.Site/src/localization/locales/`
  - `en.ts` - English
  - `es.ts` - Spanish
- **Usage**: Wrap strings with `msg("text to translate")`
- **Updates**: Components using `updateWhenLocaleChanges(this)` re-render on locale change

### Synchronization

The C# and TypeScript layers stay synchronized via JavaScript interop:

- Browser locale detection: `getLocale()` in `interop.ts` reads `navigator.language`
- C# to Browser: When C# changes locale, it calls `setLocale()` to update Lit components
- Desktop: Detects OS locale using platform APIs (Windows/Linux)

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

1. **Wrap strings with `msg()`** in your component:
```typescript
import { msg } from '@lit/localize';

const myText = msg("Text to translate");
```

2. **Extract messages** (generates XLIFF files for translation):
```bash
cd src/AvaloniaXKCD.Site
npm run lit-localize extract
```

3. **Translate** the generated XLIFF files in `/xliff/` directory

4. **Build locales**:
```bash
npm run lit-localize build
```

5. **Update component** to respond to locale changes:
```typescript
import { updateWhenLocaleChanges } from '@lit/localize';

constructor() {
  super();
  updateWhenLocaleChanges(this);
}
```

### Adding a New Language

#### C# Resources

1. Create a new resource file: `Strings.{locale}.resx`
   - Example: `Strings.fr.resx` for French
2. Copy all entries from `Strings.resx`
3. Translate the `<value>` elements
4. Update `LocalizationService.cs` if needed to add the new locale to supported cultures

#### TypeScript/Lit

1. **Update configuration** (`lit-localize.json`):
```json
{
  "sourceLocale": "en",
  "targetLocales": ["es", "fr"],
  ...
}
```

2. **Extract and translate**:
```bash
npm run lit-localize extract
# Translate the XLIFF files
npm run lit-localize build
```

3. **Update locale codes** in `/src/localization/locale-codes.ts` if needed

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

### C# Localized Elements

- **Navigation Buttons**: First, Previous, Random, Explain, Go To, Next, Last
- **Dialogs**: Error, Quit, Continue, Cancel
- **Context Menu**: Copy URL
- **Window Title**: "Axkcd" and dynamic title format

### TypeScript Localized Elements

- **Loading Messages**: All 34 humorous loading messages in `LoadingIndicator` component

## Troubleshooting

### C# Strings Not Updating

- Ensure `.resx` files are set as `EmbeddedResource` in the project file
- Check that the resource file namespace matches: `AvaloniaXKCD.Resources.Strings`
- Rebuild the project to regenerate resource files

### Lit Components Not Updating

- Ensure `updateWhenLocaleChanges(this)` is called in constructor
- Verify `msg()` is wrapping all translatable strings
- Check that `lit-localize build` was run after translations

### Locale Not Detected

- **Browser**: Check `navigator.language` in console
- **Desktop**: Verify system locale settings
- Ensure the detected locale is in the supported locales list

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
