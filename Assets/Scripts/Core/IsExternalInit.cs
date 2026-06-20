// Polyfill required for C# 9 'record' types in Unity.
// Unity's .NET runtime doesn't ship this type — declaring it here satisfies the compiler.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
