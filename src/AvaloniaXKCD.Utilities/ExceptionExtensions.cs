using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AvaloniaXKCD.Utilities;

/// <summary>
/// Provides extension methods for throwing exceptions.
/// </summary>
[StackTraceHidden]
public static class ExceptionExtensions
{
    [StackTraceHidden]
    private static class ThrowException<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T
    >
        where T : Exception
    {
        [DebuggerHidden]
        [StackTraceHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void If(bool condition, params object?[]? args)
        {
            if (condition)
            {
                var e = Activator.CreateInstance(typeof(T), args) as Exception;
                if (e != null)
                    throw e;

                throw Activator.CreateInstance<T>();
            }
        }
    }

    /// <summary>
    /// Provides extension methods for a given exception type.
    /// </summary>
    /// <typeparam name="T">The type of exception to be thrown.</typeparam>
    extension<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(T)
        where T : Exception
    {
        /// <summary>
        /// Throws an exception of type T if the specified condition is true.
        /// </summary>
        /// <example>
        /// <code>
        /// ArgumentException.ThrowIf(string.IsNullOrEmpty(name), "Name cannot be null or empty.");
        /// </code>
        /// </example>
        [DebuggerHidden]
        [StackTraceHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIf([DoesNotReturnIf(true)] bool condition, params object?[]? args) =>
            ThrowException<T>.If(condition, args);

        /// <summary>
        /// Throws an exception of type T if the specified condition is true.
        /// </summary>
        /// <example>
        /// <code>
        /// InvalidOperationException.ThrowIf(isProcessing, "Cannot perform action while processing.");
        /// </code>
        /// </example>
        [DebuggerHidden]
        [StackTraceHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIf([DoesNotReturnIf(true)] bool condition, object? arg) =>
            ThrowException<T>.If(condition, [arg]);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the provided argument is null.
    /// </summary>
    /// <typeparam name="TReturn">The type of the argument.</typeparam>
    /// <param name="argument">The argument to check for null.</param>
    /// <param name="paramName">The name of the parameter, captured automatically.</param>
    /// <returns>The non-null argument.</returns>
    /// <example>
    /// <code>
    /// public void ProcessData(string data)
    /// {
    ///     data.ThrowIfNull();
    ///     // ... process data
    /// }
    /// </code>
    /// </example>
    [DebuggerHidden]
    [StackTraceHidden]
    public static TReturn ThrowIfNull<TReturn>(
        [NotNull] this TReturn? argument,
        [CallerArgumentExpression("argument")] string? paramName = null
    )
    {
        if (argument == null)
        {
            throw new ArgumentNullException(paramName);
        }
        return argument;
    }
}
