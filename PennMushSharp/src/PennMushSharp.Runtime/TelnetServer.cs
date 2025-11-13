using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PennMushSharp.Commands;
using PennMushSharp.Core;
using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Runtime;

public sealed class TelnetServer : BackgroundService
{
  private readonly ILogger<TelnetServer> _logger;
  private readonly RuntimeOptions _options;
  private readonly InMemoryGameState _gameState;
  private readonly PasswordVerifier _passwordVerifier;
  private readonly CommandDispatcher _dispatcher;
  private readonly SessionRegistry _sessionRegistry;
  private TcpListener? _listener;

  public TelnetServer(
    ILogger<TelnetServer> logger,
    IOptions<RuntimeOptions> options,
    InMemoryGameState gameState,
    CommandDispatcher dispatcher,
    PasswordVerifier passwordVerifier,
    SessionRegistry sessionRegistry)
  {
    _logger = logger;
    _options = options.Value;
    _gameState = gameState;
    _dispatcher = dispatcher;
    _passwordVerifier = passwordVerifier;
    _sessionRegistry = sessionRegistry;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    if (_options.ListenPort <= 0)
    {
      _logger.LogInformation("Telnet server disabled (ListenPort <= 0).");
      return;
    }

    var address = string.IsNullOrWhiteSpace(_options.ListenAddress)
      ? IPAddress.Loopback
      : IPAddress.Parse(_options.ListenAddress);

    _listener = new TcpListener(address, _options.ListenPort);
    _listener.Start();
    _logger.LogInformation("Telnet server listening on {Address}:{Port}", address, _options.ListenPort);

    while (!stoppingToken.IsCancellationRequested)
    {
      TcpClient? client = null;
      try
      {
        client = await _listener.AcceptTcpClientAsync(stoppingToken);
        _ = Task.Run(() => HandleClientAsync(client, stoppingToken), stoppingToken);
      }
      catch (OperationCanceledException)
      {
        client?.Dispose();
        break;
      }
      catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
      {
        client?.Dispose();
        break;
      }
      catch (Exception ex)
      {
        client?.Dispose();
        _logger.LogError(ex, "Failed to accept telnet client.");
      }
    }
  }

  public override Task StopAsync(CancellationToken cancellationToken)
  {
    _listener?.Stop();
    return base.StopAsync(cancellationToken);
  }

  private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
  {
    using var _ = client;

    using var stream = client.GetStream();
    using var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };
    using var reader = new StreamReader(stream, Encoding.UTF8);

    var record = await AuthenticateAsync(reader, writer, cancellationToken);
    if (record is null)
      return;

    var actor = new GameObject(record.DbRef, record.Name ?? $"#{record.DbRef}");
    var sessionId = _sessionRegistry.Register(actor);

    try
    {
      await writer.WriteLineAsync("Welcome to PennMushSharp!");
      await writer.WriteLineAsync("Type LOOK or WHO to exercise the current runtime.");
      await writer.WriteLineAsync("Type QUIT to disconnect.");

      var context = new CommandContext(actor, new TelnetOutputWriter(writer));

      while (!cancellationToken.IsCancellationRequested)
      {
        var line = await reader.ReadLineAsync();
        if (line is null)
          break;

        line = line.Trim();
        if (line.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
          break;

        if (string.IsNullOrWhiteSpace(line))
          continue;

        await _dispatcher.DispatchAsync(context, line, cancellationToken);
      }
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Telnet session terminated with error.");
    }
    finally
    {
      _sessionRegistry.Unregister(sessionId);
    }
  }

  private async Task<GameObjectRecord?> AuthenticateAsync(StreamReader reader, StreamWriter writer, CancellationToken cancellationToken)
  {
    for (var attempt = 0; attempt < 3; attempt++)
    {
      await writer.WriteLineAsync("Name:");
      var name = await reader.ReadLineAsync();
      if (string.IsNullOrWhiteSpace(name))
      {
        await writer.WriteLineAsync("Please provide a player name.");
        continue;
      }

      name = name.Trim();

      if (!_gameState.TryGet(name, out var record) || record is null)
      {
        await writer.WriteLineAsync("Unknown player.");
        continue;
      }

      await writer.WriteLineAsync("Password:");
      var password = (await reader.ReadLineAsync() ?? string.Empty).Trim();
      if (_passwordVerifier.Verify(record, password))
        return record;

      await writer.WriteLineAsync("Invalid password.");
    }

    await writer.WriteLineAsync("Too many failed attempts.");
    return null;
  }

  private sealed class TelnetOutputWriter : IOutputWriter
  {
    private readonly StreamWriter _writer;

    public TelnetOutputWriter(StreamWriter writer)
    {
      _writer = writer;
    }

    public ValueTask WriteLineAsync(string text, CancellationToken cancellationToken = default)
    {
      return new ValueTask(_writer.WriteLineAsync(text.AsMemory(), cancellationToken));
    }
  }

  private sealed class CommandContext : ICommandContext
  {
    public CommandContext(GameObject actor, IOutputWriter output)
    {
      Actor = actor;
      Output = output;
    }

    public GameObject Actor { get; }
    public IOutputWriter Output { get; }
  }
}
