// <copyright file="SimpleFtpTests.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace SimpleFTP.Tests;

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Client;

[TestFixture]
public class SimpleFtpTests
{
    private string testDir = string.Empty;
    private string originalDirectory = string.Empty;
    private int port;
    private FtpServer? server;
    private CancellationTokenSource? cts;
    private Task? serverTask;

    [SetUp]
    public async Task Setup()
    {
        this.originalDirectory = Environment.CurrentDirectory;

        this.testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(this.testDir);

        await File.WriteAllBytesAsync(
            Path.Combine(this.testDir, "hello.txt"),
            "Hello from FTP!"u8.ToArray());
        await File.WriteAllBytesAsync(Path.Combine(this.testDir, "data.bin"), [0x00, 0xFF, 0x7F]);
        Directory.CreateDirectory(Path.Combine(this.testDir, "subdir"));

        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        this.port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        Environment.CurrentDirectory = this.testDir;
        this.server = new FtpServer(this.port);
        this.cts = new CancellationTokenSource();
        this.serverTask = this.server.StartAsync(this.cts.Token);
    }

    [TearDown]
    public async Task TearDown()
    {
        Environment.CurrentDirectory = this.originalDirectory;

        this.cts?.Cancel();
        if (this.serverTask != null)
        {
            try
            {
                await this.serverTask.WaitAsync(TimeSpan.FromSeconds(2));
            }
            catch (OperationCanceledException)
            {
            }
        }

        this.cts?.Dispose();

        if (Directory.Exists(this.testDir))
        {
            Directory.Delete(this.testDir, true);
        }
    }

    [Test]
    public async Task List_ValidDirectory_ReturnsCorrectEntries()
    {
        var client = new FtpClient("127.0.0.1", this.port);
        var entries = await client.ListAsync(".");

        Assert.That(entries, Is.Not.Null);
        Assert.That(entries, Has.Length.EqualTo(3));

        var names = new HashSet<string>(entries.Select(e => e.Name));
        Assert.Multiple(() =>
        {
            Assert.That(names, Contains.Item("hello.txt"));
            Assert.That(names, Contains.Item("data.bin"));
            Assert.That(names, Contains.Item("subdir"));
            Assert.That(entries.First(e => e.Name == "subdir").IsDirectory, Is.True);
            Assert.That(entries.First(e => e.Name == "hello.txt").IsDirectory, Is.False);
        });
    }

    [Test]
    public async Task List_NonExistentDirectory_ReturnsNull()
    {
        var client = new FtpClient("127.0.0.1", this.port);
        var result = await client.ListAsync("./missing_folder");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Get_ExistingTextFile_DownloadsCorrectContent()
    {
        var client = new FtpClient("127.0.0.1", this.port);
        var memoryStream = new MemoryStream();

        var size = await client.GetFileToAsync("hello.txt", memoryStream);

        Assert.That(size, Is.EqualTo(15));
        var content = Encoding.UTF8.GetString(memoryStream.ToArray());
        Assert.That(content, Is.EqualTo("Hello from FTP!"));
    }

    [Test]
    public async Task Get_ExistingBinaryFile_DownloadsExactBytes()
    {
        var client = new FtpClient("127.0.0.1", this.port);
        var memoryStream = new MemoryStream();

        var size = await client.GetFileToAsync("data.bin", memoryStream);

        Assert.Multiple(() =>
        {
            Assert.That(size, Is.EqualTo(3));
            Assert.That(memoryStream.ToArray(), Is.EqualTo(new byte[] { 0x00, 0xFF, 0x7F }));
        });
    }

    [Test]
    public async Task Get_NonExistentFile_ReturnsMinusOne()
    {
        var client = new FtpClient("127.0.0.1", this.port);
        var memoryStream = new MemoryStream();

        var size = await client.GetFileToAsync("nonexistent.txt", memoryStream);
        Assert.That(size, Is.EqualTo(-1));
    }

    [Test]
    public async Task Get_EmptyFile_DownloadsSuccessfully()
    {
        var emptyFile = Path.Combine(this.testDir, "empty.txt");
        await File.WriteAllTextAsync(emptyFile, string.Empty);

        var client = new FtpClient("127.0.0.1", this.port);
        var memoryStream = new MemoryStream();

        var size = await client.GetFileToAsync("empty.txt", memoryStream);

        Assert.Multiple(() =>
        {
            Assert.That(size, Is.EqualTo(0));
            Assert.That(memoryStream.Length, Is.EqualTo(0));
        });
    }
}