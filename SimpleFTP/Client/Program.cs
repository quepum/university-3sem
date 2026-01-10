// <copyright file="Program.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

using Client;

const string defaultHost = "127.0.0.1";
const int defaultPort = 8888;

if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: client <1|2> <path> [host] [port]");
    Console.Error.WriteLine("  1 — list directory");
    Console.Error.WriteLine("  2 — get file");
    return 1;
}

var command = args[0];
var path = args[1];
var host = args.Length >= 3 ? args[2] : defaultHost;
var portStr = args.Length >= 4 ? args[3] : defaultPort.ToString();
if (!int.TryParse(portStr, out var port))
{
    Console.Error.WriteLine("Invalid port.");
    return 1;
}

if (command != "1" && command != "2")
{
    Console.Error.WriteLine("Command must be '1' or '2'.");
    return 1;
}

try
{
    var client = new FtpClient(host, port);

    if (command == "1")
    {
        var entries = await client.ListAsync(path);
        if (entries == null)
        {
            Console.WriteLine("-1");
            return 0;
        }

        Console.WriteLine(entries.Length);
        foreach (var e in entries)
        {
            Console.WriteLine($"{e.Name} {(e.IsDirectory ? "true" : "false")}");
        }
    }
    else
    {
        var size = await client.GetFileToAsync(path, Console.OpenStandardOutput());
        if (size == -1)
        {
            Console.Error.WriteLine("File not found.");
            return 1;
        }
    }

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[Client] ERROR: {ex.Message}");
    return 1;
}