// <copyright file="Program.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyConsoleApp;

using System.Diagnostics;

/// <summary>
/// Main entry point for the directory checksum performance comparison tool.
/// </summary>
internal class Program
{
    /// <summary>
    /// Compares sequential and parallel checksum computation performance for a given directory.
    /// </summary>
    private static async Task Main(string[] args)
    {
        var dirPath = args[0];
        using var cts = new CancellationTokenSource();

        try
        {
            var sw = Stopwatch.StartNew();
            var checksum1 = await CheckSumCalculator.ComputeCheckSumAsync(dirPath, cts.Token);
            sw.Stop();
            var time1 = sw.Elapsed;

            cts.CancelAfter(Timeout.Infinite);

            sw.Restart();
            var checksum2 = await CheckSumCalculator.ComputeCheckSumParallelAsync(dirPath, cts.Token);
            sw.Stop();
            var time2 = sw.Elapsed;

            var same = checksum1.SequenceEqual(checksum2);
            Console.WriteLine($"Async sequential: {time1.TotalMilliseconds:F3} ms");
            Console.WriteLine($"Async parallel:   {time2.TotalMilliseconds:F3} ms");
            Console.WriteLine($"Checksums match:  {same}");
            Console.WriteLine($"Checksum (hex):   {ToHex(checksum1)}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\nOperation was canceled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
        }
    }

    private static string ToHex(byte[] bytes) =>
        Convert.ToHexString(bytes).ToLowerInvariant();
}