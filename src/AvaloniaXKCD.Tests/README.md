# AvaloniaXKCD.Tests

Contains AvaloniaXKCD test suite: unit, integration, E2E tests. Uses Verify for snapshots, Shouldly for assertions.

## Test categories

-   **Unit tests:** view models, services, utilities.
-   **Integration tests:** API and database integration.
-   **E2E tests:** UI workflows, user scenarios.
-   **Snapshot tests:** UI.

## Tools and frameworks

-   **TUnit:** Modern .NET test runner.
-   **Verify:** Snapshot testing library: simplifies approving/reviewing test artifact changes.
-   **Shouldly:** Assertion library: fluent API.
-   **Playwright:** Browser automation library for E2E tests.
-   **Avalonia.Headless:** Headless Avalonia UI for UI tests.

## Running Tests

Run all tests:

```bash
dotnet test
```


Review snapshot changes:

```bash
make verify-review
```

## Troubleshooting

-   **Snapshot failure:** UI changed. Review with `make verify-review`, update snapshots if expected.
-   **Flaky test:** Check async operations, timing. Use `async/await` to ensure waits.
-   **UI test failure:** Check platform issues or UI changes. Ensure test and UI are in sync.