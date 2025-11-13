using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using PennMushSharp.Runtime;
using Xunit;

namespace PennMushSharp.Tests.Runtime;

public sealed class TelnetServerTests
{
  [Fact]
  public async Task TelnetServer_AcceptsLookCommand()
  {
    var port = GetFreePort();
    using var host = RuntimeApplication.BuildHost(new[]
    {
      $"PennMushSharp:ListenPort={port}",
      $"PennMushSharp:InitialDumpPath="
    });

    await host.StartAsync();
    var bootstrapper = host.Services.GetRequiredService<IServerBootstrapper>();
    await bootstrapper.StartAsync();
    try
    {
      using var client = new TcpClient();
      await client.ConnectAsync(IPAddress.Loopback, port);
      using var stream = client.GetStream();
      using var reader = new StreamReader(stream, Encoding.UTF8);
      using var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

      await reader.ReadLineAsync(); // Name:
      await writer.WriteLineAsync("Wizard9");
      await reader.ReadLineAsync(); // Password:
      await writer.WriteLineAsync("harness");

      // consume greeting
      await reader.ReadLineAsync();
      await reader.ReadLineAsync();
      await reader.ReadLineAsync();

      await writer.WriteLineAsync("LOOK");
      string? response = null;
      for (var i = 0; i < 5; i++)
      {
        response = await reader.ReadLineAsync();
        if (!string.IsNullOrEmpty(response))
          break;
      }

      Assert.Contains("Room Zero", response, StringComparison.OrdinalIgnoreCase);

      await writer.WriteLineAsync("QUIT");
    }
    finally
    {
      await host.StopAsync();
    }
  }

  private static int GetFreePort()
  {
    var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    var port = ((IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    return port;
  }
}
