namespace Codify.System
{
    public abstract class StaticInstanceBase<T> where T : new()
    {
        public static T Instance { get; } = new T();
    }
}