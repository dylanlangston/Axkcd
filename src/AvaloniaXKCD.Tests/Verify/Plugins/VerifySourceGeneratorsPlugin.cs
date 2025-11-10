using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;

namespace AvaloniaXKCD.Tests.VerifyPlugins;

/// <summary>
/// Helper methods for verifying C# Source Generators.
/// </summary>
public static class VerifySourceGeneratorsPlugin
{
    public static AssertionVerificationTask<GeneratorDriverRunResult> Verify<TGenerator>(string sourceFile, VerifySettings? settings = null)
        where TGenerator : IIncrementalGenerator, new()
    {
        var (runResult, _) = RunGenerator<TGenerator>(sourceFile);

        var verifier = Verifier.Verify(runResult, settings);

        return new AssertionVerificationTask<GeneratorDriverRunResult>(runResult, verifier);
    }

    private record VerifySourceGeneratorOutput(
        GeneratorDriverRunResult Result,
        Compilation Output
    );

    private static VerifySourceGeneratorOutput RunGenerator<TGenerator>(string sourceFile)
        where TGenerator : IIncrementalGenerator, new()
    {
        // Read the source code from the conventional path
        var fullPath = Path.Combine(AppContext.BaseDirectory, "Verify", "Inputs", sourceFile);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Source file not found at '{fullPath}'.", fullPath);
        }
        var source = File.ReadAllText(fullPath);

        // Create a Roslyn compilation
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IExport).Assembly.Location)
        };

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: references);

        // Create and run the generator
        var generator = new TGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);

        return new(driver.GetRunResult(), output);
    }
}