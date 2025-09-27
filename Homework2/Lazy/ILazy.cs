namespace Lazy;

/// <summary>
/// Represents a lazily evaluated value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the lazily evaluated value.</typeparam>
public interface ILazy<out T>
{
    /// <summary>
    /// Retrieves the lazily evaluated value.
    /// The value is computed on first call and may be cached for subsequent calls.
    /// </summary>
    /// <returns>The computed value of type <typeparamref name="T"/>.</returns>
    T Get();
}