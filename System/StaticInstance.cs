namespace Codify.System
{
    public abstract class StaticInstance<T> where T : new()
    {
        public static T Instance { get; } = new T();
    }
}