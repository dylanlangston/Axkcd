using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace AvaloniaXKCD.Generators
{
    [Generator]
    public class ViewLocatorGenerator : IIncrementalGenerator
    {
        private const string AttributeName = "AvaloniaXKCD.Generators.ViewLocatorAttribute";

        public record DiscoveredType(string FullTypeName, string ClassName, string Suffix);

        public record ViewModelViewPair(string ViewModel, string View);

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
                var attributeSource = GetEmbeddedResource("Resources.ViewLocatorAttribute.cs");
                ctx.AddSource("ViewLocatorAttribute.g.cs", SourceText.From(attributeSource, Encoding.UTF8));
            });

            var typesProvider = context
                .SyntaxProvider.CreateSyntaxProvider(
                    static (syntax, _) => syntax is ClassDeclarationSyntax,
                    static (ctx, _) => TransformClass(ctx)
                )
                .Where(static model => model is not null);

            var targetClassProvider = context
                .SyntaxProvider.CreateSyntaxProvider(
                    static (node, _) => node is ClassDeclarationSyntax c && c.AttributeLists.Count > 0,
                    static (ctx, _) =>
                    {
                        var classDecl = (ClassDeclarationSyntax)ctx.Node;
                        if (ctx.SemanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol typeSymbol)
                        {
                            return null;
                        }

                        var hasAttribute = typeSymbol
                            .GetAttributes()
                            .Any(attr => attr.AttributeClass?.ToDisplayString() == AttributeName);

                        if (hasAttribute)
                        {
                            return new TargetClassInfo(
                                typeSymbol.ContainingNamespace.ToDisplayString(),
                                typeSymbol.Name
                            );
                        }

                        return null;
                    }
                )
                .Where(static model => model is not null);

            var combinedProvider = typesProvider.Collect().Combine(targetClassProvider.Collect());

            context.RegisterSourceOutput(
                combinedProvider,
                (spc, source) => GenerateSource(spc, source.Left, source.Right)
            );
        }

        private static DiscoveredType? TransformClass(GeneratorSyntaxContext ctx)
        {
            if (
                ctx.Node is not ClassDeclarationSyntax classDecl
                || ctx.SemanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol typeSymbol
                || typeSymbol.IsAbstract
            )
            {
                return null;
            }

            var fullTypeName = typeSymbol.ToDisplayString();
            var className = typeSymbol.Name;

            if (className.EndsWith("ViewModel"))
            {
                return new DiscoveredType(fullTypeName, className, "ViewModel");
            }

            if (className.EndsWith("View"))
            {
                return new DiscoveredType(fullTypeName, className, "View");
            }

            return null;
        }

        private static void GenerateSource(
            SourceProductionContext spc,
            ImmutableArray<DiscoveredType?> types,
            ImmutableArray<TargetClassInfo?> targets
        )
        {
            if (targets.IsDefaultOrEmpty || targets.Length == 0)
            {
                // No class was marked with the attribute, so we don't generate anything.
                return;
            }

            if (targets.Length > 1)
            {
                Report(
                    spc,
                    "XKC001",
                    "Multiple ViewLocator classes",
                    $"Only one class can be decorated with the [ViewLocator] attribute.",
                    DiagnosticSeverity.Error
                );
                return;
            }

            var target = targets[0]!;

            var validTypes = types.Where(t => t is not null).ToList();

            var viewModels = validTypes.Where(t => t!.Suffix == "ViewModel").ToList();

            var viewsByBaseName = validTypes
                .Where(t => t!.Suffix == "View")
                .ToDictionary(t => t!.ClassName.Substring(0, t.ClassName.Length - "View".Length), t => t!.FullTypeName);

            var pairs = new List<ViewModelViewPair>();
            foreach (var viewModel in viewModels)
            {
                var viewModelBaseName = viewModel!.ClassName.Substring(
                    0,
                    viewModel.ClassName.Length - "ViewModel".Length
                );

                if (viewsByBaseName.TryGetValue(viewModelBaseName, out var matchedViewFullName))
                {
                    pairs.Add(new ViewModelViewPair(viewModel.FullTypeName, matchedViewFullName));
                }
                else
                {
                    var expectedViewName = viewModel.FullTypeName.Replace("ViewModel", "View");
                    Report(
                        spc,
                        "XKC101",
                        "Orphaned ViewModel",
                        $"ViewModel '{viewModel.FullTypeName}' was found, but a corresponding View like '{expectedViewName}' was not.",
                        DiagnosticSeverity.Info
                    );
                }
            }

            string templateContent;
            try
            {
                templateContent = GetEmbeddedResource("ViewLocator.scriban-cs");
            }
            catch (FileNotFoundException ex)
            {
                Report(spc, "XKC102", "Template file not found", ex.Message, DiagnosticSeverity.Error);
                return;
            }

            var template = Template.Parse(templateContent);
            if (template.HasErrors)
            {
                var errors = string.Join("\n", template.Messages.Select(m => m.Message));
                Report(
                    spc,
                    "XKC103",
                    "Scriban template parsing error",
                    $"The Scriban template has errors:\n{errors}",
                    DiagnosticSeverity.Error
                );
                return;
            }

            var model = new
            {
                Namespace = target.Namespace,
                ClassName = target.ClassName,
                Pairs = pairs,
            };

            var result = template.Render(model, member => member.Name);
            spc.AddSource("ViewLocator.g.cs", result);
        }

        private static string GetEmbeddedResource(string resourceName)
        {
            var assembly = typeof(ViewLocatorGenerator).Assembly;
            var fullName = $"AvaloniaXKCD.Generators.{resourceName}";

            using var stream =
                assembly.GetManifestResourceStream(fullName)
                ?? throw new FileNotFoundException(
                    $"Embedded resource '{fullName}' not found. Available: {string.Join(", ", assembly.GetManifestResourceNames())}"
                );

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static void Report(
            SourceProductionContext spc,
            string id,
            string title,
            string message,
            DiagnosticSeverity severity
        )
        {
            spc.ReportDiagnostic(
                Diagnostic.Create(new DiagnosticDescriptor(id, title, message, "Generator", severity, true), null)
            );
        }
    }
}
