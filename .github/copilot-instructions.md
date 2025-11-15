# GitHub Copilot Instructions for AvaloniaXKCD

## Repository Overview

AvaloniaXKCD is a cross-platform XKCD comic viewer built with Avalonia UI and .NET 10. The application runs natively on Windows and Linux desktops, as well as in web browsers using WebAssembly.

**Key Information:**
- **Primary Language:** C# with .NET 10
- **UI Framework:** Avalonia UI 12.x
- **Architecture:** MVVM (Model-View-ViewModel)
- **License:** MIT
- **Maintainer:** Dylan Langston

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

- **Language Version:** Latest C#
- **Nullable Reference Types:** Enabled (`<Nullable>enable</Nullable>`)
- **Implicit Usings:** Enabled
- **Warnings as Errors:** Enabled (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`)
- **Indentation:** Follow existing file conventions (typically 4 spaces)
- **Naming Conventions:**
  - PascalCase for public members, types, and namespaces
  - camelCase for private fields
  - Prefix private fields with underscore if that's the existing convention in the file
- **MVVM Patterns:**
  - Keep ViewModels in `ViewModels/` directory
  - Keep Views in `Views/` directory
  - Models should be immutable when possible
  - Use `INotifyPropertyChanged` for ViewModels

### AXAML Guidelines

- Use Avalonia's AXAML (Avalonia XAML) for UI definitions
- Follow existing styling patterns in `Styles.axaml` and `ColorResources.axaml`
- Keep views focused and reusable

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

- All tests use TUnit testing framework
- Tests must pass before submitting a PR
- Include tests for new functionality
- Snapshot tests can be reviewed with `make verify-review`
- Test files should mirror the structure of source files

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

1. Create the View AXAML file in `src/AvaloniaXKCD/Views/`
2. Create the ViewModel in `src/AvaloniaXKCD/ViewModels/`
3. Register in `ViewLocator.cs` if needed
4. Add navigation logic in appropriate ViewModels

### Adding a New Service

1. Define the interface in `src/AvaloniaXKCD/Services/`
2. Implement the service
3. Register in `ServiceProvider.cs` or use dependency injection

### Modifying XKCD API Interaction

- Work in the `XKCDCore` project
- This is a library for interacting with the XKCD API
- Keep it independent from Avalonia-specific code

### Working with Tests

- Tests are in `AvaloniaXKCD.Tests/`
- Use TUnit for unit tests
- Playwright tests available for UI testing
- Use `make verify-review` to review snapshot test changes

## Important Notes

- **Cross-Platform Considerations:** Code should work on Windows, Linux, and Browser (WASM)
- **Performance:** Be mindful of WASM performance constraints
- **Unofficial Project:** This is an unofficial XKCD viewer, not affiliated with xkcd.com
- **Dev Container:** Recommended development environment available in `.devcontainer/`

## Additional Resources

- [Source Code](https://github.com/dylanlangston/axkcd)
- [Issue Tracker](https://github.com/dylanlangston/axkcd/issues)
- [Avalonia UI Documentation](https://docs.avaloniaui.net/)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [XKCD API](https://xkcd.com/json.html)
