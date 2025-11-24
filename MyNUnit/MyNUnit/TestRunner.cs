namespace MyNUnit;

using System.Diagnostics;
using System.Reflection;

/// <summary>
/// Provides functionality to discover and run test methods in assemblies.
/// </summary>
public class TestRunner
{
    private readonly string dirPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestRunner"/> class.
    /// </summary>
    /// <param name="path">The path to the directory containing test assemblies (.dll or .exe).</param>
    public TestRunner(string path)
    {
        this.dirPath = path ?? throw new ArgumentNullException(nameof(path));
        if (!Directory.Exists(this.dirPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {this.dirPath}");
        }
    }

    /// <summary>
    /// Discovers and runs all test methods in managed assemblies,
    /// then prints a summary.
    /// </summary>
    public void Run()
    {
        var results = this.RunAndGetResults();
        PrintResults(results);
    }

    /// <summary>
    /// Discovers and runs all test methods in managed assemblies and returns results.
    /// </summary>
    /// <returns>A list of <see cref="ResultModel"/> objects representing the outcome of each test.</returns>
    internal List<ResultModel> RunAndGetResults()
    {
        var dllFiles = Directory.GetFiles(this.dirPath, "*.dll", SearchOption.TopDirectoryOnly);
        var exeFiles = Directory.GetFiles(this.dirPath, "*.exe", SearchOption.TopDirectoryOnly);
        var candidateFiles = dllFiles.Concat(exeFiles).ToArray();

        var allResults = new List<ResultModel>();

        var tasks = candidateFiles.Select(async file =>
        {
            Assembly? assembly;
            try
            {
                assembly = Assembly.LoadFrom(file);
            }
            catch (BadImageFormatException)
            {
                return;
            }
            catch (FileLoadException ex)
            {
                Console.WriteLine($"Skipping '{file}': failed to load assembly ({ex.Message})");
                return;
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is UnauthorizedAccessException)
            {
                Console.WriteLine($"Skipping '{file}': access or file error ({ex.Message})");
                return;
            }

            try
            {
                var classResults = await this.RunTestsInAssemblyAsync(assembly);
                lock (allResults)
                {
                    allResults.AddRange(classResults);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running tests in '{file}': {ex.Message}");
            }
        });

        Task.WaitAll(tasks.ToArray());
        return allResults;
    }

    private async Task<List<ResultModel>> RunTestsInAssemblyAsync(Assembly assembly)
    {
        var results = new List<ResultModel>();
        var testTypes = assembly.GetTypes()
            .Where(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Any(m => m.GetCustomAttribute<TestAttribute>() != null))
            .ToList();

        var tasks = testTypes.Select(async type =>
        {
            var classResults = await this.RunTestsInClassAsync(type);
            lock (results)
            {
                results.AddRange(classResults);
            }
        });

        await Task.WhenAll(tasks);
        return results;
    }

    private async Task<List<ResultModel>> RunTestsInClassAsync(Type testClass)
    {
        var results = new List<ResultModel>();

        var beforeClassMethods = testClass.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.GetCustomAttribute<BeforeClassAttribute>() != null);
        foreach (var method in beforeClassMethods)
        {
            try
            {
                method.Invoke(null, null);
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException ?? ex;
                Console.WriteLine(
                    $"Warning: BeforeClass method '{method.Name}' in '{testClass.FullName}' threw: {inner.Message}");
            }
        }

        var testMethods = testClass.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<TestAttribute>() != null)
            .ToList();

        var testTasks = testMethods.Select(async method =>
        {
            var attr = method.GetCustomAttribute<TestAttribute>()!;
            if (!string.IsNullOrEmpty(attr.Ignore))
            {
                var ignoredResult = new ResultModel
                {
                    TestName = $"{testClass.Name}.{method.Name}",
                    IsIgnored = true,
                    IgnoreReason = attr.Ignore,
                    DurationMs = 0,
                };
                lock (results)
                {
                    results.Add(ignoredResult);
                }

                return;
            }

            var testResult = new ResultModel
            {
                TestName = $"{testClass.Name}.{method.Name}",
            };

            var stopwatch = Stopwatch.StartNew();
            object? instance = null;

            try
            {
                instance = Activator.CreateInstance(testClass);

                var beforeMethods = testClass.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.GetCustomAttribute<BeforeAttribute>() != null);
                foreach (var before in beforeMethods)
                {
                    before.Invoke(instance, null);
                }

                var expected = attr.Expected;
                try
                {
                    method.Invoke(instance, null);
                    if (expected != null)
                    {
                        testResult.IsSuccess = false;
                        testResult.ErrorMessage =
                            $"Expected exception of type {expected.Name}, but no exception was thrown.";
                    }
                    else
                    {
                        testResult.IsSuccess = true;
                    }
                }
                catch (TargetInvocationException tie)
                {
                    var actual = tie.InnerException!;
                    if (expected != null && expected.IsAssignableFrom(actual.GetType()))
                    {
                        testResult.IsSuccess = true;
                    }
                    else
                    {
                        testResult.IsSuccess = false;
                        testResult.ErrorMessage = actual.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                testResult.IsSuccess = false;
                testResult.ErrorMessage = ex.ToString();
            }
            finally
            {
                if (instance != null)
                {
                    var afterMethods = testClass.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(m => m.GetCustomAttribute<AfterAttribute>() != null);
                    foreach (var after in afterMethods)
                    {
                        try
                        {
                            after.Invoke(instance, null);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning: After method failed in '{testResult.TestName}': {ex.Message}");
                        }
                    }
                }

                stopwatch.Stop();
                testResult.DurationMs = stopwatch.ElapsedMilliseconds;
                lock (results)
                {
                    results.Add(testResult);
                }
            }
        });

        await Task.WhenAll(testTasks);

        var afterClassMethods = testClass.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.GetCustomAttribute<AfterClassAttribute>() != null);
        foreach (var method in afterClassMethods)
        {
            try
            {
                method.Invoke(null, null);
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException ?? ex;
                Console.WriteLine(
                    $"Warning: AfterClass method '{method.Name}' in '{testClass.FullName}' threw: {inner.Message}");
            }
        }

        return results;
    }

