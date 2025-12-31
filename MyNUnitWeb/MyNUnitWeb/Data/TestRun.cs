// </copyright file="TestRun.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyNUnitWeb.Data;

/// <summary>
/// Represents a single execution session of automated tests, containing aggregated statistics
/// and a collection of individual test results.
/// </summary>
public class TestRun
{
    /// <summary>
    /// Gets or sets the unique identifier of the test run.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the test run started.
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC date and time when the test run finished, or <see langword="null"/> if still running.
    /// </summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>
    /// Gets or sets the current status of the test run.
    /// Expected values: "Running", "Completed", or "Failed".
    /// </summary>
    public string Status { get; set; } = "Running";

    /// <summary>
    /// Gets or sets the total number of test methods discovered and attempted to run.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Gets or sets the number of tests that passed successfully.
    /// </summary>
    public int Passed { get; set; }

    /// <summary>
    /// Gets or sets the number of tests that failed (excluding ignored tests).
    /// </summary>
    public int Failed { get; set; }

    /// <summary>
    /// Gets or sets the number of tests that were skipped due to the <c>Ignore</c> attribute.
    /// </summary>
    public int Ignored { get; set; }

    /// <summary>
    /// Gets or sets the collection of individual test results associated with this run.
    /// This property is initialized to an empty list by default.
    /// </summary>
    public List<TestResultItem> Results { get; set; } = [];
}