// </copyright file="Server.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyChat;

using System.Net;
using System.Net.Sockets;

/// <summary>
/// Represents a TCP chat server that accepts exactly one client connection.
/// </summary>
/// <param name="port">The port number.</param>
public class Server(int port)
{
    /// <summary>
    /// Starts the server, listens for a single client, and runs a chat session.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RunAsync()
    {
        TcpListener listener = new(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"[Server] Listening on port {port}. Accepting only one client...");

        var clientSocket = await listener.AcceptSocketAsync();
        listener.Stop();

        Console.WriteLine("[Server] Client connected.");
        var session = new Session(clientSocket);
        await session.StartAsync();
    }
}