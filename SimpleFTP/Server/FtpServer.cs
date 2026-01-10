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
    private readonly List<Task> activeTasks = [];

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
                    var task = Task.Run(() => HandleClientAsync(client, ct), ct);
                    lock (this.activeTasks)
                    {
                        this.activeTasks.Add(task);
                        this.activeTasks.RemoveAll(t => t.IsCompleted);
                    }
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
            Console.WriteLine("[Server] Listener stopped. Waiting for active clients...");

            Task[] tasksToWait;
            lock (this.activeTasks)
            {
                tasksToWait = this.activeTasks.ToArray();
            }

            if (tasksToWait.Length > 0)
            {
                try
                {
                    await Task.WhenAll(tasksToWait).WaitAsync(TimeSpan.FromSeconds(5), ct);
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("[Server] Timeout while waiting for clients to finish.");
                }
            }

            Console.WriteLine("[Server] All clients finished.");
        }
    }

    private static async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        try
        {
            using (client)
            {
                using var stream = client.GetStream();
                await ClientRequestHandler.ProcessRequestAsync(stream, ct);
            }
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            Console.WriteLine($"[Server] ERROR: {ex.Message}");
        }
    }
}