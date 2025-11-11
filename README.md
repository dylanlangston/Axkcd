<div align="center">
  <img src="src/AvaloniaXKCD.Site/public/919f27.svg" alt="AvaloniaXKCD Logo" width="128" height="128"/>
</div>

# AvaloniaXKCD

[![Build Status](https://img.shields.io/github/actions/workflow/status/dylanlangston/axkcd/release.yml?style=flat-square&logo=github)](https://github.com/dylanlangston/axkcd/actions)
![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet)
![Avalonia UI](https://img.shields.io/badge/Avalonia_UI-12.x-blue?style=flat-square)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)

**XKCD viewer for desktop and browser.**

AvaloniaXKCD is an XKCD comic viewer for desktop and browser. It provides a native experience on Windows and Linux, with a browser-based version using WebAssembly.

<div align="center">
  <img src="docs/screenshot.png" alt="AvaloniaXKCD Screenshot"/>
</div>

## Features

-   **Cross-Platform:** Native desktop applications for Windows and Linux, plus a browser-based viewer.
-   **Modern UI:** Responsive interface.
-   **Developer-Friendly:** Structured codebase.

> [!NOTE]
> This project is an unofficial XKCD viewer and is not affiliated with XKCD.com.

## Installation

### Prerequisites

-   .NET 10 SDK
-   Bun (for browser development)
-   Docker (optional, for containerized development)

### Quick Start

```bash
# Clone the repository
git clone https://github.com/dylanlangston/axkcd.git
cd axkcd

# Run the desktop version
make develop-desktop

# Or run the browser version
make develop-browser
```

## Project Structure

The project is organized into several key directories:

```
axkcd/
├── src/                      # Source code
│   ├── AvaloniaXKCD/         # Core UI and application logic (MVVM)
│   ├── AvaloniaXKCD.Desktop/ # Desktop-specific platform code
│   ├── AvaloniaXKCD.Browser/ # Browser-specific platform code (WASM)
│   ├── XKCDCore/             # Library for interacting with the XKCD API
│   └── ...                   # Supporting projects and tests
├── mirror/                   # Local mirror of XKCD comics
└── makefile                  # Scripts for building, testing, and running
```

## Development

### Common Makefile Commands

The `makefile` automates common development tasks:

| Command | Description |
| ------------------ | -------------------------------------------------- |
| `setup` | Sets up the development environment. |
| `develop-desktop` | Runs the desktop application in development mode. |
| `develop-browser` | Runs the browser application in development mode. |
| `publish-windows` | Builds the application for Windows. |
| `publish-linux` | Builds the application for Linux. |
| `publish-browser` | Builds the application for the browser. |
| `test` | Runs the test suite. |
| `format` | Formats the source code. |
| `clean` | Cleans the local environment. |
| `help` | Displays the help menu with all available commands. |

### Development Container

For a consistent environment, this project includes a [Dev Container](https://code.visualstudio.com/docs/remote/containers). This is the recommended way to work on the project, as it ensures all tools and dependencies are set up correctly.

[![Open in Dev Container](https://img.shields.io/static/v1?label=Dev%20Container&message=Open&color=blue&logo=visualstudiocode)](https://vscode.dev/redirect?url=vscode://ms-vscode-remote.remote-containers/cloneInVolume?url=https://github.com/dylanlangston/axkcd)

## Technology

### Core Stack

-   [Avalonia UI](https://avaloniaui.net/): A cross-platform UI framework for .NET.
-   [.NET 10](https://dotnet.microsoft.com/): Platform for building the application.

### Tools & Infrastructure

-   **GitHub Actions:** For CI/CD.
-   **Docker:** For a consistent development environment.
-   **Make:** For build and development tasks.
-   **Bun:** For browser development.

## Contributing

Contributions welcome. See [Contributing Guide](CONTRIBUTING.md).

## Resources

-   [Source Code](https://github.com/dylanlangston/axkcd)
-   [Issue Tracker](https://github.com/dylanlangston/axkcd/issues)
-   [XKCD](https://xkcd.com/)
-   [Explain XKCD](https://www.explainxkcd.com/)

## Acknowledgments

-   Randall Munroe for [XKCD](https://xkcd.com/).
-   The [Avalonia UI](https://avaloniaui.net/) team for the UI framework.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
