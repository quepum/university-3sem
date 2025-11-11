// <copyright file="CheckSumCalculator.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyConsoleApp;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Computes recursive MD5 checksums of directories (sequential and parallel).
/// </summary>
public class CheckSumCalculator
{
    private static readonly Encoding Encoding = Encoding.UTF8;

    /// <summary>
    /// Computes directory checksum recursively (sequential).
    /// </summary>
    /// <param name="dirPath">Path to the directory.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>MD5 checksum as byte array.</returns>
    public static async Task<byte[]> ComputeCheckSumAsync(
        string dirPath,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(dirPath))
        {
            throw new DirectoryNotFoundException($"Directory {dirPath} not found");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var entries = Directory.GetFileSystemEntries(dirPath)
            .OrderBy(e => e, StringComparer.Ordinal)
            .ToList();
        var hashInputs = new List<byte[]>
        {
            Encoding.GetBytes(new DirectoryInfo(dirPath).Name),
        };

        foreach (var en in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            byte[] hash;
            if (Directory.Exists(en))
            {
                hash = await ComputeCheckSumAsync(en, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                hash = await ComputeFileHashAsync(en, cancellationToken).ConfigureAwait(false);
            }

            hashInputs.Add(hash);
        }

        return MD5.HashData(ConcatArrays(hashInputs));
    }

    /// <summary>
    /// Computes directory checksum recursively (parallel).
    /// </summary>
    /// <param name="dirPath">Path to the directory.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>MD5 checksum as byte array.</returns>
    public static async Task<byte[]> ComputeCheckSumParallelAsync(
        string dirPath,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(dirPath))
        {
            throw new DirectoryNotFoundException($"Directory {dirPath} not found");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var entries = Directory.GetFileSystemEntries(dirPath)
            .OrderBy(e => e, StringComparer.Ordinal)
            .ToList();

        var tasks = new List<Task<byte[]>>();
        foreach (var en in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Task<byte[]> task;
            if (Directory.Exists(en))
            {
                task = ComputeCheckSumParallelAsync(en, cancellationToken);
            }
            else
            {
                task = ComputeFileHashAsync(en, cancellationToken);
            }

            tasks.Add(task);
        }

        var results = await Task.WhenAll(tasks).WaitAsync(cancellationToken).ConfigureAwait(false);
        var hashInputs = new List<byte[]>
        {
            Encoding.GetBytes(new DirectoryInfo(dirPath).Name),
        };

        hashInputs.AddRange(results);
        return MD5.HashData(ConcatArrays(hashInputs));
    }

    private static async Task<byte[]> ComputeFileHashAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var fileNameBytes = Encoding.UTF8.GetBytes(Path.GetFileName(filePath));

        using var incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);

        incrementalHash.AppendData(fileNameBytes);

        const int bufferSize = 8192;
        var buffer = new byte[bufferSize];

        await using (var stream = File.OpenRead(filePath))
        {
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                incrementalHash.AppendData(buffer, 0, bytesRead);
            }
        }

        return incrementalHash.GetHashAndReset();
    }

    private static byte[] ConcatArrays(IEnumerable<byte[]> arrays)
    {
        return arrays.SelectMany(a => a).ToArray();
    }
}