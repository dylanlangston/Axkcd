# AvaloniaXKCD.Desktop

Desktop version of AvaloniaXKCD for Windows and Linux. Provides native integration and platform features.

## Supported Platforms

-   Windows 10/11 (x64)
-   Linux x64 (Debian, RedHat, AppImage)
-   macOS (planned)


## Project Structure

-   **`Assets/`**: Platform-specific assets (icons, installers).
-   **`Platform/`**: Platform-specific implementations of interfaces from `AvaloniaXKCD.Exports`.
-   **`Program.cs`**: Desktop entry point.

## Development

### Prerequisites

-   .NET 10 SDK

### Building

Build for current platform:

```bash
dotnet build
```

Build for specific platform:

```bash
make publish-windows
make publish-linux
```

### Running

Run in development mode:

```bash
make develop-desktop
```

Run built application from `out/`:

```bash
./out/AvaloniaXKCD
```

## Packaging

`makefile` in root provides packaging commands:

-   `make publish-windows VERSION=1.0.0`
-   `make publish-linux-deb VERSION=1.0.0`
-   `make publish-linux-rpm VERSION=1.0.0`
-   `make publish-linux-appimage VERSION=1.0.0`