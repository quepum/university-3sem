// <copyright file="FtpClient.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Client;

using System.Buffers;
using System.IO;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// A simple FTP client that can send requests to a server.
/// </summary>
public class FtpClient(string host, int port)
{
    /// <summary>
    /// Lists files and directories in the specified path on the server.
    /// </summary>
    /// <param name="path">Relative path to the directory.</param>
    /// <returns>An array of <see cref="ListEntry"/> or null if the directory does not exist.</returns>
    public async Task<ListEntry[]?> ListAsync(string path)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(host, port);
        await using var stream = client.GetStream();

        var request = $"1 {path}\n";
        await stream.WriteAsync(Encoding.UTF8.GetBytes(request));

        using var reader = new StreamReader(stream, Encoding.UTF8);
        var line = await reader.ReadLineAsync();
        if (string.IsNullOrEmpty(line))
        {
            return null;
        }

        var parts = line.Split(' ');
        if (parts.Length == 0 || !int.TryParse(parts[0], out var count) || count == -1)
        {
            return null;
        }

        var entries = new ListEntry[count];
        for (var i = 0; i < count; i++)
        {
            var name = parts[1 + (i * 2)];
            var isDirStr = parts[1 + (i * 2) + 1];
            var isDir = isDirStr == "true";
            entries[i] = new ListEntry(name, isDir);
        }

        return entries;
    }

    /// <summary>
    /// Downloads a file from the server and writes it to the provided destination stream.
    /// </summary>
    /// <param name="path">Relative path to the file.</param>
    /// <param name="destination">Stream to write file content to.</param>
    /// <returns>File size in bytes, or -1 if the file does not exist.</returns>
    public async Task<long> GetFileToAsync(string path, Stream destination)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(host, port);
        await using var stream = client.GetStream();

        var request = $"2 {path}\n";
        await stream.WriteAsync(Encoding.UTF8.GetBytes(request));

        var headerBuilder = new StringBuilder();
        var buffer = new byte[1];
        while (true)
        {
            var read = await stream.ReadAsync(buffer);
            if (read == 0)
            {
                if (headerBuilder.ToString() == "-1")
                {
                    return -1;
                }

                throw new EndOfStreamException("Unexpected end of stream while reading header.");
            }

            if (buffer[0] == (byte)' ')
            {
                break;
            }

            headerBuilder.Append((char)buffer[0]);
        }

        var header = headerBuilder.ToString();
        if (!long.TryParse(header, out var size) || size == -1)
        {
            return -1;
        }

        var totalRead = 0L;
        const int bufferSize = 8192;
        var readBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            while (totalRead < size)
            {
                var toRead = (int)Math.Min(bufferSize, size - totalRead);
                var bytesRead = await stream.ReadAtLeastAsync(readBuffer, toRead, false);
                if (bytesRead == 0)
                {
                    throw new EndOfStreamException("Unexpected end of stream during file download.");
                }

                await destination.WriteAsync(readBuffer.AsMemory(0, bytesRead));
                totalRead += bytesRead;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(readBuffer);
        }

        return size;
    }
}

/// <summary>
/// Represents a single entry in a directory listing.
/// </summary>
/// <param name="Name">File or directory name.</param>
/// <param name="IsDirectory">True if it's a directory.</param>
public record ListEntry(string Name, bool IsDirectory);