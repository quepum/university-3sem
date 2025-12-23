// </copyright file="Session.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyChat;

using System.Net.Sockets;
using System.Text;
using System.Threading;

/// <summary>
/// Manages a bidirectional chat session over a connected <see cref="Socket"/>.
/// Handles concurrent reading (background) and writing (foreground).
/// </summary>
/// <param name="socket">An already connected socket.</param>
public class Session(Socket socket)
{
    private readonly Socket socket = socket ?? throw new ArgumentNullException(nameof(socket));

    /// <summary>
    /// Starts the chat session loop.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StartAsync()
    {
        using var cts = new CancellationTokenSource();
        var readBuffer = new byte[1024];

        var readingTask = Task.Run(() => this.ReadLoopAsync(readBuffer, cts.Token), cts.Token);

        while (Console.ReadLine() is { } input)
        {
            if (!this.socket.Connected || cts.Token.IsCancellationRequested)
            {
                break;
            }

            var cmd = input.Trim();
            var data = Encoding.UTF8.GetBytes(cmd + "\n");

            try
            {
                await this.socket.SendAsync(data, SocketFlags.None, cts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                break;
            }

            if (string.Equals(cmd, "exit", StringComparison.OrdinalIgnoreCase))
            {
                cts.Cancel();
                break;
            }
        }

        cts.Cancel();

        try
        {
            await readingTask.WaitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
            /* ignored */
        }

        try
        {
            this.socket.Shutdown(SocketShutdown.Both);
        }
        catch (SocketException)
        {
        }
        finally
        {
            this.socket.Dispose();
        }

        Console.WriteLine("[Chat] Connection closed.");
    }

    private async Task ReadLoopAsync(byte[] buffer, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && this.socket.Connected)
        {
            try
            {
                var received = await this.socket.ReceiveAsync(buffer, SocketFlags.None, ct);
                if (received == 0)
                {
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, received);
                Console.WriteLine($"[Remote] {message.TrimEnd('\r', '\n')}");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (SocketException)
            {
                break;
            }
        }
    }
}