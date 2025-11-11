# AvaloniaXKCD.Site

Contains web frontend for AvaloniaXKCD. Hosts WASM application and provides TypeScript integration.

## Project provides

-   **WASM Hosting:** Hosts `AvaloniaXKCD.Browser` WASM app and provides infrastructure.
-   **TypeScript Integration:** Contains TypeScript code integrating with C# WASM app for communication.
-   **Asset Management:** Manages web assets (HTML, CSS, JS).

## TypeScript layer responsibilities

-   Initializes WASM app and provides configuration.
-   Integrates WASM app with DOM for UI rendering.
-   Handles browser events, forwards to WASM app.

## Development

### Prerequisites

-   Bun
-   TypeScript
-   .NET SDK

### Local Development

Start development server:

```bash
make develop-browser
```

### Building

Build production web assets:

```bash
make publish-browser
```

## Integration

-   Handles TypeScript/web logic.
-   `AvaloniaXKCD.Browser`: C# WASM code.
-   Components work together for web experience.

## Resources

- [Project Architecture](../README.md)
- [Browser Component](../AvaloniaXKCD.Browser/README.md)