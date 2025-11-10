using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace AvaloniaXKCD.Generators
{
    [Generator]
    public class ServiceProviderGenerator : IIncrementalGenerator
    {
        private const string ServiceAttributeName = "AvaloniaXKCD.Generators.ServiceAttribute";
        private const string ServiceProviderAttributeName = "AvaloniaXKCD.Generators.ServiceProviderAttribute";

        public record ServiceInfo(string ImplementationType, string? ServiceType, string Lifetime);
        public record TargetClassInfo(string Namespace, string ClassName);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif

            context.RegisterPostInitializationOutput(ctx =>
            {
                var serviceLifetimeSource = GetEmbeddedResource("Resources.ServiceLifetime.cs");
                ctx.AddSource("ServiceLifetime.g.cs", SourceText.From(serviceLifetimeSource, Encoding.UTF8));

                var attributeSource = GetEmbeddedResource("Resources.ServiceAttribute.cs");
                ctx.AddSource("ServiceAttribute.g.cs", SourceText.From(attributeSource, Encoding.UTF8));

                var serviceProviderAttributeSource = GetEmbeddedResource("Resources.ServiceProviderAttribute.cs");
                ctx.AddSource("ServiceProviderAttribute.g.cs", SourceText.From(serviceProviderAttributeSource, Encoding.UTF8));
            });

            var servicesProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    static (syntax, _) => syntax is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                    static (ctx, _) => TransformService(ctx))
                .Where(static model => model is not null)
                .Collect();

            var targetClassProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                    static (ctx, _) =>
                    {
                        var classDecl = (ClassDeclarationSyntax)ctx.Node;
                        if (ctx.SemanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol typeSymbol)
                        {
                            return null;
                        }

                        var hasAttribute = typeSymbol.GetAttributes()
                            .Any(attr => attr.AttributeClass?.ToDisplayString() == ServiceProviderAttributeName);

                        if (hasAttribute)
                        {
                            return new TargetClassInfo(typeSymbol.ContainingNamespace.ToDisplayString(), typeSymbol.Name);
                        }

                        return null;
                    })
                .Where(static model => model is not null);

            var combinedProvider = servicesProvider.Combine(targetClassProvider.Collect());

            context.RegisterSourceOutput(combinedProvider, (spc, source) =>
                GenerateSource(spc, source.Left, source.Right!));
        }

        private static ServiceInfo? TransformService(GeneratorSyntaxContext ctx)
        {
            if (ctx.Node is not ClassDeclarationSyntax classDecl ||
                ctx.SemanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol typeSymbol ||
                typeSymbol.IsAbstract)
            {
                return null;
            }

            var serviceAttribute = typeSymbol.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == ServiceAttributeName);

            if (serviceAttribute == null)
            {
                return null;
            }

            if (serviceAttribute.ConstructorArguments.Length != 1)
            {
                return null;
            }

            var lifetimeValue = (int)serviceAttribute.ConstructorArguments[0].Value!;

            string lifetimeString = lifetimeValue switch
            {
                0 => "Singleton",
                1 => "Scoped",
                2 => "Transient",
                _ => "Transient"
            };

            return new ServiceInfo(typeSymbol.ToDisplayString(), null, lifetimeString);
        }

        private static void GenerateSource(SourceProductionContext spc, ImmutableArray<ServiceInfo?> services, ImmutableArray<TargetClassInfo?> targets)
        {
            if (targets.IsDefaultOrEmpty || targets.Length == 0)
            {
                // No class was marked with the attribute, so we don't generate anything.
                return;
            }

            if (targets.Length > 1)
            {
                Report(spc, "SPG001", "Multiple ServiceProvider classes",
                       $"Only one class can be decorated with the [ServiceProvider] attribute.",
                       DiagnosticSeverity.Error);
                return;
            }

            var target = targets[0]!;

            string templateContent;
            try
            {
                templateContent = GetEmbeddedResource("ServiceProvider.scriban-cs");
            }
            catch (FileNotFoundException ex)
            {
                Report(spc, "SPG102", "Template file not found", ex.Message, DiagnosticSeverity.Error);
                return;
            }

            var template = Template.Parse(templateContent);
            if (template.HasErrors)
            {
                var errors = string.Join("\n", template.Messages.Select(m => m.Message));
                Report(spc, "SPG103", "Scriban template parsing error", $"The Scriban template has errors:\n{errors}", DiagnosticSeverity.Error);
                return;
            }

            var model = new
            {
                Namespace = target.Namespace,
                ClassName = target.ClassName,
                Services = services.Where(s => s is not null).ToList()
            };

            var result = template.Render(model, member => member.Name);
            spc.AddSource("ServiceProvider.g.cs", result);
        }

        private static string GetEmbeddedResource(string resourceName)
        {
            var assembly = typeof(ServiceProviderGenerator).Assembly;
            var fullResourceName = $"AvaloniaXKCD.Generators.{resourceName}";

            using var stream = assembly.GetManifestResourceStream(fullResourceName);
            if (stream == null)
            {
                throw new FileNotFoundException($"Embedded resource '{fullResourceName}' not found.");
            }
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static void Report(SourceProductionContext spc, string id, string title, string message, DiagnosticSeverity severity)
        {
            spc.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(id, title, message, "Generator", severity, true), null));
        }
    }
}