// <copyright file="FtpRequest.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace SimpleFTP;

/// <summary>
/// Represents a client request with a command and a file path.
/// Command is "1" for list or "2" for get; path is the target directory or file.
/// </summary>
/// <param name="Command">The operation code ("1" or "2").</param>
/// <param name="Path">The relative path on the server.</param>
public record FtpRequest(string Command, string Path);