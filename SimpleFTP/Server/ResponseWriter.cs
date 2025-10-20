// <copyright file="ResponseWriter.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace SimpleFTP;

using System.IO;
using System.Text;

/// <summary>
/// Sends formatted responses to the client for "list" and "get" commands.
/// </summary>
public static class ResponseWriter
{
    /// <summary>
    /// Sends a list of files and folders in the given directory.
    /// If the path does not exist, sends "-1".
    /// </summary>
    /// <param name="stream">The network stream to write the response to.</param>
    /// <param name="path">The directory path to list.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public static async Task SendListResponseAsync(Stream stream, string path)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                await WriteText(stream, "-1\n");
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
            await WriteText(stream, response);
        }
        catch
        {
            await WriteText(stream, "-1\n");
        }
    }

    /// <summary>
    /// Sends the content of a file to the client.
    /// </summary>
    /// <param name="stream">The network stream to write the response to.</param>
    /// <param name="path">The path to the file to send.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public static async Task SendGetResponseAsync(Stream stream, string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                await WriteText(stream, "-1");
                return;
            }

            var content = await File.ReadAllBytesAsync(path);
            var sizeStr = content.Length.ToString();

            var header = Encoding.UTF8.GetBytes(sizeStr + " ");

            await stream.WriteAsync(header);
            await stream.WriteAsync(content);
        }
        catch
        {
            await WriteText(stream, "-1");
        }
    }

    private static async Task WriteText(Stream stream, string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        await stream.WriteAsync(bytes);
    }
}