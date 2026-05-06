using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace GameSourceGeneration;

public sealed class AspectInfo
{
    public string structName;
    public string @namespace;
    public List<string> usings;
    public List<string> componentTypes;
    public string factoryInfosTypes;
    public Location structLocation;
    public INamedTypeSymbol structSymbol;
}