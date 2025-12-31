// </copyright file="TestRunsController.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyNUnitWeb.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyNUnitWeb.Data;
using MyNUnitWeb.Services;

/// <summary>
/// Provides API endpoints for managing the execution of a test run and obtaining test results.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestRunsController(
    IWebHostEnvironment env,
    TestRunnerService runnerService,
    ApplicationDbContext db)
    : ControllerBase
{
    /// <summary>
    /// Uploads one or more assembly files to a temporary directory for testing.
    /// </summary>
    /// <param name="files">The collection of uploaded files.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the unique identifier of the upload session.
    /// </returns>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFiles(IFormFile[]? files)
    {
        if (files == null || files.Length == 0)
        {
            return this.BadRequest("No files uploaded.");
        }

        var uploadId = Guid.NewGuid().ToString();
        var uploadDir = Path.Combine(env.ContentRootPath, "Uploads", uploadId);
        Directory.CreateDirectory(uploadDir);

        foreach (var file in files)
        {
            if (file.FileName.EndsWith(".dll") || file.FileName.EndsWith(".exe"))
            {
                var path = Path.Combine(uploadDir, file.FileName);
                await using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);
            }
        }

        return this.Ok(new { uploadId });
    }

    /// <summary>
    /// Executes all discovered tests in the assemblies associated with the specified upload session.
    /// </summary>
    /// <param name="uploadId">The unique identifier of the upload session.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the identifier of the created test run
    /// if successful; otherwise, a <see cref="NotFoundResult"/> if the upload directory does not exist,
    /// or a <see cref="StatusCodeResult"/> with status 500 in case of an internal error.
    /// </returns>
    [HttpPost("run/{uploadId}")]
    public async Task<IActionResult> RunTests(string uploadId)
    {
        var uploadDir = Path.Combine(env.ContentRootPath, "Uploads", uploadId);
        if (!Directory.Exists(uploadDir))
        {
            return this.NotFound();
        }

        try
        {
            var runId = await runnerService.StartTestRunAsync(uploadDir);
            return this.Ok(new { runId });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a list of all test runs, ordered by start time in descending order.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation,
    /// with a result of <see cref="ActionResult{T}"/> containing a list of <see cref="TestRun"/> objects.
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<List<TestRun>>> GetRuns()
    {
        return await db.TestRuns
            .OrderByDescending(r => r.StartedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves the detailed information of a specific test run by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the test run.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation,
    /// with a result of <see cref="ActionResult{T}"/> containing the <see cref="TestRun"/> object
    /// if found; otherwise, a <see cref="NotFoundResult"/>.
    /// </returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<TestRun>> GetRun(int id)
    {
        var run = await db.TestRuns
            .Where(r => r.Id == id)
            .Select(r => new TestRun
            {
                Id = r.Id,
                StartedAt = r.StartedAt,
                FinishedAt = r.FinishedAt,
                Status = r.Status,
                Total = r.Total,
                Passed = r.Passed,
                Failed = r.Failed,
                Ignored = r.Ignored,
                Results = r.Results.ToList(),
            })
            .FirstOrDefaultAsync();

        if (run == null)
        {
            return this.NotFound();
        }

        return run;
    }
}