# GitHub Copilot Instructions for AvaloniaXKCD

## Repository Overview

AvaloniaXKCD is a cross-platform XKCD comic viewer built with Avalonia UI and .NET 10. The application runs natively on Windows and Linux desktops, as well as in web browsers using WebAssembly.

**Key Information:**
- **Primary Language:** C# with .NET 10 (latest C# 14 features)
- **UI Framework:** Avalonia UI 12.x
- **Architecture:** MVVM (Model-View-ViewModel)
- **License:** MIT
- **Maintainer:** Dylan Langston

**Ubiquitous Language:**
- **Comic**: An XKCD comic strip with metadata (title, alt text, image URL)
- **Mirror**: Local cache of XKCD comics for offline viewing
- **View**: AXAML-based UI component
- **ViewModel**: Business logic and state management for Views
- **Service**: Application-level functionality (API calls, caching, etc.)

## Project Structure

```
Axkcd/
├── src/
│   ├── AvaloniaXKCD/           # Core application logic (MVVM)
│   │   ├── Models/             # Data models
│   │   ├── ViewModels/         # View models
│   │   ├── Views/              # UI views (AXAML)
│   │   ├── Services/           # Application services
│   │   └── Converters/         # Value converters
│   ├── AvaloniaXKCD.Desktop/   # Desktop platform code
│   ├── AvaloniaXKCD.Browser/   # Browser/WASM platform code
│   ├── AvaloniaXKCD.Tests/     # Test suite
│   ├── AvaloniaXKCD.Generators/ # Source generators
│   ├── AvaloniaXKCD.Utilities/ # Utility code
│   ├── AvaloniaXKCD.Exports/   # Export functionality
│   ├── XKCDCore/               # XKCD API interaction library
│   └── AvaloniaXKCD.Site/      # Website assets
├── mirror/                      # Local XKCD comic mirror
├── .github/                     # GitHub configuration
│   ├── workflows/              # CI/CD workflows
│   └── actions/                # Custom GitHub Actions
├── docs/                        # Documentation
└── makefile                     # Build automation
```

## Coding Standards

### C# Style Guidelines

- **Language Version:** Always use latest C# features, currently C# 14
- **Nullable Reference Types:** Enabled (`<Nullable>enable</Nullable>`)
  - Declare variables non-nullable by default
  - Always use `is null` or `is not null` instead of `== null` or `!= null`
  - Trust the C# null annotations and don't add unnecessary null checks
