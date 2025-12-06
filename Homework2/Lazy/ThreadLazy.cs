// <copyright file="ThreadLazy.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Lazy;

/// <summary>
/// A thread-safe lazy evaluation realization that computes a value on first access.
/// </summary>
/// <typeparam name="T">The type of the lazily evaluated value.</typeparam>
public class ThreadLazy<T>(Func<T> supplier) : ILazy<T>
{
    private readonly Lock lockk = new();
    private Func<T>? supplier = supplier;
    private T result = default!;
    private volatile bool isDone;

    /// <summary>
    /// Gets the lazily evaluated value in a thread-safe manner.
    /// On first successful call, invokes the supplier, caches the result, and nulls the supplier.
    /// Subsequent calls return the cached value without locking.
    /// </summary>
    /// <returns>The computed value of type <typeparamref name="T"/>.</returns>
    public T Get()
    {
        if (!this.isDone)
        {
            lock (this.lockk)
            {
                if (!this.isDone)
                {
                    this.result = this.supplier!();
                    this.supplier = null;
                    this.isDone = true;
                }
            }
        }

        return this.result;
    }
}