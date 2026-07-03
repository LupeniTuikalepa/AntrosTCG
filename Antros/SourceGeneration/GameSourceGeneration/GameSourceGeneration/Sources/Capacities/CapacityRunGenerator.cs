using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GameSourceGeneration.Capacities
{
    /// <summary>
    /// Runs on the capacity assembly. For every struct/class implementing
    /// ICapacity&lt;TData&gt;, reads TData's [WithStep] attributes (cross-assembly, from
    /// metadata) and generates:
    ///  - an ICapacityStep[] Run(TData, CastCapacityPhase) returning one
    ///    CapacityStep&lt;TData&gt; per step, wired to an ExecuteXxx partial;
    ///  - a `partial void ExecuteXxx(TData, CapacityStepContext)` declaration per
    ///    step, so the method exists (IDE guidance) and Run can reference it;
    ///  - a diagnostic (HTX020) localized ON THE CAPACITY when an ExecuteXxx
    ///    implementation is missing, so the error shows in the user's file rather
    ///    than in the generated Run.
    ///
    /// NOTE (validate at compile time): the cross-assembly attribute read and the
    /// missing-implementation detection rely on Roslyn symbol APIs that could not be
    /// compiled in the authoring environment. Zones are marked below.
    /// </summary>
    [Generator]
    public sealed class CapacityRunGenerator : IIncrementalGenerator
    {
        private const string CAPACITY_INTERFACE_METADATA_NAME = "ATCG.Capacities.ICapacity`1";
        private const string WITH_STEP_ATTRIBUTE_NAME = "WithStepAttribute";
        private const string STEP_CONTEXT_TYPE = "ATCG.Battle.Commands.GameCommands.CapacityStepContext";
        private const string PHASE_TYPE = "ATCG.Battle.CapacitySystem.Core.CastCapacityPhase";
        private const string STEP_TYPE = "ATCG.Battle.CapacitySystem.Capacities.ICapacityStep";
        private const string CONCRETE_STEP_TYPE = "ATCG.Battle.CapacitySystem.Capacities.CapacityStep";

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

        // Cheap syntactic filter: struct or class declarations with a base list
        // (they might implement ICapacity<T>). The semantic check happens in transform.
        private static bool IsCandidateType(SyntaxNode node)
        {
            return node is StructDeclarationSyntax { BaseList: not null }
                || node is ClassDeclarationSyntax { BaseList: not null };
        }

        // Resolves the symbol, finds ICapacity<TData>, reads TData's [WithStep] names,
        // and detects which ExecuteXxx are already implemented by the user.
        private static CapacityModel? BuildModel(GeneratorSyntaxContext ctx)
        {
            if (ctx.Node is not TypeDeclarationSyntax typeDecl)
                return null;

            if (ctx.SemanticModel.GetDeclaredSymbol(typeDecl) is not INamedTypeSymbol typeSymbol)
                return null;

            // --- ZONE TO VALIDATE: find ICapacity<TData> among interfaces ---
            INamedTypeSymbol capacityInterface = typeSymbol.AllInterfaces.FirstOrDefault(
                i => i.IsGenericType &&
                     i.ConstructedFrom.ToDisplayString().StartsWith("ATCG.Capacities.ICapacity<"));

            if (capacityInterface is null || capacityInterface.TypeArguments.Length != 1)
                return null;

            if (capacityInterface.TypeArguments[0] is not INamedTypeSymbol dataSymbol)
                return null;

            // --- ZONE TO VALIDATE: read [WithStep("...")] from TData (metadata) ---
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

            // Which ExecuteXxx methods does the USER already implement? We look at
            // the type's own members (the user's partial parts + generated parts).
            // A partial with an implementation has PartialImplementationPart != null;
            // a plain method is implemented by definition.
            HashSet<string> implemented = new();
            foreach (ISymbol member in typeSymbol.GetMembers())
            {
                if (member is not IMethodSymbol method)
                    continue;

                bool isImpl = method.PartialImplementationPart != null || !method.IsPartialDefinition;
                if (isImpl)
                    implemented.Add(method.Name);
            }

            // If the user has already implemented Run(), extract the actual method names used
            // This allows flexible naming conventions (Execute*, Apply*, etc.)
            HashSet<string> methodsUsedInRun = ExtractMethodsUsedInRun(typeSymbol, dataSymbol);
            if (methodsUsedInRun.Count > 0)
            {
                foreach (var methodName in methodsUsedInRun)
                    implemented.Add(methodName);
            }

            // Build per-step entries: name -> ExecuteXxx identifier.
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
            // Diagnostics for missing implementations, localized on the capacity.
            foreach (StepEntry entry in model.steps)
            {
                if (!entry.implemented)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        MissingImplementation,
                        model.location,
                        model.typeName, entry.stepName, model.dataDisplayName, entry.methodName));
                }
            }

            StringBuilder sb = new();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("using ATCG.Battle.CapacitySystem.Capacities;");
            sb.AppendLine("using ATCG.Battle.CapacitySystem.Core;");
            sb.AppendLine();
            sb.AppendLine($"namespace {model.@namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public partial {(model.isStruct ? "struct" : "class")} {model.typeName}");
            sb.AppendLine("    {");

            // Run() returning the array.
            sb.AppendLine($"        public ICapacityStep[] Run({model.dataDisplayName} data, CastCapacityPhase phase)");
            sb.AppendLine("        {");
            sb.AppendLine("            return new ICapacityStep[]");
            sb.AppendLine("            {");
            foreach (StepEntry entry in model.steps)
            {
                sb.AppendLine(
                    $"                new CapacityStep<{model.dataDisplayName}>(data, {entry.methodName}, {model.dataName}.{entry.identifier}),");
            }
            sb.AppendLine("            };");
            sb.AppendLine("        }");
            sb.AppendLine();

            // Partial declarations (exist for IDE guidance; Run references them).
            foreach (StepEntry entry in model.steps)
            {
                sb.AppendLine(
                    $"        partial void {entry.methodName}({model.dataDisplayName} data, CapacityStepContext ctx);");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            ctx.AddSource($"{model.typeName}.Run.g.cs", sb.ToString());
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

        // Extract method names used in an existing Run() implementation.
        // This allows detection of capacity step implementations regardless of naming convention.
        private static HashSet<string> ExtractMethodsUsedInRun(INamedTypeSymbol typeSymbol, INamedTypeSymbol dataSymbol)
        {
            var result = new HashSet<string>();

            // Find the Run method in the type's members
            IMethodSymbol? runMethod = typeSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m => m.Name == "Run" && m.Parameters.Length == 2);

            if (runMethod?.DeclaringSyntaxReferences.Length > 0)
            {
                var syntaxRef = runMethod.DeclaringSyntaxReferences.FirstOrDefault();
                if (syntaxRef?.GetSyntax() is MethodDeclarationSyntax methodSyntax && methodSyntax.Body != null)
                {
                    // Walk the method body and find all invocations that look like callback methods
                    // Look for patterns like: new CapacityStep<T>(data, MethodName, ...)
                    var invocations = methodSyntax.Body.DescendantNodes().OfType<ArgumentListSyntax>();
                    
                    foreach (var argList in invocations)
                    {
                        // Check if this looks like CapacityStep constructor (has 3 args and one is an identifier)
                        if (argList.Arguments.Count >= 2)
                        {
                            // The second argument is typically the method reference
                            var secondArg = argList.Arguments.ElementAtOrDefault(1)?.Expression;
                            if (secondArg is IdentifierNameSyntax identifier)
                            {
                                result.Add(identifier.Identifier.Text);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private readonly struct StepEntry
        {
            public readonly string stepName;
            public readonly string identifier;
            public readonly string methodName;
            public readonly bool implemented;

            public StepEntry(string stepName, string identifier, string methodName, bool implemented)
            {
                this.stepName = stepName;
                this.identifier = identifier;
                this.methodName = methodName;
                this.implemented = implemented;
            }
        }

        private readonly struct CapacityModel
        {
            public readonly string @namespace;
            public readonly string typeName;
            public readonly bool isStruct;
            public readonly string dataDisplayName;
            public readonly string dataName;
            public readonly ImmutableArray<StepEntry> steps;
            public readonly Location location;

            public CapacityModel(string ns, string typeName, bool isStruct, string dataDisplayName,
                string dataName, ImmutableArray<StepEntry> steps, Location location)
            {
                @namespace = ns;
                this.typeName = typeName;
                this.isStruct = isStruct;
                this.dataDisplayName = dataDisplayName;
                this.dataName = dataName;
                this.steps = steps;
                this.location = location;
            }
        }
    }
}