// <copyright file="ResponseWriter.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace SimpleFTP;

using System.IO;
using System.Text;

/// <summary>
/// Sends formatted responses to the client for "list" and "get" commands.
/// </summary>
internal static class ResponseWriter
{
    /// <summary>
    /// Sends a list of files and folders in the given directory.
    /// If the path does not exist, sends "-1".
    /// </summary>
    /// <param name="stream">The network stream to write the response to.</param>
    /// <param name="path">The directory path to list.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public static async Task SendListResponseAsync(Stream stream, string path, CancellationToken ct = default)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                await WriteTextAsync(stream, "-1\n", ct);
                return;
            }

            var entries = Directory.GetFileSystemEntries(path);
            var parts = new List<string> { entries.Length.ToString() };

            foreach (var entry in entries)
            {
                var name = Path.GetFileName(entry);
                var isDir = File.GetAttributes(entry).HasFlag(FileAttributes.Directory);
                parts.Add(name);
                parts.Add(isDir ? "true" : "false");
            }

            var response = string.Join(" ", parts) + "\n";
            await WriteTextAsync(stream, response, ct);
        }
        catch
        {
            await WriteTextAsync(stream, "-1\n", ct);
        }
    }

    /// <summary>
    /// Sends the content of a file to the client.
    /// </summary>
    /// <param name="stream">The network stream to write the response to.</param>
    /// <param name="path">The path to the file to send.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public static async Task SendGetResponseAsync(Stream stream, string path, CancellationToken ct = default)
    {
        try
        {
            if (!File.Exists(path))
            {
                await WriteTextAsync(stream, "-1", ct);
                return;
            }

            var fi = new FileInfo(path);
            var header = Encoding.UTF8.GetBytes(fi.Length + " ");
            await stream.WriteAsync(header, ct);

            await using var fileStream = File.OpenRead(path);
            await fileStream.CopyToAsync(stream, ct);
        }
        catch
        {
            await WriteTextAsync(stream, "-1", ct);
        }
    }

    private static async Task WriteTextAsync(Stream stream, string text, CancellationToken ct = default)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        await stream.WriteAsync(bytes, ct);
    }
}