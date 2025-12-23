// </copyright file="Program.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

using MyChat;

var (valid, parsedArgs) = ArgumentsParser.Parse(args);

if (!valid)
{
    Console.WriteLine("Input error");
    return;
}

if (parsedArgs!.IsServer)
{
    await new Server(parsedArgs.Port).RunAsync();
}
else
{
    await new Client(parsedArgs.ServerAddress!, parsedArgs.Port).RunAsync();
}