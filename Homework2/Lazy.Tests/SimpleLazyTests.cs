// <copyright file="SimpleLazyTests.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Lazy.Tests;

public class SimpleLazyTests
{
    private int callCount;

    [SetUp]
    public void Setup() => this.callCount = 0;

    [Test]
    public void Get_InvokedOnce_SupplierCalledOnlyOnce()
    {
        var lazy = new SimpleLazy<int>(() =>
        {
            this.callCount++;
            return 42;
        });

        var result1 = lazy.Get();
        var result2 = lazy.Get();

        Assert.Multiple(() =>
        {
            Assert.That(result1, Is.EqualTo(42));
            Assert.That(result2, Is.EqualTo(42));
            Assert.That(this.callCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void Get_SupplierReturnsNull_ReturnsNull()
    {
        var lazy = new SimpleLazy<string?>(() => null);
        Assert.That(lazy.Get(), Is.Null);
    }

    [Test]
    public void Get_SupplierReturnsDefaultInt_ReturnsZero()
    {
        var lazy = new SimpleLazy<int>(() => default);
        Assert.That(lazy.Get(), Is.EqualTo(0));
    }

    [Test]
    public void Constructor_SupplierIsNull_DoesNotThrowImmediately()
    {
        Assert.DoesNotThrow(() => _ = new SimpleLazy<string>(null!));
    }
}