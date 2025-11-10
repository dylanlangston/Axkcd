// https://stackoverflow.com/a/64749403
// Required for using 'init' accessors in projects targeting frameworks older than .NET 5.
// The IsExternalInit type exists in .NET 5+, so define it manually here for compatibility.
// Make it internal to avoid conflicts if multiple libraries define the same type.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}