// <copyright file="Program.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace SimpleFTP;

/// <summary>
/// Parses a raw client request string into a structured command and path.
/// </summary>
public class RequestParser
{
    /// <summary>
    /// Parses a string into a command and path.
    /// </summary>
    /// <param name="input">The raw request line from the client.</param>
    /// <returns>A parsed request object or null if the format is wrong.</returns>
    public static FtpRequest? Parse(string input)
    {
        var parts = input.Split(' ', 2);
        return parts.Length != 2 ? null : new FtpRequest(parts[0], parts[1]);
    }
}