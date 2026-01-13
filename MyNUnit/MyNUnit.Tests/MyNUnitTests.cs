// <copyright file="MyNUnitTests.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyNUnit.Tests;

[TestFixture]
public class MyNUnitTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}