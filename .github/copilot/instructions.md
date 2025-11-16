# GitHub Copilot Instructions for AvaloniaXKCD

## Project Context

AvaloniaXKCD is a cross-platform XKCD viewer built with Avalonia UI, supporting both native desktop (Windows/Linux) and browser (WebAssembly) targets. The codebase uses:

- **.NET 10** with C# for application logic
- **Avalonia UI 12.x** for cross-platform UI
- **TypeScript/Lit** for browser frontend components
- **MVVM pattern** with CommunityToolkit.Mvvm
- **Export/Import pattern** for cross-platform abstractions
- **Verify** for snapshot-style testing

## Architecture Patterns

### Export System

The project uses a custom Export/Import system (`AvaloniaXKCD.Exports`) for dependency injection:

- **Interfaces**: Define contracts in `AvaloniaXKCD.Exports` (must implement `IExport`)
- **Abstract base classes**: Can live in `AvaloniaXKCD/Exports/` namespace
- **Platform implementations**: In `AvaloniaXKCD.Desktop/Exports/` or `AvaloniaXKCD.Browser/Exports/`
- **Registration**: Automatic via source generators
- **Retrieval**: `ExportContainer.Get<IInterface>()`
- **Multi-registration**: Use `AddMulti<T>()` and `GetAll<T>()` for multiple implementations

### Localization Architecture

**Key Insight**: Browser and Desktop have different localization approaches:

#### Desktop Implementation
- Uses .NET `ResourceManager` to load `.resx` files directly
- Resources embedded in `AvaloniaXKCD.Core` assembly
- `LocalizationService` is abstract base class with virtual `GetString()`
- Desktop implementation inherits and uses base `GetString()` method

#### Browser Implementation  
- **Does NOT use .resx files** - uses JavaScript interop instead
- `BrowserLocalizationService` overrides `GetString()` to call `GetBrowserString()`
- `GetBrowserString()` uses JSImport to call TypeScript `getString(key)`
- TypeScript layer (`localized-string.ts`) manages all browser strings
- This avoids resource loading issues in WebAssembly environment

#### TypeScript/Lit Localization
- `@lit/localize` with runtime mode for dynamic locale switching
- `localized-string.ts`: Custom element with static `getString()` for interop
- `dom-translator.ts`: Translates static HTML with `data-i18n` attributes
- `locale.ts`: Configures Lit localization and auto-detects browser locale
- `@localized()` decorator for automatic component re-rendering
- XLIFF files in `/xliff/` for translations

### Project Structure

```
src/
├── AvaloniaXKCD/              # Core UI (MVVM, Views, ViewModels)
│   ├── Exports/               # Abstract base classes for platform features
│   └── Resources/             # .resx localization files
├── AvaloniaXKCD.Desktop/      # Desktop platform implementation
│   └── Exports/               # Desktop-specific exports
├── AvaloniaXKCD.Browser/      # Browser/WASM platform implementation
│   └── Exports/               # Browser-specific exports (uses JSImport/JSExport)
├── AvaloniaXKCD.Exports/      # Cross-platform interfaces
├── AvaloniaXKCD.Site/         # Browser frontend (TypeScript/Lit/Vite)
│   └── src/
│       ├── components/        # Lit web components
│       ├── localization/      # Lit localization infrastructure
│       └── interop.ts         # C# ↔ TypeScript bridge
├── AvaloniaXKCD.Tests/        # Test suite with Verify snapshots
└── XKCDCore/                  # XKCD API client library
```

## Coding Guidelines

### General Principles

1. **Minimal Changes**: Make the smallest possible changes to achieve the goal
2. **Existing Patterns**: Follow established patterns in the codebase
3. **No Breaking Changes**: Unless explicitly requested or fixing bugs
4. **Test Early**: Run tests before and after changes to catch regressions
5. **Assembly References**: Be careful with assembly loading - verify the correct assembly contains the resources

### Localization Implementation

When working with localization:

1. **Desktop vs Browser**: Remember they use different string retrieval mechanisms
2. **Resource Assembly**: .resx files are in `AvaloniaXKCD.Core`, not `AvaloniaXKCD.Exports`
3. **Browser Strings**: Browser gets strings from TypeScript, not .resx files
4. **Interop Required**: Browser needs JSImport/JSExport for locale and string operations
5. **IDs Required**: Lit `msg()` calls must include `id` parameter for extraction
6. **Static HTML**: Use `data-i18n` attributes and `dom-translator.ts` for non-component content
7. **Fallback**: Always implement fallback to English for unsupported locales

### Testing Standards

