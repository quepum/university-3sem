// <copyright file="BeforeClassFailsTests.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyNUnit.Tests.TestClasses;

using MyNUnit;

public class BeforeClassFailsTests
{
    [BeforeClass]
    public static void FailingSetup() => throw new InvalidOperationException("BeforeClass failed!");

    [Test]
    public void ShouldNotRun()
    {
    }
}