# AvaloniaXKCD.Browser

Browser version of AvaloniaXKCD, using WebAssembly (WASM). Provides C# backend logic for browser execution, integrating with `AvaloniaXKCD.Site` TypeScript frontend.

## Project Responsibilities

-   **`Program.cs`**: WASM entry point.
-   **`Exports/`**: Browser-specific implementations of interfaces from `AvaloniaXKCD.Exports`.
-   **JavaScript Interop**: Uses JavaScript interop for browser APIs and TypeScript frontend communication.

## Features

-   **WebAssembly Integration:** Uses WebAssembly to run .NET runtime in browser for a native-like experience.
-   **Local Storage:** Caches comics and data in browser local storage for offline access.
-   **Native APIs:** Integrates with browser APIs via JavaScript interop for a rich user experience.
-   **PWA:** Designed as a PWA for offline support and installation.
-   **Browser Support:** Supported on modern web browsers.

## Development

### Prerequisites

-   .NET 10 SDK with WASM workload installed
-   Bun
-   Modern web browser

### Getting Started

1.  Install WASM workload:
    ```bash
    dotnet workload install wasm-tools
    ```
2.  Build project:
    ```bash
    dotnet build
    ```
3.  Run development server:
    ```bash
    make develop-browser
    ```
4.  Open `http://localhost:5235`.

## Building for Production

Creates optimized WASM build in `out/`:

```bash
make publish-browser
```

## Troubleshooting

-   **WASM not loading:** Check browser console for errors. Check network requests.
-   **Performance issues:** Monitor memory usage. Check network requests. Verify caching.
-   **JavaScript interop errors:** Check browser console. Verify method signatures. Check serialization.