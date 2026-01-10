// <copyright file="Program.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

using SimpleFTP;

const int defaultPort = 8888;

var port = args.Length > 0 && int.TryParse(args[0], out var p) ? p : defaultPort;

using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("\n[Server] Shutting down...");
    e.Cancel = true;
    cts.Cancel();
};

try
{
    var server = new FtpServer(port);
    await server.StartAsync(cts.Token);
}
catch (Exception ex)
{
    Console.WriteLine($"[Server] ERROR: {ex.Message}");
}

Console.WriteLine("[Server] Stopped.");