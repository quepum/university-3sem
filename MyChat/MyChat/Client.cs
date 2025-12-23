// </copyright file="Client.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyChat;

using System.Net;
using System.Net.Sockets;

/// <summary>
/// Represents a TCP chat client that connects to a remote server.
/// </summary>
/// <param name="address">The IP address of the server.</param>
/// <param name="port">The server port.</param>
public class Client(IPAddress address, int port)
{
    /// <summary>
    /// Establishes a connection to the server and starts a chat session.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RunAsync()
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            await socket.ConnectAsync(address, port);
            Console.WriteLine($"[Client] Connected to {address}:{port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Connection failed: {ex.Message}");
            socket.Dispose();
            return;
        }

        var session = new Session(socket);
        await session.StartAsync();
    }
}