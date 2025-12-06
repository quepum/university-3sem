// <copyright file="MyTask.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Homework3;

using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Represents an asynchronous operation that produces a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
internal sealed class MyTask<TResult> : IMyTask<TResult>
{
    private readonly Func<TResult> work;
    private readonly MyThreadPool pool;
    private readonly Lock syncRoot = new();
    private readonly ManualResetEventSlim completionEvent = new ManualResetEventSlim(false);

    private bool isCompleted;
    private TResult result = default!;
    private Exception exception = null!;
    private List<Action> continuations = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyTask{TResult}"/> class.
    /// </summary>
    /// <param name="work">The function to execute.</param>
    /// <param name="pool">The thread pool that owns this task.</param>
    public MyTask(Func<TResult> work, MyThreadPool pool)
    {
        this.work = work ?? throw new ArgumentNullException(nameof(work));
        this.pool = pool ?? throw new ArgumentNullException(nameof(pool));
    }

    /// <inheritdoc/>
    public bool IsCompleted
    {
        get
        {
            lock (this.syncRoot)
            {
                return this.isCompleted;
            }
        }
    }

    /// <inheritdoc/>
    public TResult Result
    {
        get
        {
            this.completionEvent.Wait();
            lock (this.syncRoot)
            {
                if (this.exception != null)
                {
                    throw new AggregateException(this.exception);
                }

                return this.result;
            }
        }
    }

    /// <inheritdoc/>
    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation)
    {
        if (continuation == null)
        {
            throw new ArgumentNullException(nameof(continuation));
        }

        var nextTask = new MyTask<TNewResult>(() => continuation(this.Result), this.pool);
        var wrapper = new Action(() => nextTask.Execute());

        lock (this.syncRoot)
        {
            if (this.isCompleted)
            {
                this.pool.EnqueueContinuation(wrapper);
            }
            else
            {
                this.continuations ??= [];
                this.continuations.Add(wrapper);
            }
        }

        return nextTask;
    }

    internal void Execute()
    {
        try
        {
            this.result = this.work();
        }
        catch (Exception ex)
        {
            this.exception = ex;
        }
        finally
        {
            List<Action> continuationsToRun = null!;
            lock (this.syncRoot)
            {
                this.isCompleted = true;
                if (this.continuations != null)
                {
                    continuationsToRun = [..this.continuations];
                    this.continuations.Clear();
                }
            }

            this.completionEvent.Set();

            if (continuationsToRun != null)
            {
                foreach (var cont in continuationsToRun)
                {
                    this.pool.EnqueueContinuation(cont);
                }
            }
        }
    }
}