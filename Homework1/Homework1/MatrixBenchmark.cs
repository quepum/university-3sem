// <copyright file="MatrixBenchmark.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Homework1;

using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

/// <summary>
/// Benchmark class for comparing sequential and parallel matrix multiplication performance.
/// </summary>
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
[CsvExporter]
public class MatrixBenchmark
{
    [Params(100, 500, 1000, 2000)]
    public int MatrixSize;

    private int[,]? matrixA;
    private int[,]? matrixB;
    private string? matrixAFile;
    private string? matrixBFile;

    /// <summary>
    /// Setup method called before each benchmark run.
    /// Generates test matrices, saves them to temporary files, and optimizes thread priority.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        Thread.CurrentThread.Priority = ThreadPriority.Highest;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        this.matrixA = MatrixMaker.Generate(this.MatrixSize, this.MatrixSize);
        this.matrixB = MatrixMaker.Generate(this.MatrixSize, this.MatrixSize);

        this.matrixAFile = Path.GetTempFileName();
        this.matrixBFile = Path.GetTempFileName();

        MatrixMaker.Write(this.matrixA, this.matrixAFile);
        MatrixMaker.Write(this.matrixB, this.matrixBFile);
    }

    /// <summary>
    /// Cleanup method called after each benchmark run.
    /// Deletes temporary files and performs garbage collection.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            if (File.Exists(this.matrixAFile))
            {
                File.Delete(this.matrixAFile);
            }

            if (File.Exists(this.matrixBFile))
            {
                File.Delete(this.matrixBFile);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// Benchmark for sequential matrix multiplication.
    /// </summary>
    /// <returns>The product matrix resulting from sequential multiplication.</returns>
    [Benchmark(Baseline = true)]
    public int[,] SequentialMultiplication()
    {
        var a = MatrixMaker.Read(this.matrixAFile);
        var b = MatrixMaker.Read(this.matrixBFile);
        return MatrixMultiplier.SequentialMultiplication(a, b);
    }

    /// <summary>
    /// Benchmark for parallel matrix multiplication.
    /// </summary>
    /// <returns>The product matrix resulting from parallel multiplication.</returns>
    [Benchmark]
    public int[,] ParallelMultiplication()
    {
        var a = MatrixMaker.Read(this.matrixAFile);
        var b = MatrixMaker.Read(this.matrixBFile);
        return MatrixMultiplier.ParallelMultiplication(a, b, Environment.ProcessorCount);
    }
}