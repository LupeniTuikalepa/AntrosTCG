using System;

namespace UnityEngine;

public class RuntimeInitializeOnLoadMethodAttribute : Attribute
{
    public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType type)
    {

    }
}

public enum RuntimeInitializeLoadType
{
    BeforeSceneLoad,
    AfterSceneLoad,
    SubSceneLoad,
    SubsystemRegistration,
}

public static class Debug
{
    public static void Log(object message, object source = null) { }
}