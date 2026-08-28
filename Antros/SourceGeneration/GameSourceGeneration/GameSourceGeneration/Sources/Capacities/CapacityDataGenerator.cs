using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GameSourceGeneration.Capacities;

[Generator]
public class CapacityDataGenerator : IIncrementalGenerator
{
    private const string WithStepAttributeName = "ATCG.Capacities.Attributs.WithStepAttribute";

    // ---- Diagnostics (aligned on the HTX scheme) -------------------------

    // Empty or whitespace step name.
    private static readonly DiagnosticDescriptor HTX010 = new(
        id: "HTX010",
        title: "Invalid step name",
        messageFormat: "[WithStep] on '{0}' has an empty or whitespace step name",
        category: "Capacities",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // Two step names collapse to the same C# identifier.
    private static readonly DiagnosticDescriptor HTX011 = new(
        id: "HTX011",
        title: "Step identifier collision",
        messageFormat: "Step names '{0}' and '{1}' on '{2}' both map to identifier '{3}'",
        category: "Capacities",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // Duplicate identical step name.
    private static readonly DiagnosticDescriptor HTX012 = new(
        id: "HTX012",
        title: "Duplicate step name",
        messageFormat: "Step name '{0}' is declared more than once on '{1}'",
        category: "Capacities",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<CapacityDataPartialFileInfos?> infos = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                WithStepAttributeName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => GetInfos(ctx))
            .Where(static info => info is not null);

        context.RegisterSourceOutput(infos, static (ctx, info) => Emit(ctx, info!));
    }

    private static CapacityDataPartialFileInfos? GetInfos(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol type)
            return null;

        // Order of declaration == order of the attributes; preserved by Roslyn.
        List<string> stepNames = new();
        foreach (AttributeData attr in ctx.Attributes)
        {
            if (attr.ConstructorArguments.Length == 0)
                continue;
            if (attr.ConstructorArguments[0].Value is string s)
                stepNames.Add(s);
        }

        return new CapacityDataPartialFileInfos
        {
            className = type.Name,
            @namespace = type.ContainingNamespace.IsGlobalNamespace
                ? null
                : type.ContainingNamespace.ToDisplayString(),
            steps = stepNames.ToArray(),
            isCapacity = DerivesFromCapacityData(type)
        };
    }

    private static bool DerivesFromCapacityData(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? b = type.BaseType; b != null; b = b.BaseType)
            if (b.ToDisplayString() == "ATCG.Capacities.CapacityData")
                return true;
        return false;
    }

    private static void Emit(SourceProductionContext ctx, CapacityDataPartialFileInfos info)
    {
        // --- validate + build (name -> identifier) ------------------------
        Dictionary<string, string> identifierByName = new();
        Dictionary<string, string> nameByIdentifier = new();
        List<(string name, string identifier)> ordered = new();

        foreach (string name in info.steps)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(HTX010, Location.None, info.className));
                continue;
            }

            if (identifierByName.ContainsKey(name))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(HTX012, Location.None, name, info.className));
                continue;
            }

            string identifier = ToIdentifier(name);

            if (nameByIdentifier.TryGetValue(identifier, out string existingName))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    HTX011, Location.None, existingName, name, info.className, identifier));
                continue;
            }

            identifierByName[name] = identifier;
            nameByIdentifier[identifier] = name;
            ordered.Add((name, identifier));
        }

        if (ordered.Count == 0)
            return;

        // --- emit ---------------------------------------------------------
        // Unity runs C# 9 (.NET Standard 2.1): NO file-scoped namespace,
        // NO global-using. Emit a classic braced namespace. The generator's
        // OWN source may use newer syntax; only this EMITTED text must be C# 9.
        bool hasNamespace = !string.IsNullOrEmpty(info.@namespace);
        string indent = hasNamespace ? "        " : "    ";   // members
        string typeIndent = hasNamespace ? "    " : "";        // partial class

        StringBuilder sb = new();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine();

        if (hasNamespace)
        {
            sb.Append("namespace ").AppendLine(info.@namespace);
            sb.AppendLine("{");
        }

        sb.Append(typeIndent).Append("partial class ").AppendLine(info.className);
        sb.Append(typeIndent).AppendLine("{");

        // const names
        foreach ((string name, string identifier) in ordered)
            sb.Append(indent).Append("public const string ").Append(identifier)
              .Append(" = \"").Append(Escape(name)).AppendLine("\";");

        sb.AppendLine();

        // Ordered declared steps — OVERRIDES the CutsceneDefinition base so runtime and editor share
        // the same ordered list, instead of a separate static member that hides the base property.
        sb.Append(indent).Append("public override global::System.Collections.Generic.IReadOnlyList<string> DeclaredSteps { get; } = new string[] { ");
        sb.Append(string.Join(", ", ordered.Select(o => identifierByName[o.name])));
        sb.AppendLine(" };");

        // Data-driven part (per-step StepData fields + MapSteps) is emitted only for capacities.
        // Other cutscene kinds (attacks, deploys) just get the declared step names above.
        if (info.isCapacity)
        {
            sb.AppendLine();

            // serialized per-step fields, filled by the editor tools (ReadOnly).
            foreach ((string name, string identifier) in ordered)
            {
                sb.Append(indent).AppendLine("[field: global::UnityEngine.SerializeField, global::Sirenix.OdinInspector.BoxGroup(\"Steps\"), global::Sirenix.OdinInspector.ReadOnly]");
                sb.Append(indent).Append("public global::ATCG.Capacities.CapacityStepData ")
                  .Append(identifier).AppendLine("StepData { get; private set; }");
            }

            sb.AppendLine();

            // generated mapping: base clears the map then calls MapSteps().
            sb.Append(indent).AppendLine("protected override void MapSteps(global::System.Collections.Generic.Dictionary<string, global::ATCG.Capacities.CapacityStepData> map)");
            sb.Append(indent).AppendLine("{");
            foreach ((string name, string identifier) in ordered)
            {
                sb.Append(indent).Append("    map[").Append(identifier).Append("] = ")
                  .Append(identifier).AppendLine("StepData;");
            }
            sb.Append(indent).AppendLine("}");
        }

        sb.Append(typeIndent).AppendLine("}");

        if (hasNamespace)
            sb.AppendLine("}");

        ctx.AddSource($"{info.className}.Steps.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    // "Before Explosion" -> "BeforeExplosion". Strips non-identifier chars,
    // PascalCases word boundaries, prefixes '_' if it would start with a digit.
    private static string ToIdentifier(string raw)
    {
        StringBuilder sb = new(raw.Length);
        bool upperNext = true;
        foreach (char c in raw)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(upperNext ? char.ToUpperInvariant(c) : c);
                upperNext = false;
            }
            else
            {
                upperNext = true; // word boundary
            }
        }

        if (sb.Length == 0)
            return "_";
        if (char.IsDigit(sb[0]))
            sb.Insert(0, '_');
        return sb.ToString();
    }

    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}