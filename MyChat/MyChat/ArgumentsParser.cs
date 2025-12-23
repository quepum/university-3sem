// </copyright file="ArgumentsParser.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyChat;

using System.Net;

/// <summary>
/// Provides methods to parse command-line arguments for the chat application.
/// </summary>
public static class ArgumentsParser
{
    /// <summary>
    /// Parses an array of command-line arguments into a structured <see cref="Arguments"/> object.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the application.</param>
    /// <returns>
    /// A tuple where:
    /// <list type="bullet">
    ///   <item><description><c>Valid</c> is <see langword="true"/> if arguments match expected format.</description></item>
    ///   <item><description><c>Args</c> contains parsed data if valid; otherwise, <see langword="null"/>.</description></item>
    /// </list>
    /// Valid formats:
    /// <list type="number">
    ///   <item><c>[port]</c> — server mode.</item>
    ///   <item><c>[ip_address] [port]</c> — client mode.</item>
    /// </list>
    /// </returns>
    public static (bool Valid, Arguments? Args) Parse(string[] args)
    {
        if (args.Length == 1 && int.TryParse(args[0], out var port) && IsValidPort(port))
        {
            return (true, new Arguments { IsServer = true, Port = port });
        }

        if (args.Length == 2 &&
            int.TryParse(args[1], out port) &&
            IsValidPort(port) &&
            IPAddress.TryParse(args[0], out var address))
        {
            return (true, new Arguments { IsServer = false, ServerAddress = address, Port = port });
        }

        return (false, null);
    }

    private static bool IsValidPort(int port) => port is >= 1 and <= 65535;
}