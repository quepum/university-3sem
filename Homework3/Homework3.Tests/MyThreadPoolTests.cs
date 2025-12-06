// <copyright file="MyThreadPoolTests.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Homework3.Tests;

using System.Threading;
using NUnit.Framework;

[TestFixture]
public class MyThreadPoolTests
{
    [Test]
    public void Submit_ReturnsCorrectResult()
    {
        using var pool = new MyThreadPool(2);
        var task = pool.Submit(() => 42);
        Assert.That(task.Result, Is.EqualTo(42));
    }

    [Test]
    public void Submit_StringTask_ReturnsCorrectResult()
    {
        using var pool = new MyThreadPool(2);
        var task = pool.Submit(() => "hello");
        Assert.That(task.Result, Is.EqualTo("hello"));
    }

    [Test]
    public void ContinueWith_ChainsCorrectly()
    {
        using var pool = new MyThreadPool(2);
        var task = pool.Submit(() => 5)
            .ContinueWith(x => x * 3)
            .ContinueWith(x => x.ToString());

        Assert.That(task.Result, Is.EqualTo("15"));
    }

    [Test]
    public void ExceptionInTask_ThrowsAggregateException()
    {
        using var pool = new MyThreadPool(1);
        var task = pool.Submit<int>(() => throw new InvalidOperationException("Oops"));

        var ex = Assert.Throws<AggregateException>(() => _ = task.Result);
        Assert.That(ex.InnerException, Is.InstanceOf<InvalidOperationException>());
        Assert.That(ex.InnerException.Message, Is.EqualTo("Oops"));
    }

    [Test]
    public void MultipleTasks_ExecuteConcurrently()
    {
        const int taskCount = 8;
        using var pool = new MyThreadPool(4);

        var tasks = new IMyTask<int>[taskCount];
        var started = 0;
        var completed = 0;
        var lockObj = new object();

        for (var i = 0; i < taskCount; i++)
        {
            tasks[i] = pool.Submit(() =>
            {
                lock (lockObj)
                {
                    started++;
                }

                Thread.Sleep(1); // минимальная задержка
                lock (lockObj)
                {
                    completed++;
                    return completed;
                }
            });
        }

        foreach (var t in tasks)
        {
            _ = t.Result;
        }

        Assert.Multiple(() =>
        {
            Assert.That(started, Is.EqualTo(taskCount));
            Assert.That(completed, Is.EqualTo(taskCount));
        });
    }

    [Test]
    public void Shutdown_WaitsForAllSubmittedTasksToComplete()
    {
        using var pool = new MyThreadPool(2);

        var task1 = pool.Submit(() =>
        {
            Thread.Sleep(1);
            return 1;
        });
        var task2 = pool.Submit(() =>
        {
            Thread.Sleep(1);
            return 2;
        });

        pool.Shutdown();

        Assert.Multiple(() =>
        {
            Assert.That(task1.Result, Is.EqualTo(1));
            Assert.That(task2.Result, Is.EqualTo(2));
        });
    }

    [Test]
    public void Submit_AfterShutdown_ThrowsObjectDisposedException()
    {
        using var pool = new MyThreadPool(2);
        pool.Shutdown();
        Assert.Throws<ObjectDisposedException>(() => pool.Submit(() => 42));
    }

    [Test]
    public void ContinueWith_AfterTaskCompletion_SchedulesOnPool()
    {
        using var pool = new MyThreadPool(2);
        var task = pool.Submit(() => 100);
        var cont = task.ContinueWith(x => x + 1);
        Assert.That(cont.Result, Is.EqualTo(101));
    }

    [Test]
    public void ContinueWith_BeforeTaskCompletion_WorksCorrectly()
    {
        using var pool = new MyThreadPool(2);
        var task = pool.Submit(() =>
        {
            Thread.Sleep(10);
            return 999;
        });

        var cont = task.ContinueWith(x => x.ToString());
        Assert.That(cont.Result, Is.EqualTo("999"));
    }

    [Test]
    public void PoolUsesExactlyNThreads()
    {
        const int threadCount = 3;
        using var pool = new MyThreadPool(threadCount);

        var activeThreads = 0;
        var maxActiveThreads = 0;
        var lockObj = new object();

        const int taskCount = 10;
        var tasks = new IMyTask<int>[taskCount];

        for (var i = 0; i < taskCount; i++)
        {
            tasks[i] = pool.Submit(() =>
            {
                lock (lockObj)
                {
                    activeThreads++;
                    if (activeThreads > maxActiveThreads)
                    {
                        maxActiveThreads = activeThreads;
                    }
                }

                Thread.Sleep(10);

                lock (lockObj)
                {
                    activeThreads--;
                }

                return 0;
            });
        }

        foreach (var t in tasks)
        {
            _ = t.Result;
        }

        Assert.That(maxActiveThreads, Is.EqualTo(threadCount));
    }
}