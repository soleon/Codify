using System.Diagnostics.CodeAnalysis;

namespace Codify.System;

/// <summary>
///     Provides a lazily initialized static instance for types with a public parameterless constructor.
/// </summary>
/// <typeparam name="T">The type of the instance to create.</typeparam>
public abstract class StaticInstance<T> where T : new()
{
    /// <summary>
    ///     Gets the shared instance of <typeparamref name="T" />.
    /// </summary>
    [SuppressMessage(
        "Design",
        "CA1000:Do not declare static members on generic types",
        Justification = "This generic singleton-style property is the public API this base class exists to provide.")]
    public static T Instance { get; } = new();
}
