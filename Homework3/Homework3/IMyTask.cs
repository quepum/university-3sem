// <copyright file="IMyTask.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Homework3;

/// <summary>
/// Represents an operation that produces a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
public interface IMyTask<out TResult>
{
    /// <summary>
    /// Gets a value indicating whether the task has completed.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets the result of the task. Blocks the calling thread until the task completes.
    /// If the task faulted, throws an <see cref="AggregateException"/>.
    /// </summary>
    TResult Result { get; }

    /// <summary>
    /// Creates a continuation that executes when this task completes.
    /// </summary>
    /// <typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
    /// <param name="continuation">The function delegate to execute when this task completes.</param>
    /// <returns>A new task that represents the continuation.</returns>
    IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation);
}