// <copyright file="ThreadLazyTests.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Lazy.Tests;

using System.Collections.Concurrent;

public class ThreadLazyTests
{
    private int callCount;

    [SetUp]
    public void SetUp() => this.callCount = 0;

    [Test]
    public async Task Get_ConcurrentCalls_SupplierInvokedExactlyOnce()
    {
        const int threadCount = 10;
        var lazy = new ThreadLazy<int>(() =>
        {
            this.callCount++;
            return 999;
        });

        var tasks = new Task<int>[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            tasks[i] = Task.Run(() => lazy.Get());
        }

        var results = await Task.WhenAll(tasks);
        foreach (var result in results)
        {
            Assert.That(result, Is.EqualTo(999));
        }

        Assert.That(this.callCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Get_ConcurrentCalls_ResultIsConsistentAndCached()
    {
        var lazy = new ThreadLazy<Guid>(Guid.NewGuid);

        var barrier = new Barrier(5);
        var results = new ConcurrentBag<Guid>();

        var tasks = Enumerable.Range(0, 5).Select(_ => Task.Run(() =>
        {
            barrier.SignalAndWait();
            var result = lazy.Get();
            results.Add(result);
        })).ToArray();

        await Task.WhenAll(tasks);

        var distinctResults = results.Distinct().ToList();
        Assert.That(distinctResults, Has.Count.EqualTo(1));
    }
}