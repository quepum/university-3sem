// <copyright file="FtpClient.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Client;

using System.Net.Sockets;
using System.Text;

/// <summary>
/// A simple FTP client that can send requests to a server.
/// </summary>
public class FtpClient(string host, int port)
{
    /// <summary>
    /// Sends a command to the server and shows the result.
    /// </summary>
    /// <param name="command">The command: "1" for list, "2" for get.</param>
    /// <param name="path">The path to the file or directory on the server.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task ExecuteAsync(string command, string path)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(host, port);
        await using var stream = client.GetStream();

        var request = $"{command} {path}\n";
        var reqBytes = Encoding.UTF8.GetBytes(request);
        await stream.WriteAsync(reqBytes);

        switch (command)
        {
            case "1":
            {
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var response = await reader.ReadLineAsync();
                Console.WriteLine(response ?? string.Empty);
                break;
            }

            case "2":
            {
                var header = new StringBuilder();
                var buffer = new byte[1];
                while (true)
                {
                    var read = await stream.ReadAsync(buffer);
                    if (read == 0)
                    {
                        throw new EndOfStreamException("Connection closed unexpectedly.");
                    }

                    if (buffer[0] == (byte)' ')
                    {
                        break;
                    }

                    header.Append((char)buffer[0]);
                }

                if (!long.TryParse(header.ToString(), out var size) || size == -1)
                {
                    Console.WriteLine("File not found.");
                    return;
                }

                var content = new byte[size];
                await ReadExactlyAsync(stream, content);
                Console.Write($"{size} {Encoding.UTF8.GetString(content)}");
                Console.WriteLine();
                break;
            }
        }
    }

    private static async Task ReadExactlyAsync(Stream stream, byte[] buffer)
    {
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(totalRead));
            if (read == 0)
            {
                throw new EndOfStreamException("Unexpected end of stream.");
            }

            totalRead += read;
        }
    }
}