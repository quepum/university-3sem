namespace Homework3.Tests;

public class MyThreadPoolTests
{
    private MyThreadPool pool;

    [SetUp]
    public void SetUp()
    {
        this.pool = new MyThreadPool(4);
    }

    [TearDown]
    public void TearDown()
    {
        this.pool?.Dispose();
    }

    [Test]
    public void Submit_ReturnsCorrectResult()
    {
        var task = this.pool.Submit(() => 42);
        Assert.That(task.Result, Is.EqualTo(42));
    }

    [Test]
    public void Submit_StringTask_ReturnsCorrectResult()
    {
        var task = this.pool.Submit(() => "hello");
        Assert.That(task.Result, Is.EqualTo("hello"));
    }

    [Test]
    public void ContinueWith_ChainsCorrectly()
    {
        var task = this.pool.Submit(() => 5)
                       .ContinueWith(x => x * 3)
                       .ContinueWith(x => x.ToString());

        Assert.That(task.Result, Is.EqualTo("15"));
    }

    [Test]
    public void ExceptionInTask_ThrowsAggregateException()
    {
        var task = this.pool.Submit<int>(() => throw new InvalidOperationException("Oops"));

        var ex = Assert.Throws<AggregateException>(() => _ = task.Result);
        Assert.That(ex.InnerException, Is.InstanceOf<InvalidOperationException>());
        Assert.That(ex.InnerException!.Message, Is.EqualTo("Oops"));
    }

    [Test]
    public void MultipleTasks_ExecuteConcurrently()
    {
        const int taskCount = 8;
        var tasks = new IMyTask<int>[taskCount];
        var started = 0;
        var completed = 0;
        var lockObj = new object();

        for (var i = 0; i < taskCount; i++)
        {
            tasks[i] = this.pool.Submit(() =>
            {
                lock (lockObj)
                {
                    started++;
                }

                Thread.Sleep(20);

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
        var task1 = this.pool.Submit(() => 
        {
            Thread.Sleep(10);
            return 1;
        });
        var task2 = this.pool.Submit(() =>
        {
            Thread.Sleep(10);
            return 2;
        });

        this.pool.Shutdown();

        Assert.Multiple(() =>
        {
            Assert.That(task1.Result, Is.EqualTo(1));
            Assert.That(task2.Result, Is.EqualTo(2));
        });
    }

    [Test]
    public void Submit_AfterShutdown_ThrowsObjectDisposedException()
    {
        this.pool.Shutdown();

        Assert.Throws<ObjectDisposedException>(() => this.pool.Submit(() => 42));
    }

    [Test]
    public void ContinueWith_AfterTaskCompletion_SchedulesOnPool()
    {
        var task = this.pool.Submit(() => 100);
        var cont = task.ContinueWith(x => x + 1);

        Assert.That(cont.Result, Is.EqualTo(101));
    }

    [Test]
    public void ContinueWith_BeforeTaskCompletion_WorksCorrectly()
    {
        var task = this.pool.Submit(() =>
        {
            Thread.Sleep(30);
            return 999;
        });

        var cont = task.ContinueWith(x => x.ToString());

        Assert.That(cont.Result, Is.EqualTo("999"));
    }

    [Test]
    public void PoolUsesExactlyNThreads()
    {
        const int threadCount = 3;
        using var localPool = new MyThreadPool(threadCount);

        var activeThreads = 0;
        var maxActiveThreads = 0;
        var lockObj = new object();

        const int taskCount = 10;
        var tasks = new IMyTask<int>[taskCount];

        for (var i = 0; i < taskCount; i++)
        {
            tasks[i] = localPool.Submit(() =>
            {
                lock (lockObj)
                {
                    activeThreads++;
                    if (activeThreads > maxActiveThreads)
                    {
                        maxActiveThreads = activeThreads;
                    }
                }

                Thread.Sleep(20);

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
