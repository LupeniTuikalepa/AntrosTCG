using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
/*
namespace GameSourceGeneration.Tests
{
    public class EntityAspectInterfaceGeneratorTests
    {
        [Fact]
        public void GeneratesInterfaceForArity3()
        {
            var source = """
                         namespace Test
                         {
                             public struct HeroEntityAspect : IEntityAspect<ComponentA, ComponentB, ComponentC> { }
                             public interface IEntityAspect<T1, T2, T3> { }
                             public struct ComponentA { }
                             public struct ComponentB { }
                             public struct ComponentC { }
                         }
                         """;

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source) },
                references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var generator = new EntityAspectInterfaceGenerator();
            var driver = CSharpGeneratorDriver
                .Create(generator)
                .RunGenerators(compilation);

            var result = driver.GetRunResult();

            Assert.Single(result.GeneratedTrees);

            var generatedSource = result.GeneratedTrees[0].ToString();
            Assert.Contains("IEntityAspect<T1, T2, T3>", generatedSource);
            Assert.Contains("where T1 : struct, IEntityComponent", generatedSource);
        }
    }
}
*/