1. **Snapshot Testing**: Use Verify for regression protection
2. **Test Methods**: Use descriptive names like `ShouldDetectBrowserLocale`
3. **Assertions in Snapshots**: Nest assertions within `Verify()` calls
4. **Browser Tests**: Use canvas screenshots for Avalonia browser tests (DOM querying doesn't work)
5. **Parallel Execution**: Browser tests have parallel limits to avoid resource contention

### TypeScript/Lit Guidelines

1. **Component Decorators**: Use `@customElement()` and `@localized()` properly
2. **Property Decorators**: Use `@property()` for reactive properties
3. **Localization**: Always include IDs: `msg('Text', { id: 'Key_Name' })`
4. **Static Registration**: Register strings in `localized-string.ts` for interop
5. **DOM Translation**: Initialize via `initializeDOMTranslation()` in `main.ts`

### Common Mistakes to Avoid

1. **Assembly Loading**: Don't assume `typeof(Class).Assembly` has resources - verify!
2. **Browser Implementation**: Don't try to use ResourceManager in browser - use interop
3. **Resource Files**: Don't create resources in `AvaloniaXKCD.Exports` - they go in `AvaloniaXKCD`
4. **Lit Components**: Don't use `updateWhenLocaleChanges()` - use `@localized()` decorator
5. **String IDs**: Don't forget to add `id` parameter to `msg()` calls
6. **DOM Querying**: Don't query Avalonia DOM in browser tests - use canvas screenshots
7. **Locale Detection**: Don't hardcode locales - use detection and fallback
8. **Force Push**: Never use `git reset` or `git rebase` as force push is not available

### Build and Test Commands

```bash
# Run all tests
make test

# Run desktop app
make develop-desktop

# Run browser app  
make develop-browser

# Extract Lit translations
cd src/AvaloniaXKCD.Site && npm run lit-localize:extract

# Build Lit translations
cd src/AvaloniaXKCD.Site && npm run lit-localize:build

# Format code
make format
```

## Review Process

Before finalizing changes:

1. **Run Tests**: Execute `make test` to catch regressions
2. **Code Review**: Use `code_review` tool before final commit
3. **Security Scan**: Use `codeql_checker` tool after code review
4. **Verify Changes**: Manually test changed functionality
5. **Check Scope**: Review committed files - use `.gitignore` for artifacts
6. **Documentation**: Update relevant documentation files

## Reflection on Past Issues

### Mistakes Made in Localization Implementation

1. **Assembly Reference Error**: Initially used `typeof(LocalizationService).Assembly` which pointed to wrong assembly
   - **Lesson**: Always verify which assembly contains embedded resources
   - **Fix**: Explicitly load correct assembly or use platform-specific approaches

2. **Browser Resource Loading**: Tried to use ResourceManager in browser which failed
   - **Lesson**: Browser WebAssembly has different resource loading constraints
   - **Fix**: Use JavaScript interop to retrieve strings from TypeScript layer

3. **Lit Component Updates**: Used `updateWhenLocaleChanges()` instead of `@localized()` decorator
   - **Lesson**: Follow Lit's recommended patterns for localization
   - **Fix**: Use `@localized()` decorator for automatic re-rendering

4. **DOM Testing**: Tried to query Avalonia DOM elements in browser tests
   - **Lesson**: Avalonia browser implementation doesn't expose DOM (issue #15453)
   - **Fix**: Use canvas screenshots for visual validation

5. **Message IDs**: Forgot to include `id` parameter in some `msg()` calls
   - **Lesson**: XLIFF extraction requires explicit IDs
   - **Fix**: Always use `msg('Text', { id: 'Key_Name' })` format

6. **Static HTML**: Didn't initially implement localization for static HTML content
   - **Lesson**: Browser UI has both component-based and static HTML content
   - **Fix**: Created `dom-translator.ts` for `data-i18n` attribute translation

### Best Practices Going Forward

1. **Verify Architecture**: Before implementing, understand Desktop vs Browser differences
2. **Test Both Targets**: Changes affecting both platforms need testing in both
3. **Follow Existing Patterns**: The codebase has established patterns - use them
4. **Read Documentation**: Check official docs (Avalonia, Lit) for recommended approaches
5. **Ask for Clarification**: If unsure about architecture decisions, seek feedback early
6. **Incremental Testing**: Test after each meaningful change, not just at the end
7. **Review Corrections**: When user corrects implementation, study the changes carefully

## Security Considerations

1. **Dependency Scanning**: Use `gh-advisory-database` tool before adding dependencies
2. **CodeQL**: Run `codeql_checker` before finalizing changes
3. **No Secrets**: Never commit secrets or sensitive data
4. **Input Validation**: Validate user input and external data
5. **Resource Access**: Be cautious with assembly loading and reflection

## When in Doubt

- Check existing implementations for similar functionality
- Review test files to understand expected behavior
- Look at recent commits by the maintainer for patterns
- Ask for feedback before proceeding with significant changes
- Test in both Desktop and Browser targets when applicable
