# AvaloniaXKCD.Exports

Contains shared interfaces and abstractions for platform-specific implementations across AvaloniaXKCD projects. Acts as a contract between core logic and platform code.

## Best Practices

-   Each interface: single, defined responsibility.
-   Avoid platform-specific types in interfaces.
-   No platform-specific library dependencies.
-   Document expected behavior with XML comments.
-   Specify pre/post-conditions with code contracts.
