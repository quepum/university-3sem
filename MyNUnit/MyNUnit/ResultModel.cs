namespace MyNUnit;

/// <summary>
/// Represents the result of a single test execution.
/// </summary>
public class ResultModel
{
    /// <summary>
    /// Gets the fully qualified name of the test method.
    /// </summary>
    public string TestName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether it gets or sets whether the test passed successfully.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets a value indicating whether it gets or sets whether the test was ignored.
    /// </summary>
    public bool IsIgnored { get; init; }

    /// <summary>
    /// Gets the reason for ignoring the test (if applicable).
    /// </summary>
    public string? IgnoreReason { get; init; }

    /// <summary>
    /// Gets or sets the execution time of the test in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Gets or sets the error message if the test failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}