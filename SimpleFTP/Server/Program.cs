// <copyright file="Program.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace SimpleFTP;

/// <summary>
/// The main program class for the FTP server.
/// </summary>
internal static class Program
{
    private const int Port = 8888;

    /// <summary>
    /// The entry point of the server application.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public static async Task Main()
    {
        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            Console.WriteLine("[Server] Shutting down...");
            e.Cancel = true;
            cts.Cancel();
        };

        var server = new FtpServer(port: Port);
        await server.StartAsync(cts.Token);

        Console.WriteLine("[Server] Stopped.");
    }
}