// <copyright file="MyThreadPool.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Homework3;

using System;
using System.Collections.Concurrent;
using System.Threading;

/// <summary>
/// Provides a simple thread pool with a fixed number of worker threads.
/// </summary>
public sealed class MyThreadPool : IDisposable
{
    private readonly Thread[] threads;
    private readonly ConcurrentQueue<WorkItem> workQueue = new();
    private readonly CancellationTokenSource shutdownCts = new();
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyThreadPool"/> class with the specified number of threads.
    /// </summary>
    /// <param name="threadCount">The number of worker threads in the pool.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="threadCount"/> is less than or equal to zero.</exception>
    public MyThreadPool(int threadCount)
    {
        if (threadCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(threadCount), "Thread count must be greater than zero.");
        }

        this.threads = new Thread[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            this.threads[i] = new Thread(this.WorkerLoop)
            {
                IsBackground = true,
            };
            this.threads[i].Start();
        }
    }

    /// <summary>
    /// Submits a task for execution.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
    /// <param name="work">The function to execute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="work"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the pool has been shut down.</exception>
    public IMyTask<TResult> Submit<TResult>(Func<TResult> work)
    {
        this.ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(work);

        var task = new MyTask<TResult>(work, this);
        this.workQueue.Enqueue(new WorkItem(() => task.Execute()));
        return task;
    }

    /// <summary>
    /// Shuts down the thread pool. Waits for all worker threads to finish.
    /// No new tasks will be accepted after this method is called.
    /// </summary>
    public void Shutdown()
    {
        this.ThrowIfDisposed();
        this.shutdownCts.Cancel();

        foreach (var thread in this.threads)
        {
            if (thread.IsAlive)
            {
                thread.Join();
            }
        }

        this.disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!this.disposed)
        {
            this.Shutdown();
        }
    }

    internal void EnqueueContinuation(Action continuation)
    {
        this.workQueue.Enqueue(new WorkItem(continuation));
    }

    private void WorkerLoop()
    {
        while (!this.shutdownCts.Token.IsCancellationRequested || !this.workQueue.IsEmpty)
        {
            if (this.workQueue.TryDequeue(out var workItem))
            {
                try
                {
                    workItem.Action();
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                Thread.Yield();
            }
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);
    }

    private readonly struct WorkItem(Action action)
    {
        public Action Action { get; } = action ?? throw new ArgumentNullException(nameof(action));
    }
}