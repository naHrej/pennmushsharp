using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PennMushSharp.Runtime;
using Xunit;

namespace PennMushSharp.Tests.Characterization;

public sealed class ManagedCharacterizationTests
{
  public static IEnumerable<object[]> ScenarioData
  {
    get
    {
      foreach (var scenario in CharacterizationScenario
        .LoadAll(CharacterizationTestHelpers.ScenarioDirectory)
        .Where(s => s.Name.StartsWith("functions_", StringComparison.OrdinalIgnoreCase)))
        yield return new object[] { scenario };
    }
  }

  [Theory]
  [MemberData(nameof(ScenarioData))]
  public async Task ManagedRuntime_MatchesGoldenFunctionOutput(CharacterizationScenario scenario)
  {
    var goldenPath = Path.Combine(CharacterizationTestHelpers.GoldenDirectory, $"{scenario.Name}.log");
    Assert.True(File.Exists(goldenPath), $"Golden transcript '{goldenPath}' not found.");

    var actualTranscript = await ExecuteScenarioAsync(scenario);
    var goldenTranscript = File.ReadAllLines(goldenPath, Encoding.UTF8).Select(line => line.TrimEnd('\r'));

    var actual = ExtractSignificantLines(actualTranscript);
    var expected = ExtractSignificantLines(goldenTranscript);

    Assert.NotEmpty(expected);
    Assert.NotEmpty(actual);
    Assert.Equal(expected, actual);
  }

  private static IReadOnlyList<string> ExtractSignificantLines(IEnumerable<string> lines) =>
    lines
      .Where(line => line.StartsWith("FUNC_", StringComparison.OrdinalIgnoreCase))
      .ToArray();

  private static async Task<IReadOnlyList<string>> ExecuteScenarioAsync(CharacterizationScenario scenario)
  {
    var port = GetFreePort();
    var dumpPath = Path.Combine(CharacterizationTestHelpers.RepositoryRoot, "data", "indb");
    var accountsPath = Path.Combine(Path.GetTempPath(), "pennmushsharp-tests", Guid.NewGuid().ToString("N"), "accounts.dump");
    Directory.CreateDirectory(Path.GetDirectoryName(accountsPath)!);

    using var host = RuntimeApplication.BuildHost(new[]
    {
      $"PennMushSharp:ListenAddress=127.0.0.1",
      $"PennMushSharp:ListenPort={port}",
      $"PennMushSharp:InitialDumpPath={dumpPath}",
      $"PennMushSharp:AccountStorePath={accountsPath}"
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

      var transcript = new List<string>();
      var readTask = Task.Run(async () =>
      {
        string? line;
        while ((line = await reader.ReadLineAsync()) is not null)
          transcript.Add(line);
      });

      var connectCommand = string.IsNullOrWhiteSpace(scenario.Password)
        ? $"CONNECT {scenario.User}"
        : $"CONNECT {scenario.User} {scenario.Password}";
      await writer.WriteLineAsync(connectCommand);
      await Task.Delay(100);

      foreach (var command in scenario.Commands)
      {
        await writer.WriteLineAsync(command);
        await Task.Delay(50);
      }

      if (!string.IsNullOrWhiteSpace(scenario.DisconnectCommand))
      {
        await writer.WriteLineAsync(scenario.DisconnectCommand);
      }
      else
      {
        await writer.WriteLineAsync("QUIT");
      }

      await writer.FlushAsync();

      var completed = await Task.WhenAny(readTask, Task.Delay(TimeSpan.FromSeconds(5)));
      if (completed != readTask)
      {
        client.Close();
        await readTask;
      }

      return transcript;
    }
    finally
    {
      await host.StopAsync();
      TryCleanup(accountsPath);
    }
  }

  private static void TryCleanup(string path)
  {
    try
    {
      var directory = Path.GetDirectoryName(path);
      if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
        Directory.Delete(directory, recursive: true);
    }
    catch
    {
      // best-effort cleanup
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
