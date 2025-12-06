// <copyright file="SimpleLazy.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Lazy;

/// <summary>
/// A simple lazy evaluation realization that computes a value on first access.
/// Thread-unsafe. Discards the supplier after first evaluation to allow garbage collection.
/// </summary>
/// <typeparam name="T">The type of the lazily evaluated value.</typeparam>
public class SimpleLazy<T>(Func<T> supplier) : ILazy<T>
{
    private Func<T>? supplier = supplier;
    private T result = default!;
    private bool isDone;

    /// <summary>
    /// Gets the lazily evaluated value.
    /// On first call, invokes the supplier to compute the value, caches it, and nulls the supplier.
    /// Subsequent calls return the cached value.
    /// </summary>
    /// <returns>The computed value of type <typeparamref name="T"/>.</returns>
    public T Get()
    {
        if (!this.isDone)
        {
            this.result = this.supplier!();
            this.supplier = null;
            this.isDone = true;
        }

        return this.result;
    }
}