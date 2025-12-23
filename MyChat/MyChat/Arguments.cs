// </copyright file="Arguments.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyChat;

using System.Net;

/// <summary>
/// Represents the arguments for the chat application.
/// </summary>
public class Arguments
{
    /// <summary>
    /// Gets or sets a value indicating whether the application should run as a server.
    /// If <see langword="true"/>, <see cref="ServerAddress"/> is <see langword="null"/>.
    /// </summary>
    public bool IsServer { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the remote server to connect to.
    /// This property is <see langword="null"/> when <see cref="IsServer"/> is <see langword="true"/>.
    /// </summary>
    public IPAddress? ServerAddress { get; set; }

    /// <summary>
    /// Gets or sets the TCP port number to listen on (server mode) or connect to (client mode).
    /// </summary>
    public int Port { get; set; }
}