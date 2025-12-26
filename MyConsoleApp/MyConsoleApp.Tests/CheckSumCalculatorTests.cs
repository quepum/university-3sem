// <copyright file="CheckSumCalculatorTests.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyConsoleApp.Tests;

[TestFixture]
public class CheckSumCalculatorTests
{
    private string tempDir = string.Empty;

    [SetUp]
    public void SetUp()
    {
        this.tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(this.tempDir))
        {
            Directory.Delete(this.tempDir, true);
        }
    }

    [Test]
    public async Task AsyncSequentialAndParallel_ReturnSameResult()
    {
        // Arrange
        var file1 = Path.Combine(this.tempDir, "alpha.txt");
        await File.WriteAllTextAsync(file1, "content A");

        var sub = Path.Combine(this.tempDir, "beta");
        Directory.CreateDirectory(sub);
        var file2 = Path.Combine(sub, "gamma.bin");
        await File.WriteAllBytesAsync(file2, [0x01, 0x02, 0x03]);

        var hash1 = await CheckSumCalculator.ComputeCheckSumAsync(this.tempDir);
        var hash2 = await CheckSumCalculator.ComputeCheckSumParallelAsync(this.tempDir);

        Assert.That(hash1, Is.EqualTo(hash2));
    }

    [Test]
    public async Task EmptyDirectory_HashIsDeterministic()
    {
        var empty = Path.Combine(this.tempDir, "empty");
        Directory.CreateDirectory(empty);

        var hash1 = await CheckSumCalculator.ComputeCheckSumAsync(empty);
        var hash2 = await CheckSumCalculator.ComputeCheckSumParallelAsync(empty);

        Assert.That(hash1, Is.EqualTo(hash2));
    }

    [Test]
    public void NonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        var badPath = Path.Combine(this.tempDir, "nonexistent");

        Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => CheckSumCalculator.ComputeCheckSumAsync(badPath));

        Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => CheckSumCalculator.ComputeCheckSumParallelAsync(badPath));
    }
}