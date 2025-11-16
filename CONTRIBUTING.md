# Contributing to AvaloniaXKCD

Thank you for considering contributing to AvaloniaXKCD!

Found a bug or have a feature request? [Open an issue](https://github.com/dylanlangston/axkcd/issues/new). Get confirmation/approval before coding.

### Branching

Branch name example (issue #325):

```sh
git checkout -b 325-add-japanese-localization
```

### Development Setup

Use the [Dev Container](https://code.visualstudio.com/docs/remote/containers) for a consistent development environment. It ensures tools and dependencies are set up correctly.

[![Open in Dev Container](https://img.shields.io/static/v1?label=Dev%20Container&message=Open&color=blue&logo=visualstudiocode)](https://vscode.dev/redirect?url=vscode://ms-vscode-remote.remote-containers/cloneInVolume?url=https://github.com/dylanlangston/axkcd)

### Contribution Workflow

1.  Tests must pass.
2.  Include tests for new functionality.
3.  Ensure code is formatted and linted before submitting:
    - Run `make format` to format all code (C# and TypeScript).
    - Run `make lint` to check for linting issues in TypeScript.
    - Run `make format-check` to verify formatting without making changes.

### Code Quality Standards

This project uses automated linting and formatting tools to maintain code quality and consistency:

- **C#**: Uses CSharpier for opinionated formatting and `dotnet format` for style analysis
- **TypeScript/JavaScript**: Uses Prettier for formatting and ESLint for linting

**Available Commands:**

- `make format` - Format all code (C# and TypeScript)
- `make format-csharp` - Format only C# code
- `make format-typescript` - Format only TypeScript code
- `make lint` - Run all linters
- `make lint-typescript` - Lint TypeScript code
- `make format-check` - Check formatting without making changes (used in CI)

The CI pipeline will automatically check formatting and linting on all pull requests.
