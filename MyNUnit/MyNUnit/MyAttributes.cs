namespace MyNUnit;

/// <summary>
/// Marks a method as a test method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class TestAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the type of the expected exception, if any.
    /// </summary>
    public Type? Expected { get; set; }

    /// <summary>
    /// Gets or sets the reason for ignoring the test, if the test is ignored.
    /// </summary>
    public string? Ignore { get; set; }
}

/// <summary>
/// Marks a method to be executed before each test method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BeforeAttribute : Attribute;

/// <summary>
/// Marks a method to be executed after each test method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AfterAttribute : Attribute;

/// <summary>
/// Marks a static method to be executed once before all tests in the class.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BeforeClassAttribute : Attribute;

/// <summary>
/// Marks a static method to be executed once after all tests in the class.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AfterClassAttribute : Attribute;