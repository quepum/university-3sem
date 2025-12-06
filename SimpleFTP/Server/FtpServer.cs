// <copyright file="FtpServer.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace SimpleFTP;

using System.Net;
using System.Net.Sockets;

/// <summary>
/// This class runs the FTP server and listens for client connections.
/// </summary>
public class FtpServer(int port)
{
    private readonly TcpListener listener = new(IPAddress.Any, port);

    /// <summary>
    /// Starts the server and accepts incoming client connections.
    /// </summary>
    /// <param name="ct">The cancellation token used to stop the server gracefully.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task StartAsync(CancellationToken ct)
    {
        this.listener.Start();
        Console.WriteLine($"Listening on port {port}...");
        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var client = await this.listener.AcceptTcpClientAsync(ct);
                    _ = Task.Run(() => HandleClientAsync(client), ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        finally
        {
            this.listener.Stop();
            Console.WriteLine("[Server] Listener stopped.");
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            {
                using var stream = client.GetStream();
                await ClientRequestHandler.ProcessRequestAsync(stream);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] ERROR: {ex.Message}");
        }
    }
}