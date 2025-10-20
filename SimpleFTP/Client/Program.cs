// <copyright file="Program.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace Client;

/// <summary>
/// The main program class for the FTP client.
/// </summary>
internal static class Program
{
    private const int Port = 8888;

    /// <summary>
    /// The main entry point of the client application.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public static async Task Main()
    {
        Console.WriteLine("Usage: <1|2> <path>");
        Console.WriteLine("Enter 'exit' to quit.");

        while (true)
        {
            Console.Write("> ");
            var input = await Task.Run(Console.ReadLine);

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            var parts = input.Split(' ', 2);
            if (parts.Length != 2)
            {
                Console.WriteLine("Wrong input.");
                continue;
            }

            var command = parts[0];
            var path = parts[1];

            if (command != "1" && command != "2")
            {
                Console.WriteLine("Wrong input.");
                continue;
            }

            try
            {
                var client = new FtpClient("127.0.0.1", Port);
                await client.ExecuteAsync(command, path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client] ERROR: {ex.Message}");
            }
        }
    }
}