    private static void PrintResults(List<ResultModel> results)
    {
        var passed = results.Where(r => r.IsSuccess).ToList();
        var failed = results.Where(r => r is { IsSuccess: false, IsIgnored: false }).ToList();
        var ignored = results.Where(r => r.IsIgnored).ToList();

        Console.WriteLine("\n=== Test Run Summary ===");
        Console.WriteLine(
            $"Total: {results.Count}, Passed: {passed.Count}, Failed: {failed.Count}, Ignored: {ignored.Count}");
        Console.WriteLine();

        if (failed.Count != 0)
        {
            Console.WriteLine("=== FAILED TESTS ===");
            foreach (var f in failed)
            {
                Console.WriteLine($"{f.TestName} ({f.DurationMs} ms)");
                if (!string.IsNullOrEmpty(f.ErrorMessage))
                {
                    var firstLine = f.ErrorMessage.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault() ?? f.ErrorMessage;
                    Console.WriteLine($"   {firstLine}");
                }

                Console.WriteLine();
            }
        }

        if (ignored.Any())
        {
            Console.WriteLine("=== IGNORED TESTS ===");
            foreach (var i in ignored)
            {
                Console.WriteLine($"➖ {i.TestName}: {i.IgnoreReason}");
            }

            Console.WriteLine();
        }

        if (passed.Count != 0)
        {
            Console.WriteLine("=== PASSED TESTS ===");
            foreach (var p in passed)
            {
                Console.WriteLine($"{p.TestName} ({p.DurationMs} ms)");
            }

            Console.WriteLine();
        }
    }
}