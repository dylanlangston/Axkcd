namespace AvaloniaXKCD.Tests;

public class ExportRegistrarGeneratorTests
{
    [Test]
    public Task GeneratesRegistrarForSimpleExport()
    {
        var sourceFile = $"{nameof(ExportRegistrarGeneratorTests)}.{nameof(GeneratesRegistrarForSimpleExport)}.cs";
        return Verify<ExportRegistrarGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(_ =>
                _.GeneratedTrees.ShouldHaveSingleItem()
                    .GetText()
                    .ToString()
                    .ShouldContain("ExportContainer.Add<TestNamespace.IService, TestNamespace.ServiceImpl>")
            );
    }

    [Test]
    public Task GeneratesRegistrarForMultipleImplementations()
    {
        var sourceFile =
            $"{nameof(ExportRegistrarGeneratorTests)}.{nameof(GeneratesRegistrarForMultipleImplementations)}.cs";
        return Verify<ExportRegistrarGenerator>(sourceFile);
    }

    [Test]
    public Task GeneratesRegistrarWithConstructorDependencies()
    {
        var sourceFile =
            $"{nameof(ExportRegistrarGeneratorTests)}.{nameof(GeneratesRegistrarWithConstructorDependencies)}.cs";
        return Verify<ExportRegistrarGenerator>(sourceFile);
    }

    [Test]
    public Task SkipsAbstractClassesAndRegistersConcreteImplementation()
    {
        var sourceFile =
            $"{nameof(ExportRegistrarGeneratorTests)}.{nameof(SkipsAbstractClassesAndRegistersConcreteImplementation)}.cs";
        return Verify<ExportRegistrarGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(_ =>
                _.GeneratedTrees.ShouldHaveSingleItem()
                    .GetText()
                    .ToString()
                    .ShouldContain("ExportContainer.Add<TestNamespace.IService, TestNamespace.ConcreteService>();")
            );
    }

    [Test]
    public Task HandlesMultipleInterfacesOnSingleImplementation()
    {
        var sourceFile =
            $"{nameof(ExportRegistrarGeneratorTests)}.{nameof(HandlesMultipleInterfacesOnSingleImplementation)}.cs";
        return Verify<ExportRegistrarGenerator>(sourceFile);
    }

    [Test]
    public Task ChoosesConstructorWithMostParameters()
    {
        var sourceFile = $"{nameof(ExportRegistrarGeneratorTests)}.{nameof(ChoosesConstructorWithMostParameters)}.cs";
        return Verify<ExportRegistrarGenerator>(sourceFile);
    }

    [Test]
    public Task HandlesNoExportsFoundGracefully()
    {
        var sourceFile = $"{nameof(ExportRegistrarGeneratorTests)}.{nameof(HandlesNoExportsFoundGracefully)}.cs";
        return Verify<ExportRegistrarGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(_ =>
                _.Diagnostics.ShouldHaveSingleItem()
                    .GetMessage()
                    .ToString()
                    .ShouldContain("The ExportRegistrarGenerator did not find any classes implementing IExport.")
            );
    }
}
