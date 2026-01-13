// <copyright file="Program.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

using MyNUnit;

if (args.Length != 1)
{
    Console.WriteLine("ERROR: Missing path to test assemblies.");
    Console.WriteLine("Usage: MyNUnit <path_to_test_assemblies_directory>");
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