// <copyright file="InvalidSignatureTests.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyNUnit.Tests.TestClasses;

using MyNUnit;

public class InvalidSignatureTests
{
    [Test]
    public int ReturnsInt() => 42;

    [Test]
    public void TakesParam(int x)
    {
    }
}