namespace AvaloniaXKCD.Tests;

public class ServiceProviderGeneratorTests
{
    [Test]
    public Task GeneratesProviderForSingleService()
    {
        var sourceFile = $"{nameof(ServiceProviderGeneratorTests)}.{nameof(GeneratesProviderForSingleService)}.cs";
        return Verify<ServiceProviderGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                // Verify that the generated file contains the correct service registration
                _ => _.GeneratedTrees.ShouldSatisfyAllConditions([
                    trees => trees.Length.ShouldBe(4), // Includes the 3 attribute files
                    trees => trees.Last().GetText().ToString().ShouldContain("services.AddSingleton<TestNamespace.TestService>();")
                ])
            );
    }

    [Test]
    public Task GeneratesProviderForMultipleServices()
    {
        var sourceFile = $"{nameof(ServiceProviderGeneratorTests)}.{nameof(GeneratesProviderForMultipleServices)}.cs";
        return Verify<ServiceProviderGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                _ => _.GeneratedTrees.ShouldSatisfyAllConditions([
                    trees => trees.Length.ShouldBe(4),
                    trees => trees.Last().GetText().ToString().ShouldSatisfyAllConditions([
                        text => text.ShouldContain("services.AddSingleton<Test.Services.LoggerService>();"),
                        text => text.ShouldContain("services.AddTransient<Test.Services.DataService>();")
                    ])
                ])
            );
    }

    [Test]
    public Task IgnoresAbstractServices()
    {
        var sourceFile = $"{nameof(ServiceProviderGeneratorTests)}.{nameof(IgnoresAbstractServices)}.cs";
        return Verify<ServiceProviderGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                _ => _.GeneratedTrees.ShouldSatisfyAllConditions([
                    trees => trees.Length.ShouldBe(4),
                    trees => trees.Last().GetText().ToString().ShouldSatisfyAllConditions([
                        // The abstract service should not be registered
                        text => text.ShouldNotContain("BaseService"),
                        // The concrete service should be registered
                        text => text.ShouldContain("services.AddSingleton<TestNamespace.ConcreteService>();")
                    ])
                ])
            );
    }

    [Test]
    public Task HandlesAllServiceLifetimes()
    {
        var sourceFile = $"{nameof(ServiceProviderGeneratorTests)}.{nameof(HandlesAllServiceLifetimes)}.cs";
        return Verify<ServiceProviderGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                _ => _.GeneratedTrees.ShouldSatisfyAllConditions([
                    trees => trees.Length.ShouldBe(4),
                    trees => trees.Last().GetText().ToString().ShouldSatisfyAllConditions([
                        text => text.ShouldContain("services.AddSingleton<Test.Services.SingletonService>();"),
                        text => text.ShouldContain("services.AddScoped<Test.Services.ScopedService>();"),
                        text => text.ShouldContain("services.AddTransient<Test.Services.TransientService>();")
                    ])
                ])
            );
    }

    [Test]
    public Task GeneratesNoOutputWhenNoServicesFound()
    {
        var sourceFile = $"{nameof(ServiceProviderGeneratorTests)}.{nameof(GeneratesNoOutputWhenNoServicesFound)}.cs";
        return Verify<ServiceProviderGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                _ => _.GeneratedTrees.ShouldSatisfyAllConditions([
                    trees => trees.Length.ShouldBe(4),
                    // The provider should be generated but with no service registrations
                    trees => trees.Last().GetText().ToString().ShouldNotContain("services.Add")
                ])
            );
    }

    [Test]
    public Task GeneratesNoServiceProviderWhenAttributeMissing()
    {
        var sourceFile = $"{nameof(ServiceProviderGeneratorTests)}.{nameof(GeneratesNoServiceProviderWhenAttributeMissing)}.cs";
        return Verify<ServiceProviderGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                // Only the attribute files should be generated
                _ => _.GeneratedTrees.ShouldSatisfyAllConditions([
                    trees => trees.Length.ShouldBe(3),
                    trees => trees.Any(t => t.GetText().ToString().Contains("ServiceProviderAttribute")),
                    trees => trees.Any(t => t.GetText().ToString().Contains("ServiceAttribute")),
                    trees => trees.Any(t => t.GetText().ToString().Contains("ServiceLifetime"))
                ])
            );
    }

    [Test]
    public Task HandlesMultipleServiceProviderAttributesError()
    {
        var sourceFile = $"{nameof(ServiceProviderGeneratorTests)}.{nameof(HandlesMultipleServiceProviderAttributesError)}.cs";
        return Verify<ServiceProviderGenerator>(sourceFile)
            .Assert<GeneratorDriverRunResult>(
                _ => _.GeneratedTrees.ShouldSatisfyAllConditions([
                    trees => trees.Length.ShouldBe(3), // Only attribute files
                    trees => trees.All(t => !t.GetText().ToString().Contains("ConfigureServices"))
                ])
            );
    }
}