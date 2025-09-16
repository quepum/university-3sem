// <copyright file="Program.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Homework1;

using BenchmarkDotNet.Running;

/// <summary>
/// Main program for running matrix multiplication benchmarks.
/// </summary>
public static class Program
{
    /// <summary>
    /// Main entry point.
    /// </summary>
    public static void Main()
    {
        Console.WriteLine("Matrix Multiplication Performance Benchmark");
        Console.WriteLine("==========================================");
        Console.WriteLine("Testing square matrices with sizes: 100, 500, 1000, 2000");
        Console.WriteLine();

        var summary = BenchmarkRunner.Run<MatrixBenchmark>();

        Console.WriteLine("\nBenchmark completed successfully!");
        Console.WriteLine($"Results in: {summary.ResultsDirectoryPath}");
        Console.WriteLine("\nPress any key");
        Console.ReadKey();
    }
}