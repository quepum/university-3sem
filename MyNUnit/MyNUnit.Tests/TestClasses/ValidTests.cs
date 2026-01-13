// <copyright file="ValidTests.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyNUnit.Tests.TestClasses;

using MyNUnit;

public class ValidTests
{
    [BeforeClass]
    public static void SetupClass()
    {
    }

    [AfterClass]
    public static void TeardownClass()
    {
    }

    [Before]
    public void Setup()
    {
    }

    [After]
    public void Teardown()
    {
    }

    [Test]
    public void PassingTest()
    {
    }

    [Test(Expected = typeof(ArgumentException))]
    public void ThrowsExpected() => throw new ArgumentException();

    [Test(Ignore = "Skipped for demo")]
    public void IgnoredTest()
    {
    }

    [Test(Expected = typeof(InvalidOperationException))]
    public void FailsBecauseWrongException()
    {
        throw new ArgumentException("Wrong exception type");
    }
}