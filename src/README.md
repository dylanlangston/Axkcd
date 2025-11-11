# AvaloniaXKCD Source Code

This directory contains AvaloniaXKCD source code.

## Structure

-   **`/src/AvaloniaXKCD/`**: Core UI project: shared controls, view models, logic. Uses MVVM with `CommunityToolkit.Mvvm`.
-   **`/src/AvaloniaXKCD.Browser/`**: Browser platform: WebAssembly (WASM) implementation.
-   **`/src/AvaloniaXKCD.Desktop/`**: Desktop platform: Native desktop application (Windows, Linux).
-   **`/src/AvaloniaXKCD.Exports/`**: Supporting library: Common cross-platform interfaces and models.
-   **`/src/AvaloniaXKCD.Generators/`**: Project for build-time code generation: performance optimizations.
-   **`/src/AvaloniaXKCD.Site/`**: Web frontend assets and static site generation for browser version.
-   **`/src/AvaloniaXKCD.Tests/`**: Test suite: snapshot and integration tests.
-   **`/src/XKCDCore/`**: .NET library: type-safe access to XKCD and ExplainXKCD APIs.

## Development

Development requires .NET 10 SDK and Bun.

```bash
# Restore .NET dependencies
dotnet restore

# Set up the environment
make setup

# Run the desktop version
make develop-desktop

# Run the browser version
make develop-browser
```

### Testing

Run the test suite:

```bash
make test
```

## Building and Publishing

`makefile` in root provides commands for building and publishing:

-   `make publish-windows`
-   `make publish-linux`
-   `make publish-browser`

## Contributing

Contributions welcome. See [Contributing Guide](../../CONTRIBUTING.md). Work in the appropriate project and follow coding standards.

See individual README files for component details.