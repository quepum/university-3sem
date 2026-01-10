// <copyright file="RequestParser.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace SimpleFTP;

/// <summary>
/// Parses a raw client request string into a structured command and path.
/// </summary>
internal static class RequestParser
{
    /// <summary>
    /// Parses a string into a command and path.
    /// </summary>
    /// <param name="input">The raw request line from the client.</param>
    /// <returns>A parsed request object or null if the format is wrong.</returns>
    public static (string Command, string Path)? Parse(string input)
    {
        var parts = input.Split(' ', 2);
        return parts.Length == 2 ? (parts[0], parts[1]) : null;
    }
}