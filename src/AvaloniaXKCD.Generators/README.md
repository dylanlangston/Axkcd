# AvaloniaXKCD.Generators

Contains C# source generators for AvaloniaXKCD. Optimizes performance by replacing reflection with compile-time code generation.

## Generators are designed to:

-   **Generate boilerplate MVVM code:** Creates view model boilerplate, freeing developers for application logic.
-   **Create strongly-typed resource accessors:** Provides compile-time type safety and improved performance.
-   **Create optimized data binding code:** Faster and more efficient than reflection.

## Creating a generator

1.  Create Scriban template.
2.  Create generator class implementing `ISourceGenerator`.
3.  Register in `GeneratorRegistration.cs`.
4.  Add tests in `AvaloniaXKCD.Tests/Generators/`.

## Debugging

Debug generators: enable environment variables:

```bash
export DOTNET_GENERATING_SOURCES=1
export DOTNET_ROSLYN_LOG_LEVEL=Debug
```

Compiler outputs generated source files to `obj/` for inspection.

## Resources

- [Source Generator Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)
- [Scriban Template Engine](https://github.com/scriban/scriban)
- [Project Architecture](../README.md)