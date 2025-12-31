// </copyright file="TestResultItem.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyNUnitWeb.Data;

/// <summary>
/// Represents the result of a single test execution within a test run.
/// </summary>
public class TestResultItem
{
    /// <summary>
    /// Gets or sets the unique identifier of the test result.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the foreign key referencing the parent <see cref="TestRun"/>.
    /// </summary>
    public int TestRunId { get; set; }

    /// <summary>
    /// Gets or sets the fully qualified name of the test method.
    /// </summary>
    public string TestName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the test passed successfully.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the test was skipped due to the <c>Ignore</c> attribute.
    /// </summary>
    public bool IsIgnored { get; set; }

    /// <summary>
    /// Gets or sets the reason why the test was ignored, if applicable.
    /// This value is taken from the <c>Ignore</c> property of the <see cref="MyNUnit.TestAttribute"/>.
    /// </summary>
    public string? IgnoreReason { get; set; }

    /// <summary>
    /// Gets or sets the execution time of the test in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Gets or sets the error message if the test failed.
    /// For successful or ignored tests, this value is <see langword="null"/>.
    /// </summary>
    public string? ErrorMessage { get; set; }
}