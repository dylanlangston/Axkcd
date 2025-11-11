# AvaloniaXKCD Core UI

Main UI project: shared Avalonia controls, view models, application logic. Uses MVVM with `CommunityToolkit.Mvvm`.

## Structure

-   **`/Assets/`**: Static resources.
-   **`/Converters/`**: Data binding converters.
-   **`/MarkupExtensions/`**: XAML markup extensions.
-   **`/Models/`**: UI data models.
-   **`/Services/`**: Application services (navigation, data fetching).
-   **`/Settings/`**: Settings and configuration.
-   **`/Styles/`**: Styles and themes.
-   **`/ViewModels/`**: View models.
-   **`/Views/`**: AXAML views.

## Features

-   **Architecture:** `CommunityToolkit.Mvvm`.
-   **UI:** Responsive UI, async operations.
-   **Controls:** Reusable UI components.
-   **Platform:** Works on desktop and browser.

## Development

### Prerequisites

-   .NET 10 SDK
-   Avalonia UI Extension for IDE.

### Getting Started

1.  Open solution.
2.  Restore dependencies:
    ```bash
    dotnet restore
    ```
3.  Build project:
    ```bash
    dotnet build
    ```

### Adding Features

Follow MVVM pattern:

1.  New view in `Views/`.
2.  Corresponding view model in `ViewModels/`.
3.  Register new services in IoC container if needed.
4.  Add tests in `AvaloniaXKCD.Tests`.

## Dependencies

-   Avalonia UI 12.x
-   CommunityToolkit.Mvvm
-   Microsoft.Extensions.DependencyInjection
-   XKCDCore (internal)
