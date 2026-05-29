using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GameSourceGeneration
{
    [Generator]
    public class EntityAspectImplementationGenerator : IIncrementalGenerator
    {
        private const string GetAspectMethodName = "GetAspect";
        private const string TryGetAspectMethodName = "TryGetAspect";
        private const string CreateAspectMethodName = "CreateAspect";
        private const string ComponentsFactoryClassName = "ComponentsFactory";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<AspectInfo> aspects = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is StructDeclarationSyntax
                    {
                        BaseList: not null,
                        Modifiers: var mods
                    } && mods.Any(m => m.Text == "partial"),
                    transform: static (ctx, _) => GetAspectInfo(ctx))
                .Where(static info => info != null)!;

            context.RegisterSourceOutput(aspects, static (ctx, info) =>
            {
                if (info.factoryInfosTypes != null && info.structSymbol != null)
                {
                    bool isImplemented = info.structSymbol
                        .GetMembers("CreateComponents")
                        .OfType<IMethodSymbol>()
                        .Any(m => !m.IsPartialDefinition);

                    if (!isImplemented)
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(
                            MissingCreateComponents,
                            info.structLocation,
                            info.structName,
                            info.factoryInfosTypes
                        ));
                    }
                }

                string source = GenerateAspectImplementation(info);
                ctx.AddSource($"{info.structName}.g.cs", SourceText.From(source, Encoding.UTF8));
            });
        }

        private static AspectInfo? GetAspectInfo(GeneratorSyntaxContext ctx)
        {
            StructDeclarationSyntax structDecl = (StructDeclarationSyntax)ctx.Node;

            AspectInfo aspectInfo = null;
            SeparatedSyntaxList<BaseTypeSyntax> types = structDecl.BaseList!.Types;
            foreach (BaseTypeSyntax baseType in types)
            {
                if (baseType.Type is GenericNameSyntax { Identifier.Text: "IEntityAspect" } generic)
                {
                    var typeArguments = generic.TypeArgumentList.Arguments
                        .Select(arg => arg.ToString())
                        .Distinct()
                        .ToList();

                    if (typeArguments.Count == 0)
                        continue;

                    string namespaceName = GetNamespace(structDecl);

                    // Récupère la racine du fichier pour extraire les usings
                    var compilationUnit = structDecl.SyntaxTree.GetRoot() as CompilationUnitSyntax;
                    var usings = compilationUnit?.Usings
                        .Select(u => u.ToString()) // "using ATCG.Battle.Cards;"
                        .ToList() ?? new List<string>();


                    aspectInfo ??= new AspectInfo();
                    aspectInfo.structName = structDecl.Identifier.Text;
                    aspectInfo.@namespace = namespaceName;
                    aspectInfo.componentTypes = typeArguments;
                    aspectInfo.usings = usings;
                    aspectInfo.structLocation = structDecl.Identifier.GetLocation();
                    aspectInfo.structSymbol = ctx.SemanticModel.GetDeclaredSymbol(structDecl) as INamedTypeSymbol;
                }

                if (baseType.Type is GenericNameSyntax { Identifier.Text: "ICreateEntityAspect" } genericNameSyntax)
                {
                    var typeArguments = genericNameSyntax.TypeArgumentList.Arguments
                        .Select(arg => arg.ToString()).First();

                    aspectInfo ??= new AspectInfo();
                    aspectInfo.factoryInfosTypes = typeArguments;

                }
            }

            return aspectInfo;
        }

        private static string GetNamespace(SyntaxNode node)
        {
            SyntaxNode? current = node.Parent;
            while (current != null)
            {
                if (current is NamespaceDeclarationSyntax ns)
                    return ns.Name.ToString();
                if (current is FileScopedNamespaceDeclarationSyntax fns)
                    return fns.Name.ToString();
                current = current.Parent;
            }

            return string.Empty;
        }

        private static string GenerateAspectImplementation(AspectInfo info)
        {
            var sb = new StringBuilder();
            //sb.AppendLine("// <auto-generated/>");
            sb.AppendLine();

            // Usings en tête de fichier
            foreach (var u in info.usings)
                sb.AppendLine(u);

            if (info.usings.Count > 0)
                sb.AppendLine();

            if (!string.IsNullOrEmpty(info.@namespace))
            {
                sb.AppendLine($"namespace {info.@namespace}");
                sb.AppendLine("{");
            }

            sb.AppendLine($"    public static class {info.structName}Extensions");
            sb.AppendLine($"    {{");

            string structVariableName = FirstLetterLowerCase(info.structName);
            sb.AppendLine($"        public static bool Is{info.structName}(this EntityAddress address, out {info.structName} {structVariableName})");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            if(address.Is{info.structName}())");
            sb.AppendLine($"            {{");
            sb.AppendLine($"                {structVariableName} = {info.structName}.{GetAspectMethodName}(address);");
            sb.AppendLine($"                return true;");
            sb.AppendLine($"            }}");
            sb.AppendLine($"            {structVariableName} = default;");
            sb.AppendLine($"            return false;");
            sb.AppendLine($"        }}");
            sb.AppendLine();
            sb.AppendLine($"        public static bool Is{info.structName}(this EntityAddress address) => {info.structName}.IsAspect(address);");
            sb.AppendLine($"    }}");
            sb.AppendLine();

            sb.AppendLine($"    public partial struct {info.structName}");
            sb.AppendLine("    {");

            // Une propriété ref par composant
            foreach (var type in info.componentTypes)
            {
                sb.AppendLine($"        public ref {type} {type} => ref EntityAddress.GetComponent<{type}>();");
            }

            sb.AppendLine();
            sb.AppendLine("        public EntityAddress EntityAddress { get; }");
            sb.AppendLine();

            sb.AppendLine($"        public {info.structName}(EntityAddress entityAddress)");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            EntityAddress = entityAddress;");
            sb.AppendLine($"        }}");


            sb.AppendLine($"        public static bool IsAspect(EntityAddress address)");
            sb.AppendLine($"        {{");
            foreach (var type in info.componentTypes)
            {
                sb.AppendLine($"            if(!address.HasComponent<{type}>())");
                sb.AppendLine($"                return false;");
            }

            sb.AppendLine($"            return true;");
            sb.AppendLine($"        }}");

            sb.AppendLine();
            sb.AppendLine($"        public static {info.structName} {GetAspectMethodName}(EntityAddress entityAddress) => new {info.structName}(entityAddress);");

            if (info.factoryInfosTypes != null)
            {
                string componentsFactoryVariableName = FirstLetterLowerCase(ComponentsFactoryClassName);

                sb.AppendLine();
                sb.AppendLine("#region factory");

                sb.AppendLine($"        public ref struct {ComponentsFactoryClassName}");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            public readonly EntityAddress address;");
                sb.AppendLine();
                sb.AppendLine($"            public {ComponentsFactoryClassName}(EntityAddress address)");
                sb.AppendLine($"            {{");
                sb.AppendLine($"                this.address = address;");
                sb.AppendLine($"            }}");
                // Une propriété factory par composant
                foreach (var type in info.componentTypes)
                {
                    sb.AppendLine($"            public {type} {type}");
                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                get => address.GetComponent<{type}>();");
                    sb.AppendLine($"                set => address.AddOrSetComponent<{type}>(value);");
                    sb.AppendLine($"            }}");

                }

                sb.AppendLine($"        }}");

                string[] split = info.factoryInfosTypes.Split('.');
                string factoryInfosVariableName = FirstLetterLowerCase(split[split.Length - 1]);
                sb.AppendLine();

                sb.AppendLine($"        private static partial void CreateComponents(ref {ComponentsFactoryClassName} {componentsFactoryVariableName}, {info.factoryInfosTypes} {factoryInfosVariableName});");

                sb.AppendLine();
                sb.AppendLine($"        public static bool {TryGetAspectMethodName}(EntityAddress entityAddress, out {info.structName} {structVariableName})");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            if({info.structName}.IsAspect(entityAddress))");
                sb.AppendLine($"            {{");
                sb.AppendLine($"                {structVariableName} = {GetAspectMethodName}(entityAddress);");
                sb.AppendLine($"                return true;");
                sb.AppendLine($"            }}");
                sb.AppendLine($"            {structVariableName} = default;");
                sb.AppendLine($"            return false;");
                sb.AppendLine($"        }}");
                sb.AppendLine();
                sb.AppendLine($"        public static {info.structName} {CreateAspectMethodName}(World world, in {info.factoryInfosTypes} {factoryInfosVariableName})");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            Entity entity = world.CreateEntity();");
                sb.AppendLine($"            EntityAddress address = new EntityAddress(world, entity);");
                sb.AppendLine($"            var factory = new {ComponentsFactoryClassName}(address);");
                sb.AppendLine($"            CreateComponents(ref factory, {factoryInfosVariableName});");

                sb.AppendLine();

                foreach (var type in info.componentTypes)
                {
                    sb.AppendLine($"            if(!address.HasComponent<{type}>())");
                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                address.AddOrSetComponent<{type}>();");
                    sb.AppendLine($"                UnityEngine.Debug.Log(\"[EntitySystem] Adding default component of type {type} for newly created aspect of type {info.structName} because none was created inside CreateComponents method.\");");
                    sb.AppendLine($"            }}");

                }

                sb.AppendLine();
                sb.AppendLine($"            return new {info.structName}(address);");
                sb.AppendLine($"        }}");
                sb.AppendLine();

                sb.AppendLine("#endregion");
            }

            sb.AppendLine("    }");

            if (!string.IsNullOrEmpty(info.@namespace))
                sb.AppendLine("}");

            return sb.ToString();
        }

        private static string FirstLetterLowerCase(string text)
        {
            return char.ToLowerInvariant(text[0]) + text.Substring(1);
        }
        private static readonly DiagnosticDescriptor MissingCreateComponents = new DiagnosticDescriptor(
            id: "ASP001",
            title: "Missing CreateComponents implementation",
            messageFormat: "'{0}' must implement 'private static partial void CreateComponents(ref ComponentsFactory factory, {1} setup)' required by ICreateEntityAspect<{1}>",
            category: "EntityAspect",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
    }
}