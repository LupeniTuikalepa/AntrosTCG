using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ATCG.Capacities.Generators
{
    /// <summary>
    /// Runs on the capacity assembly. For every struct/class implementing
    /// ICapacity&lt;TData&gt;, reads TData's [WithStep] attributes (cross-assembly, from
    /// metadata) and generates GetSteps() + partial ExecuteXxx declarations, with a
    /// diagnostic (HTX020) localized on the capacity when an impl is missing.
    /// </summary>
    [Generator]
    public sealed class CapacityRunGenerator : IIncrementalGenerator
    {
        // Interface identity (metadata name + namespace) used to detect capacities.
        private const string CAPACITY_INTERFACE_METADATA_NAME = "ICapacity`1";
        private const string CAPACITY_INTERFACE_NAMESPACE = "ATCG.Battle.CapacitySystem.Core";

        // Attribute (short symbol name) carrying the step names on the data type.
        private const string WITH_STEP_ATTRIBUTE_NAME = "WithStepAttribute";

        // Namespaces emitted as usings in the generated file.
        private const string CAPACITIES_NAMESPACE = "ATCG.Battle.CapacitySystem.Capacities";
        private const string CORE_NAMESPACE = "ATCG.Battle.CapacitySystem.Core";

        private static readonly DiagnosticDescriptor MissingImplementation = new(
            id: "HTX020",
            title: "Capacity step implementation missing",
            messageFormat: "Capacity '{0}' declares step '{1}' (via {2}) but does not implement '{3}'",
            category: "ATCG.Capacities",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<CapacityModel> models =
                context.SyntaxProvider.CreateSyntaxProvider(
                        predicate: static (node, _) => IsCandidateType(node),
                        transform: static (ctx, _) => BuildModel(ctx))
                    .Where(static m => m is not null)
                    .Select(static (m, _) => m!.Value);

            context.RegisterSourceOutput(models, Emit);
        }

        // Cheap syntactic filter: struct or class declarations with a base list.
        private static bool IsCandidateType(SyntaxNode node)
        {
            return node is StructDeclarationSyntax { BaseList: not null }
                || node is ClassDeclarationSyntax { BaseList: not null };
        }

        // Detects ICapacity<TData> by metadata name + namespace on the implemented
        // interfaces, reads TData's [WithStep] names, and flags missing ExecuteXxx.
        private static CapacityModel? BuildModel(GeneratorSyntaxContext ctx)
        {
            if (ctx.Node is not TypeDeclarationSyntax typeDecl)
                return null;

            if (ctx.SemanticModel.GetDeclaredSymbol(typeDecl) is not INamedTypeSymbol typeSymbol)
                return null;

            INamedTypeSymbol capacityInterface = typeSymbol.AllInterfaces.FirstOrDefault(
                i => i.MetadataName == CAPACITY_INTERFACE_METADATA_NAME &&
                     i.ContainingNamespace.ToDisplayString() == CAPACITY_INTERFACE_NAMESPACE);

            if (capacityInterface is null || capacityInterface.TypeArguments.Length != 1)
                return null;

            if (capacityInterface.TypeArguments[0] is not INamedTypeSymbol dataSymbol)
                return null;

            List<string> stepNames = new();
            foreach (AttributeData attr in dataSymbol.GetAttributes())
            {
                if (attr.AttributeClass?.Name != WITH_STEP_ATTRIBUTE_NAME)
                    continue;
                if (attr.ConstructorArguments.Length < 1)
                    continue;
                if (attr.ConstructorArguments[0].Value is string s && !string.IsNullOrEmpty(s))
                    stepNames.Add(s);
            }

            if (stepNames.Count == 0)
                return null;

            HashSet<string> implemented = new();
            foreach (ISymbol member in typeSymbol.GetMembers())
            {
                if (member is not IMethodSymbol method)
                    continue;

                bool isImpl = method.PartialImplementationPart != null || !method.IsPartialDefinition;
                if (isImpl)
                    implemented.Add(method.Name);
            }

            List<StepEntry> entries = new();
            foreach (string name in stepNames)
            {
                string identifier = ToIdentifier(name);
                string methodName = "Execute" + identifier;
                entries.Add(new StepEntry(name, identifier, methodName, implemented.Contains(methodName)));
            }

            return new CapacityModel(
                typeSymbol.ContainingNamespace.ToDisplayString(),
                typeSymbol.Name,
                typeDecl is StructDeclarationSyntax,
                dataSymbol.ToDisplayString(),
                dataSymbol.Name,
                entries.ToImmutableArray(),
                typeSymbol.Locations.FirstOrDefault());
        }

        private static void Emit(SourceProductionContext ctx, CapacityModel model)
        {
            foreach (StepEntry entry in model.Steps)
            {
                if (!entry.Implemented)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        MissingImplementation,
                        model.Location,
                        model.TypeName, entry.StepName, model.DataDisplayName, entry.MethodName));
                }
            }

            StringBuilder sb = new();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine($"using {CAPACITIES_NAMESPACE};");
            sb.AppendLine($"using {CORE_NAMESPACE};");
            sb.AppendLine();
            sb.AppendLine($"namespace {model.Namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public partial {(model.IsStruct ? "struct" : "class")} {model.TypeName}");
            sb.AppendLine("    {");

            sb.AppendLine($"        public ICapacityStep[] GetSteps({model.DataDisplayName} data, CastCapacityPhase phase) => new ICapacityStep[]");
            sb.AppendLine("        {");
            foreach (StepEntry entry in model.Steps)
            {
                sb.AppendLine(
                    $"            new CapacityStep<{model.DataDisplayName}>(data, {entry.MethodName}, {model.DataDisplayName}.{entry.Identifier}),");
            }
            sb.AppendLine("        };");
            sb.AppendLine();

            foreach (StepEntry entry in model.Steps)
            {
                sb.AppendLine(
                    $"        private partial void {entry.MethodName}({model.DataDisplayName} data, CapacityStepContext ctx);");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            ctx.AddSource($"{model.TypeName}.Steps.g.cs", sb.ToString());
        }

        // Step name -> PascalCase identifier (matches the data-side constant naming).
        private static string ToIdentifier(string name)
        {
            StringBuilder sb = new();
            bool upperNext = true;
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(upperNext ? char.ToUpperInvariant(c) : c);
                    upperNext = false;
                }
                else
                {
                    upperNext = true;
                }
            }
            return sb.ToString();
        }

        private readonly struct StepEntry
        {
            public readonly string StepName;
            public readonly string Identifier;
            public readonly string MethodName;
            public readonly bool Implemented;

            public StepEntry(string stepName, string identifier, string methodName, bool implemented)
            {
                StepName = stepName;
                Identifier = identifier;
                MethodName = methodName;
                Implemented = implemented;
            }
        }

        private readonly struct CapacityModel
        {
            public readonly string Namespace;
            public readonly string TypeName;
            public readonly bool IsStruct;
            public readonly string DataDisplayName;
            public readonly string DataName;
            public readonly ImmutableArray<StepEntry> Steps;
            public readonly Location Location;

            public CapacityModel(string ns, string typeName, bool isStruct, string dataDisplayName,
                string dataName, ImmutableArray<StepEntry> steps, Location location)
            {
                Namespace = ns;
                TypeName = typeName;
                IsStruct = isStruct;
                DataDisplayName = dataDisplayName;
                DataName = dataName;
                Steps = steps;
                Location = location;
            }
        }
    }
}