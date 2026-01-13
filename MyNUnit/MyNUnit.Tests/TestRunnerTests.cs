// <copyright file="TestRunnerTests.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyNUnit.Tests;

using NUnit.Framework;

[TestFixture]
public class TestRunnerTests
{
    private List<ResultModel> RunTestRunnerOnType<T>()
    {
        var type = typeof(T);
        var assembly = type.Assembly;
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var dest = Path.Combine(tempDir, Path.GetFileName(assembly.Location));
            File.Copy(assembly.Location, dest, overwrite: true);
            var runner = new TestRunner(tempDir);
            var allResults = runner.RunAndGetResults();

            return allResults
                .Where(r => r.TestName.StartsWith(type.Name + "."))
                .ToList();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public void ValidTests_ExecuteCorrectly()
    {
        var results = this.RunTestRunnerOnType<TestClasses.ValidTests>();

        Assert.That(results, Has.Count.EqualTo(4));

        var passed = results.Where(r => r.IsSuccess).ToList();
        var failed = results.Where(r => r is { IsSuccess: false, IsIgnored: false }).ToList();
        var ignored = results.Where(r => r.IsIgnored).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(passed, Has.Count.EqualTo(2));
            Assert.That(failed, Has.Count.EqualTo(1));
            Assert.That(ignored, Has.Count.EqualTo(1));

            Assert.That(failed[0].ErrorMessage, Does.Contain("Wrong exception type"));
            Assert.That(ignored[0].IgnoreReason, Is.EqualTo("Skipped for demo"));
        });
    }

    [Test]
    public void InvalidMethodSignatures_AreReportedAsFailures()
    {
        var results = this.RunTestRunnerOnType<TestClasses.InvalidSignatureTests>();

        Assert.That(results, Has.Count.EqualTo(2));
        foreach (var result in results)
        {
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.ErrorMessage, Does.Contain("Invalid test method"));
            });
        }

        Assert.Multiple(() =>
        {
            Assert.That(results.First(r => r.TestName.Contains("ReturnsInt")).ErrorMessage, Does.Contain("must return void"));
            Assert.That(results.First(r => r.TestName.Contains("TakesParam")).ErrorMessage, Does.Contain("must not accept parameters"));
        });
    }

    [Test]
    public void BeforeClassFailure_MarksAllTestsAsFailed()
    {
        var results = this.RunTestRunnerOnType<TestClasses.BeforeClassFailsTests>();

        Assert.That(results, Has.Count.EqualTo(1));
        var result = results[0];

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("BeforeClass method"));
            Assert.That(result.ErrorMessage, Does.Contain("BeforeClass failed!"));
        });
    }

    [Test]
    public void AfterMethodFailure_MarksTestAsFailed()
    {
        var results = this.RunTestRunnerOnType<TestClasses.AfterFailsTests>();

        Assert.That(results, Has.Count.EqualTo(1));
        var result = results[0];

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("After method failed"));
            Assert.That(result.ErrorMessage, Does.Contain("Exception has been thrown by the target of an invocation"));
        });
    }

    [Test]
    public void ExpectedException_Handling()
    {
        var results = this.RunTestRunnerOnType<TestClasses.ValidTests>();
        var throwsExpected = results.First(r => r.TestName.Contains("ThrowsExpected"));
        Assert.That(throwsExpected.IsSuccess, Is.True);
    }

    [Test]
    public void IgnoredTests_AreSkipped()
    {
        var results = this.RunTestRunnerOnType<TestClasses.ValidTests>();
        var ignored = results.First(r => r.IsIgnored);
        Assert.Multiple(() =>
        {
            Assert.That(ignored.IgnoreReason, Is.EqualTo("Skipped for demo"));
            Assert.That(ignored.DurationMs, Is.EqualTo(0));
        });
    }

    [Test]
    public void DirectoryNotFound_ThrowsException()
    {
        Assert.Throws<DirectoryNotFoundException>(() => new TestRunner("/invalid/path"));
    }

    [Test]
    public void NullDirectory_ThrowsArgumentNullException()
    {
        string? nullPath = null;
        Assert.Throws<ArgumentNullException>(() => new TestRunner(nullPath));
    }

    [Test]
    public void NonManagedFiles_AreSkipped()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var fakeDll = Path.Combine(tempDir, "fake.dll");
            File.WriteAllText(fakeDll, "This is not a .NET assembly");

            var runner = new TestRunner(tempDir);
            var results = runner.RunAndGetResults();

            Assert.That(results, Is.Empty);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}