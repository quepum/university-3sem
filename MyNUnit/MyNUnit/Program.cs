using MyNUnit;

if (args.Length != 1)
{
    Environment.Exit(1);
}

try
{
    var runner = new TestRunner(args[0]);
    runner.Run();
}
catch (Exception e)
{
    Console.WriteLine($"ERROR: {e.Message}");
    Environment.Exit(1);
}