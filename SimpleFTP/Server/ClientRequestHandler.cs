// <copyright file="ClientRequestHandler.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace SimpleFTP;

using System.Text;

/// <summary>
/// Handles incoming client requests and calls the appropriate response method.
/// </summary>
public static class ClientRequestHandler
{
    /// <summary>
    /// Reads a request from the client stream and sends a response.
    /// </summary>
    /// <param name="stream">The network stream connected to the client.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public static async Task ProcessRequestAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var line = await reader.ReadLineAsync();

        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        var request = RequestParser.Parse(line);
        if (request == null)
        {
            return;
        }

        switch (request.Command)
        {
            case "1":
                await ResponseWriter.SendListResponseAsync(stream, request.Path);
                break;
            case "2":
                await ResponseWriter.SendGetResponseAsync(stream, request.Path);
                break;
        }
    }
}