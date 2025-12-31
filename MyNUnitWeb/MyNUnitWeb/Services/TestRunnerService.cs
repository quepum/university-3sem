// </copyright file="TestRunnerService.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyNUnitWeb.Services;

using MyNUnit;
using MyNUnitWeb.Data;

/// <summary>
/// Service responsible for orchestrating the execution of test runs using the <see cref="MyNUnit.TestRunner"/>
/// and persisting the results to the database.
/// </summary>
public class TestRunnerService(ApplicationDbContext db)
{
    /// <summary>
    /// Starts a new test run by executing all tests found in the assemblies located in the specified directory.
    /// Test results are saved to the database, and the unique identifier of the test run is returned.
    /// </summary>
    /// <param name="tempDirectory">
    /// The path to the directory containing the test assemblies (.dll or .exe files).
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation,
    /// with a result of the unique identifier (<see cref="int"/>) of the created test run.
    /// </returns>
    public async Task<int> StartTestRunAsync(string tempDirectory)
    {
        var testRun = new TestRun { StartedAt = DateTime.UtcNow };
        db.TestRuns.Add(testRun);
        await db.SaveChangesAsync();

        try
        {
            var runner = new TestRunner(tempDirectory);
            var results = runner.RunAndGetResults();

            testRun.FinishedAt = DateTime.UtcNow;
            testRun.Status = "Completed";
            testRun.Total = results.Count;
            testRun.Passed = results.Count(r => r.IsSuccess);
            testRun.Failed = results.Count(r => r is { IsSuccess: false, IsIgnored: false });
            testRun.Ignored = results.Count(r => r.IsIgnored);

            foreach (var r in results)
            {
                testRun.Results.Add(new TestResultItem
                {
                    TestName = r.TestName,
                    IsSuccess = r.IsSuccess,
                    IsIgnored = r.IsIgnored,
                    IgnoreReason = r.IgnoreReason,
                    DurationMs = r.DurationMs,
                    ErrorMessage = r.ErrorMessage,
                });
            }

            await db.SaveChangesAsync();
            return testRun.Id;
        }
        catch (Exception)
        {
            testRun.Status = "Failed";
            testRun.FinishedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            throw;
        }
    }
}