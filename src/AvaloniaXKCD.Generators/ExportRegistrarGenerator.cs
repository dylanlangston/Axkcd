using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;

namespace AvaloniaXKCD.Generators
{
    [Generator]
    public class ExportRegistrarGenerator : IIncrementalGenerator
    {
        private const string IExportFullName = "AvaloniaXKCD.Exports.IExport";

        // Models for the Scriban template
        public record ConstructorDependency(string Type);

        public record ExportedType(
            string FullTypeName,
            List<string> Interfaces,
            List<ConstructorDependency> ConstructorDependencies
        );

        public record ExportedInterface(string Name, List<ExportedType> Implementations);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
            var exportsProvider = context
                .SyntaxProvider.CreateSyntaxProvider(
                    static (syntax, _) => syntax is ClassDeclarationSyntax { BaseList: not null },
                    static (ctx, _) => TransformClass(ctx)
                )
                .Where(static model => model is not null);

            context.RegisterSourceOutput(exportsProvider.Collect(), GenerateSource);
        }

        private static ExportedType? TransformClass(GeneratorSyntaxContext ctx)
        {
            if (ctx.Node is not ClassDeclarationSyntax classDecl)
                return null;

            if (
                ctx.SemanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol typeSymbol
                || typeSymbol.IsAbstract
            )
                return null;

            var iExportInterface = ctx.SemanticModel.Compilation.GetTypeByMetadataName(IExportFullName);
            if (iExportInterface == null)
            {
                return new ExportedType("IEXPORT_NOT_FOUND", new(), new());
            }

            // Ensure class implements IExport
            if (!typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, iExportInterface)))
                return null;

            // Find interfaces that derive from IExport (but are not IExport itself)
            var exportInterfaces = typeSymbol
                .AllInterfaces.Where(i =>
                    !SymbolEqualityComparer.Default.Equals(i, iExportInterface)
                    && (
                        i.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(x, iExportInterface))
                        || i.AllInterfaces.Any(x => x.ToDisplayString() == IExportFullName)
                        || i.ToDisplayString() == IExportFullName
                    )
                )
                .Select(i => i.ToDisplayString())
                .Distinct()
                .ToList();

            if (exportInterfaces.Count == 0)
                return null;

            var constructor = typeSymbol
                .InstanceConstructors.OrderByDescending(c => c.Parameters.Length)
                .FirstOrDefault();

            var deps =
                constructor?.Parameters.Select(p => new ConstructorDependency(p.Type.ToDisplayString())).ToList()
                ?? new();

            return new ExportedType(typeSymbol.ToDisplayString(), exportInterfaces, deps);
        }

        private static void GenerateSource(SourceProductionContext spc, ImmutableArray<ExportedType?> exports)
        {
            if (exports.IsDefaultOrEmpty)
            {
                Report(
                    spc,
                    "XKC001",
                    "No exportable classes found",
                    "The ExportRegistrarGenerator did not find any classes implementing IExport.",
                    DiagnosticSeverity.Info
                );
                return;
            }

            if (exports.Any(e => e?.FullTypeName == "IEXPORT_NOT_FOUND"))
            {
                Report(
                    spc,
                    "XKC002",
                    "IExport interface not found",
                    $"The required interface '{IExportFullName}' was not found in the compilation.",
                    DiagnosticSeverity.Warning
                );
                return;
            }

            var validExports = exports.Where(e => e is not null).ToList();
            if (validExports.Count == 0)
            {
                Report(
                    spc,
                    "XKC003",
                    "No valid exports found",
                    "The ExportRegistrarGenerator found IExport but no concrete classes implementing it with service interfaces.",
                    DiagnosticSeverity.Info
                );
                return;
            }

            var exportsByInterface = validExports
                .SelectMany(e => e!.Interfaces.Select(i => new { Interface = i, Impl = e }))
                .GroupBy(x => x.Interface)
                .Select(g => new ExportedInterface(g.Key, g.Select(x => x.Impl!).ToList()))
                .ToList();

            string templateContent;
            try
            {
                templateContent = GetEmbeddedResource("ExportRegistrar.scriban-cs");
            }
            catch (FileNotFoundException ex)
            {
                Report(spc, "XKC004", "Template file not found", ex.Message, DiagnosticSeverity.Error);
                return;
            }

            var template = Template.Parse(templateContent);
            if (template.HasErrors)
            {
                var errors = string.Join("\n", template.Messages.Select(m => m.Message));
                Report(
                    spc,
                    "XKC005",
                    "Scriban template parsing error",
                    $"The Scriban template has errors:\n{errors}",
                    DiagnosticSeverity.Error
                );
                return;
            }

            // Directly render using a strongly typed model
            var model = new { Interfaces = exportsByInterface };
            var result = template.Render(model, member => member.Name);

            spc.AddSource("ExportRegistrar.g.cs", result);
        }

        private static string GetEmbeddedResource(string resourceName)
        {
            var assembly = typeof(ExportRegistrarGenerator).Assembly;
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
