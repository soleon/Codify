namespace Codify.System;

public abstract class StaticInstance<T> where T : new()
{
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1000:Do not declare static members on generic types",
        Justification = "This generic singleton-style property is the public API this base class exists to provide.")]
    public static T Instance { get; } = new();
}