- **Implicit Usings:** Enabled
- **Warnings as Errors:** Enabled (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`)
- **Indentation:** Follow existing file conventions (typically 4 spaces)
- **Modern C# Features:**
  - Use pattern matching and switch expressions wherever possible
  - Use `nameof` instead of string literals when referring to member names
  - Prefer file-scoped namespace declarations
  - Use single-line using directives
- **Naming Conventions:**
  - PascalCase for public members, types, and namespaces
  - camelCase for private fields and local variables
  - Prefix interfaces with "I" (e.g., `IComicService`)
  - Prefix private fields with underscore if that's the existing convention in the file
- **Documentation:**
  - Write clear and concise comments explaining why, not what
  - Write code with good maintainability practices
  - Document design decisions and trade-offs
  - Ensure XML doc comments for public APIs where applicable
- **Error Handling:**
  - Handle edge cases gracefully
  - Write clear exception handling with appropriate exception types
  - Implement a consistent error handling strategy

### MVVM Patterns

- **ViewModels:**
  - Keep ViewModels in `ViewModels/` directory
  - Implement `INotifyPropertyChanged` for data binding
  - ViewModels should not reference Views directly
  - Keep presentation logic in ViewModels, not in Views
- **Views:**
  - Keep Views in `Views/` directory
  - Views should be thin, containing only UI-related code
  - Use data binding to connect to ViewModels
- **Models:**
  - Models should be immutable when possible
  - Use record types for simple data transfer objects
  - Place business logic in services, not models

### AXAML Guidelines

- Use Avalonia's AXAML (Avalonia XAML) for UI definitions
- Follow existing styling patterns in `Styles.axaml` and `ColorResources.axaml`
- Keep views focused and reusable
- Use proper data binding syntax
- Leverage Avalonia's styling system for consistent UI

### Formatting

- Apply code-formatting style defined in `.editorconfig`
- Insert a newline before the opening curly brace of code blocks
- Ensure final return statement of a method is on its own line
- Run `make format` before committing

### Asynchronous Programming

- Use `async` and `await` for I/O-bound operations
- Never block on async code (no `.Result` or `.Wait()`)
- Use `ConfigureAwait(false)` when possible in library code
- Name async methods with `Async` suffix (e.g., `LoadComicAsync`)

### Dependency Injection

- Use constructor injection to acquire dependencies
- Register services in `ServiceProvider.cs`
- Keep dependencies explicit and minimal
- Prefer interfaces over concrete implementations

### File Organization

- Keep files organized within their appropriate directories
- Place tests in `AvaloniaXKCD.Tests/` following the same structure as source code
- Source generators belong in `AvaloniaXKCD.Generators/`

## Development Workflow

### Prerequisites

- .NET 10 SDK
- Bun (for browser development)
- Docker (optional, for containerized development)

### Setup

```bash
# First-time setup
make setup

# Install Playwright browsers for testing
make setup-playwright
```

### Building and Running

```bash
# Run desktop version in development mode
make develop-desktop

# Run browser version in development mode
make develop-browser

# Format code before committing
make format

# Run tests
make test

# Clean build artifacts
make clean
```

### Publishing

```bash
# Publish for Windows (requires VERSION variable)
VERSION=1.0.0 make publish-windows

# Publish for Linux (requires VERSION variable)
VERSION=1.0.0 make publish-linux

# Publish for browser (requires VERSION variable)
VERSION=1.0.0 make publish-browser
```

### Docker-based Workflows

```bash
# Test via Docker
make docker-test

# Publish via Docker (requires VERSION variable)
VERSION=1.0.0 make docker-publish-windows
VERSION=1.0.0 make docker-publish-linux
VERSION=1.0.0 make docker-publish-browser
```

## Testing Guidelines

### Test Framework

- All tests use **TUnit** testing framework
- Tests must pass before submitting a PR
- Include tests for new functionality
- Snapshot tests can be reviewed with `make verify-review`
- Test files should mirror the structure of source files

### Test Structure

```csharp
[Test]
public async Task LoadComic_ValidId_ReturnsComic()
{
    // Arrange - Set up test data and dependencies
    var service = new ComicService();
    var comicId = 123;

    // Act - Execute the method being tested
    var result = await service.LoadComicAsync(comicId);

    // Assert - Verify the expected outcome
    Assert.NotNull(result);
    Assert.Equal(comicId, result.Id);
}
```

### Test Categories

- **Unit Tests:** Test individual components in isolation
  - Focus on ViewModels, Services, and business logic
  - Mock external dependencies
  - Fast and deterministic
- **Integration Tests:** Test component interactions
  - Test API integrations
  - Test persistence and caching
  - Validate data flow between layers
- **UI Tests:** Test user interface behavior
  - Use Playwright for browser testing
  - Test user workflows and interactions
  - Validate visual elements

### Test Best Practices

- **DO NOT** emit "Arrange", "Act", or "Assert" comments in production code
- Copy existing test style for method names and capitalization
- Test critical paths and edge cases
- Test error scenarios and validation
- Use descriptive test names that explain what is being tested
- Keep tests isolated and independent
- Ensure tests are repeatable and deterministic
- Mock external dependencies appropriately

## Contribution Process

1. **Open an Issue First:** For bugs or features, open an issue and get confirmation before coding
2. **Branch Naming:** Use format `{issue-number}-{description}` (e.g., `325-add-japanese-localization`)
3. **Code Quality:**
   - Run `make format` to format code
   - Ensure `make test` passes
   - Follow existing code patterns
4. **Pull Requests:**
   - Reference the issue number
   - Include tests for new functionality
   - Ensure CI passes

## Technology Stack

### Core Technologies

- **.NET 10:** Runtime and SDK
- **Avalonia UI 12.x:** Cross-platform UI framework
- **C#:** Primary programming language
- **WebAssembly:** For browser deployment

### Build & Development Tools

- **Make:** Build automation
- **Docker:** Containerized builds and development
- **GitHub Actions:** CI/CD automation
- **Bun:** JavaScript tooling for browser version
- **PupNet:** Cross-platform packaging tool
- **dotnet format:** Code formatting
- **TUnit:** Testing framework
- **Verify:** Snapshot testing tool

### Package Management

- **NuGet:** .NET package management
- **Central Package Management:** Using `Directory.Packages.props`

## Common Tasks for Copilot

### Adding a New View

1. **Create View AXAML** in `src/AvaloniaXKCD/Views/`
   - Follow existing naming conventions (e.g., `ComicView.axaml`)
   - Define UI structure using Avalonia AXAML
   - Apply consistent styling from `Styles.axaml`
2. **Create ViewModel** in `src/AvaloniaXKCD/ViewModels/`
   - Implement `INotifyPropertyChanged` for data binding
   - Add properties for data binding
   - Implement commands for user interactions
3. **Register in ViewLocator** if using convention-based view resolution
   - Update `ViewLocator.cs` to map ViewModel to View
4. **Add Navigation Logic**
   - Update relevant ViewModels with navigation commands
   - Ensure proper state management during navigation
5. **Write Tests**
   - Create unit tests for ViewModel logic
   - Add UI tests for user interactions if appropriate

### Adding a New Service

1. **Define Interface** in `src/AvaloniaXKCD/Services/`
   - Create interface with clear method signatures
   - Use async methods for I/O operations
   - Document expected behavior and exceptions
2. **Implement Service**
   - Create concrete implementation
   - Follow single responsibility principle
   - Handle errors appropriately
3. **Register Service**
   - Register in `ServiceProvider.cs`
   - Use appropriate lifetime (Singleton, Transient, Scoped)
4. **Write Tests**
   - Test service logic with unit tests
   - Mock dependencies as needed
   - Test error scenarios

### Modifying XKCD API Interaction

- **Location:** Work in the `XKCDCore` project
- **Purpose:** Library for interacting with the XKCD API
- **Guidelines:**
  - Keep it independent from Avalonia-specific code
  - Use standard .NET HTTP patterns
  - Implement proper error handling for network issues
  - Cache responses appropriately
  - Follow XKCD API rate limiting guidelines

### Working with Tests

- **Location:** Tests are in `AvaloniaXKCD.Tests/`
- **Framework:** Use TUnit for unit tests
- **UI Testing:** Playwright tests available for browser version
- **Snapshot Testing:** Use `make verify-review` to review snapshot test changes
- **Structure:** Mirror source code structure in test project
- **Naming:** Use descriptive test method names

### Performance Optimization

- **WASM Considerations:**
  - Minimize bundle size by tree-shaking unused code
  - Use lazy loading for heavy components
  - Optimize image loading and caching
  - Profile WASM performance regularly
- **Desktop Performance:**
  - Use async operations to keep UI responsive
  - Implement efficient data binding patterns
  - Cache frequently accessed data
  - Optimize rendering with virtualization when needed
- **General:**
  - Use appropriate collection types
  - Avoid unnecessary allocations
  - Profile and benchmark critical paths

## Important Notes

- **Cross-Platform Considerations:** 
  - Code must work on Windows, Linux, and Browser (WASM)
  - Test on multiple platforms when possible
  - Be aware of platform-specific limitations
  - Use Avalonia's cross-platform APIs
- **Performance:** 
  - Be especially mindful of WASM performance constraints
  - Minimize JavaScript interop calls
  - Optimize bundle size for browser version
  - Use async operations to maintain responsiveness
- **Security:**
  - Validate all user inputs
  - Handle API errors gracefully
  - Don't expose sensitive information in error messages
  - Follow security best practices for web deployment
- **Unofficial Project:** 
  - This is an unofficial XKCD viewer, not affiliated with xkcd.com
  - Respect XKCD's API usage guidelines
  - Include proper attribution
- **Dev Container:** 
  - Recommended development environment available in `.devcontainer/`
  - Ensures consistent tooling across team members
  - Pre-configured with all necessary dependencies

## Code Review Guidelines

When reviewing code changes:
- **Verify** adherence to coding standards and MVVM patterns
- **Check** for proper error handling and edge cases
- **Ensure** tests are included for new functionality
- **Validate** performance implications, especially for WASM
- **Confirm** cross-platform compatibility considerations
- **Review** for security vulnerabilities
- **Assess** maintainability and code clarity
- **Verify** documentation and comments are appropriate

## Anti-Patterns to Avoid

- **DON'T** put business logic in Views or code-behind
- **DON'T** block async code with `.Result` or `.Wait()`
- **DON'T** ignore null reference handling
- **DON'T** use magic strings or numbers (use constants)
- **DON'T** create tight coupling between components
- **DON'T** skip writing tests for new features
- **DON'T** ignore existing code patterns and conventions
- **DON'T** make sweeping changes without discussion

## Localization Architecture (Key Details)

### Export System

The project uses a custom Export/Import system (`AvaloniaXKCD.Exports`) for dependency injection:

- **Interfaces**: Define contracts in `AvaloniaXKCD.Exports` (must implement `IExport`)
- **Abstract base classes**: Can live in `AvaloniaXKCD/Exports/` namespace
- **Platform implementations**: In `AvaloniaXKCD.Desktop/Exports/` or `AvaloniaXKCD.Browser/Exports/`
- **Registration**: Automatic via source generators
- **Retrieval**: `ExportContainer.Get<IInterface>()`
- **Multi-registration**: Use `AddMulti<T>()` and `GetAll<T>()` for multiple implementations

### Localization Implementation Patterns

**Critical Insight**: Browser and Desktop have fundamentally different localization approaches:

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

### Common Localization Mistakes to Avoid

1. **Assembly Loading**: Don't assume `typeof(Class).Assembly` has resources - verify which assembly!
2. **Browser Implementation**: Don't try to use ResourceManager in browser - use interop
3. **Resource Files**: Don't create resources in `AvaloniaXKCD.Exports` - they go in `AvaloniaXKCD`
4. **Lit Components**: Don't use `updateWhenLocaleChanges()` - use `@localized()` decorator
5. **String IDs**: Don't forget to add `id` parameter to `msg()` calls: `msg('Text', { id: 'Key_Name' })`
6. **DOM Querying**: Don't query Avalonia DOM in browser tests - use canvas screenshots
7. **Locale Detection**: Don't hardcode locales - use detection and fallback

### Localization Testing

1. **Snapshot Testing**: Use Verify for regression protection
2. **Test Methods**: Use descriptive names like `ShouldDetectBrowserLocale`
3. **Assertions in Snapshots**: Nest assertions within `Verify()` calls
4. **Browser Tests**: Use canvas screenshots for Avalonia browser tests (DOM querying doesn't work due to Avalonia issue #15453)
5. **Test Both Platforms**: Desktop and Browser need separate validation

## Reflection on Past Implementation Issues

### Lessons Learned from Localization Work

1. **Assembly Reference Error**: Initially used `typeof(LocalizationService).Assembly` which pointed to wrong assembly
   - **Fix**: Explicitly load correct assembly or use platform-specific approaches

2. **Browser Resource Loading**: Tried to use ResourceManager in browser which failed
   - **Fix**: Use JavaScript interop to retrieve strings from TypeScript layer

3. **Lit Component Updates**: Used `updateWhenLocaleChanges()` instead of `@localized()` decorator
   - **Fix**: Follow Lit's recommended patterns for localization

4. **DOM Testing**: Tried to query Avalonia DOM elements in browser tests
   - **Fix**: Use canvas screenshots for visual validation

5. **Message IDs**: Forgot to include `id` parameter in some `msg()` calls
   - **Fix**: Always use `msg('Text', { id: 'Key_Name' })` format

6. **Static HTML**: Didn't initially implement localization for static HTML content
   - **Fix**: Created `dom-translator.ts` for `data-i18n` attribute translation

### Best Practices Going Forward

1. **Verify Architecture**: Before implementing, understand Desktop vs Browser differences
2. **Test Both Targets**: Changes affecting both platforms need testing in both
3. **Follow Existing Patterns**: The codebase has established patterns - use them
4. **Read Documentation**: Check official docs (Avalonia, Lit) for recommended approaches
5. **Ask for Clarification**: If unsure about architecture decisions, seek feedback early
6. **Incremental Testing**: Test after each meaningful change, not just at the end
7. **Review Corrections**: When user corrects implementation, study the changes carefully

## Security and Review Process

### Before Finalizing Changes

1. **Run Tests**: Execute `make test` to catch regressions
2. **Code Review**: Use `code_review` tool before final commit
3. **Security Scan**: Use `codeql_checker` tool after code review
4. **Verify Changes**: Manually test changed functionality
5. **Check Scope**: Review committed files - use `.gitignore` for artifacts
6. **Documentation**: Update relevant documentation files

### Security Considerations

1. **Dependency Scanning**: Use `gh-advisory-database` tool before adding dependencies
2. **CodeQL**: Run `codeql_checker` before finalizing changes
3. **No Secrets**: Never commit secrets or sensitive data
4. **Input Validation**: Validate user input and external data
5. **Resource Access**: Be cautious with assembly loading and reflection

## Additional Resources

- [Source Code](https://github.com/dylanlangston/axkcd)
- [Issue Tracker](https://github.com/dylanlangston/axkcd/issues)
- [Localization Guide](LOCALIZATION.md)
- [Avalonia UI Documentation](https://docs.avaloniaui.net/)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [XKCD API](https://xkcd.com/json.html)
