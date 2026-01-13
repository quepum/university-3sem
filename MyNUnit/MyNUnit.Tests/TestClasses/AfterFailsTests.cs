// <copyright file="AfterFailsTests.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyNUnit.Tests.TestClasses;

using MyNUnit;

public class AfterFailsTests
{
    [Test]
    public void TestThatPasses()
    {
    }

    [After]
    public void FailingAfter() => throw new Exception("After failed!");
}