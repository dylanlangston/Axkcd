namespace AvaloniaXKCD.Tests;

public class ViewLocatorGeneratorTests
{
    [Test]
    public Task GeneratesMapForSimplePair()
    {
        var sourceFile = $"{nameof(ViewLocatorGeneratorTests)}.{nameof(GeneratesMapForSimplePair)}.cs";
        return Verify<ViewLocatorGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                // Verify that the generated file contains the correct mapping
                _ => _.GeneratedTrees.ShouldSatisfyAllConditions([
                    trees => trees.Length.ShouldBe(2),
                    trees => trees.Last().GetText().ToString().ShouldContain("{ typeof(TestNamespace.MainViewModel), typeof(TestNamespace.MainView) }")
                ])
            );
    }

    [Test]
    public Task GeneratesMapForMultiplePairs()
    {
        var sourceFile = $"{nameof(ViewLocatorGeneratorTests)}.{nameof(GeneratesMapForMultiplePairs)}.cs";
        return Verify<ViewLocatorGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                _ => _.GeneratedTrees.ShouldSatisfyAllConditions([
                    trees => trees.Length.ShouldBe(2),
                    // Check that both pairs are correctly registered with their full type names
                    trees => trees.Last().GetText().ToString().ShouldSatisfyAllConditions([
                        text => text.ShouldContain("{ typeof(Test.ViewModels.HomeViewModel), typeof(Test.Views.HomeView) }"),
                        text => text.ShouldContain("{ typeof(Test.ViewModels.SettingsViewModel), typeof(Test.Views.SettingsView) }")
                    ])
                ])
            );
    }

    [Test]
    public Task IgnoresOrphanedViewModelAndReportsDiagnostic()
    {
        var sourceFile = $"{nameof(ViewLocatorGeneratorTests)}.{nameof(IgnoresOrphanedViewModelAndReportsDiagnostic)}.cs";
        return Verify<ViewLocatorGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                // Verify that two files are generated
                _ => _.GeneratedTrees.ShouldSatisfyAllConditions([
                    trees => trees.Length.ShouldBe(2),
                    // Check the contents of the last generated file
                    trees => trees.Last().GetText().ToString().ShouldSatisfyAllConditions([
                        // The generated map should NOT contain the orphaned ViewModel
                        text => text.ShouldNotContain("OrphanViewModel"),
                        // It SHOULD contain the valid pair
                        text => text.ShouldContain("{ typeof(TestNamespace.LoginViewModel), typeof(TestNamespace.LoginView) }")
                    ])
                ])
            );
    }

    [Test]
    public Task IgnoresOrphanedView()
    {
        var sourceFile = $"{nameof(ViewLocatorGeneratorTests)}.{nameof(IgnoresOrphanedView)}.cs";
        return Verify<ViewLocatorGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                _ => _.GeneratedTrees.ShouldSatisfyAllConditions([
                    trees => trees.Length.ShouldBe(2),
                    trees => trees.Last().GetText().ToString().ShouldSatisfyAllConditions([
                        // The generator should simply ignore the OrphanView and not report it.
                        text => text.ShouldNotContain("OrphanView"),
                        // It should still generate the correct mapping for the valid pair.
                        text => text.ShouldContain("{ typeof(TestNamespace.ProfileViewModel), typeof(TestNamespace.ProfileView) }")
                    ])
                ])
            );
    }

    [Test]
    public Task IgnoresAbstractClassesAndPairsConcreteImplementations()
    {
        var sourceFile = $"{nameof(ViewLocatorGeneratorTests)}.{nameof(IgnoresAbstractClassesAndPairsConcreteImplementations)}.cs";
        return Verify<ViewLocatorGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                _ => _.GeneratedTrees.ShouldSatisfyAllConditions([
                    trees => trees.Length.ShouldBe(2),
                    trees => trees.Last().GetText().ToString().ShouldSatisfyAllConditions([
                        // The map should not contain the abstract types
                        text => text.ShouldNotContain("BaseViewModel"),
                        text => text.ShouldNotContain("BaseView"),
                        // It should correctly map the concrete implementations
                        text => text.ShouldContain("{ typeof(TestNamespace.DetailsViewModel), typeof(TestNamespace.DetailsView) }")
                    ])
                ])
            );
    }

    [Test]
    public Task GeneratesNoOutputWhenNoPairsFound()
    {
        var sourceFile = $"{nameof(ViewLocatorGeneratorTests)}.{nameof(GeneratesNoOutputWhenNoPairsFound)}.cs";
        return Verify<ViewLocatorGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                // Even with no pairs, the generator should produce the two boilerplate files
                _ => _.GeneratedTrees.ShouldSatisfyAllConditions([
                    trees => trees.Length.ShouldBe(2),
                    // The main mapping file should be generated but contain no mappings
                    trees => trees.Last().GetText().ToString().ShouldNotContain("{ typeof")
                ])
            );
    }

    [Test]
    public Task GeneratesNoViewLocatorWhenAttributeMissing()
    {
        var sourceFile = $"{nameof(ViewLocatorGeneratorTests)}.{nameof(GeneratesNoViewLocatorWhenAttributeMissing)}.cs";
        return Verify<ViewLocatorGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                // The generator should produce the ViewLocatorAttribute
                _ => _.GeneratedTrees.ShouldHaveSingleItem().GetText().ToString().ShouldContain("ViewLocatorAttribute")
            );
    }

    [Test]
    public Task DetectsMultipleViewLocatorAttributesAndReportsError()
    {
        var sourceFile = $"{nameof(ViewLocatorGeneratorTests)}.{nameof(DetectsMultipleViewLocatorAttributesAndReportsError)}.cs";
        return Verify<ViewLocatorGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                _ => _.Diagnostics.ShouldSatisfyAllConditions([
                    diags => diags.Length.ShouldBe(1),
                    diags => diags[0].Id.ShouldBe("XKC001"),
                    diags => diags[0].Severity.ShouldBe(DiagnosticSeverity.Error)
                ])
            );
    }

    [Test]
    public Task HandlesNestedNamespaces()
    {
        var sourceFile = $"{nameof(ViewLocatorGeneratorTests)}.{nameof(HandlesNestedNamespaces)}.cs";
        return Verify<ViewLocatorGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                _ => _.GeneratedTrees.ShouldSatisfyAllConditions([
                    trees => trees.Length.ShouldBe(2),
                    trees => trees.Last().GetText().ToString().ShouldSatisfyAllConditions([
                        text => text.ShouldContain("{ typeof(Test.Nested.Sub.UserViewModel), typeof(Test.Nested.Sub.UserView) }"),
                        text => text.ShouldContain("{ typeof(Test.Nested.AccountViewModel), typeof(Test.Nested.AccountView) }")
                    ])
                ])
            );
    }
}