// </copyright file="ApplicationDbContext.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>
namespace MyNUnitWeb.Data;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Represents the Entity Framework Core database context for the MyNUnit web application.
/// This context manages the persistence of test run metadata and individual test results.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the DbSet for <see cref="TestRun"/> entities, representing test execution sessions.
    /// </summary>
    public DbSet<TestRun> TestRuns => this.Set<TestRun>();

    /// <summary>
    /// Gets the DbSet for <see cref="TestResultItem"/> entities, representing individual test outcomes.
    /// </summary>
    public DbSet<TestResultItem> TestResults => this.Set<TestResultItem>();
}