// <copyright file="MatrixMultiplier.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Homework1;

/// <summary>
/// Represents 2 ways of matrix multiplication: sequential and parallel.
/// </summary>
public static class MatrixMultiplier
{
    /// <summary>
    /// Multiplies two matrices sequentially using the standard algorithm.
    /// </summary>
    /// <param name="matrixA">The first matrix to multiply.</param>
    /// <param name="matrixB">The second matrix to multiply.</param>
    /// <returns>The result matrix.</returns>
    public static int[,] SequentialMultiplication(int[,] matrixA, int[,] matrixB)
    {
        ArgumentNullException.ThrowIfNull(matrixA);
        ArgumentNullException.ThrowIfNull(matrixB);
        ValidateMatrixDimensions(matrixA, matrixB);

        var rowsA = matrixA.GetLength(0);
        var colsB = matrixB.GetLength(1);

        var result = new int[rowsA, colsB];

        MultiplyRowRange(matrixA, matrixB, result, 0, rowsA);

        return result;
    }

    /// <summary>
    /// Multiplies two matrices in parallel using multiple threads.
    /// </summary>
    /// <param name="matrixA">The first matrix to multiply.</param>
    /// <param name="matrixB">The second matrix to multiply.</param>
    /// <param name="threadCount">The number of threads.</param>
    /// <returns>The result matrix.</returns>
    public static int[,] ParallelMultiplication(int[,] matrixA, int[,] matrixB, int threadCount)
    {
        ArgumentNullException.ThrowIfNull(matrixA);
        ArgumentNullException.ThrowIfNull(matrixB);
        ValidateMatrixDimensions(matrixA, matrixB);

        if (threadCount <= 1)
        {
            return SequentialMultiplication(matrixA, matrixB);
        }

        var rowsA = matrixA.GetLength(0);
        var colsB = matrixB.GetLength(1);

        var actualThreadCount = Math.Min(threadCount, rowsA);
        var result = new int[rowsA, colsB];
        var threads = new Thread[actualThreadCount];

        var rowsPerThread = rowsA / actualThreadCount;

        for (var t = 0; t < actualThreadCount; t++)
        {
            var start = t * rowsPerThread;
            var end = Math.Min(start + rowsPerThread, rowsA);

            threads[t] = new Thread(() => MultiplyRowRange(matrixA, matrixB, result, start, end));
            threads[t].Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        return result;
    }

    private static void MultiplyRowRange(int[,] matrixA, int[,] matrixB, int[,] result, int start, int end)
    {
        var colsA = matrixA.GetLength(1);
        var colsB = matrixB.GetLength(1);

        for (var i = start; i < end; i++)
        {
            for (var j = 0; j < colsB; j++)
            {
                var sum = 0;
                for (var k = 0; k < colsA; k++)
                {
                    sum += matrixA[i, k] * matrixB[k, j];
                }

                result[i, j] = sum;
            }
        }
    }

    private static void ValidateMatrixDimensions(int[,] matrixA, int[,] matrixB)
    {
        var colsA = matrixA.GetLength(1);
        var rowsB = matrixB.GetLength(0);

        if (colsA != rowsB)
        {
            throw new ArgumentException("Cannot multiply matrices with such dimensions.");
        }
    }
}