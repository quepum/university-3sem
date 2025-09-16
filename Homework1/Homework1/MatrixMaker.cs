// <copyright file="MatrixMaker.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Homework1;

/// <summary>
/// Provides methods for reading, writing, and generating matrices.
/// </summary>
public static class MatrixMaker
{
    /// <summary>
    /// Generates random matrix with specified sizes.
    /// </summary>
    /// <param name="rows">Number of rows in the matrix.</param>
    /// <param name="cols">Number of columns in the matrix.</param>
    /// <returns>A randomly generated matrix.</returns>
    public static int[,] Generate(int rows, int cols)
    {
        var matrix = new int[rows, cols];
        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                matrix[i, j] = Random.Shared.Next(0, 10);
            }
        }

        return matrix;
    }

    /// <summary>
    /// Reads a matrix from given text file.
    /// </summary>
    /// <param name="file">The path to the file.</param>
    /// <returns>An array representing the matrix.</returns>
    public static int[,] Read(string? file)
    {
        if (!File.Exists(file))
        {
            throw new FileNotFoundException($"File not found: {file}");
        }

        var lines = File.ReadAllLines(file);
        if (lines.Length == 0)
        {
            throw new ArgumentException("File is empty.");
        }

        var rows = lines.Length;
        var cols = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

        var matrix = new int[rows, cols];
        for (var i = 0; i < rows; i++)
        {
            var values = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (var j = 0; j < cols; j++)
            {
                matrix[i, j] = int.Parse(values[j]);
            }
        }

        return matrix;
    }

    /// <summary>
    /// Writes given matrix to the file.
    /// </summary>
    /// <param name="matrix">The matrix to write.</param>
    /// <param name="file">The path to the output file.</param>
    public static void Write(int[,]? matrix, string? file)
    {
        ArgumentNullException.ThrowIfNull(matrix);

        using var writer = new StreamWriter(file ?? throw new ArgumentNullException(nameof(file)));
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                writer.Write(matrix[i, j] + " ");
            }

            writer.WriteLine();
        }
    }
}