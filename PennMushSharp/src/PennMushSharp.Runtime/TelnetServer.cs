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
  private readonly AccountService _accountService;
  private TcpListener? _listener;

  public TelnetServer(
    ILogger<TelnetServer> logger,
    IOptions<RuntimeOptions> options,
    InMemoryGameState gameState,
    CommandDispatcher dispatcher,
    PasswordVerifier passwordVerifier,
    SessionRegistry sessionRegistry,
    AccountService accountService)
  {
    _logger = logger;
    _options = options.Value;
    _gameState = gameState;
    _dispatcher = dispatcher;
    _passwordVerifier = passwordVerifier;
    _sessionRegistry = sessionRegistry;
    _accountService = accountService;
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
    using var disposableClient = client;

    using var stream = client.GetStream();
    using var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };
    using var reader = new StreamReader(stream, Encoding.UTF8);

    await writer.WriteLineAsync("PennMushSharp telnet server.");
    await writer.WriteLineAsync("Use CONNECT <name> [<password>] to log in or CREATE <name> <password> to register.");
    await writer.WriteLineAsync("Type QUIT to disconnect.");

    var output = new TelnetOutputWriter(writer);
    GameObject? actor = null;
    Guid sessionId = Guid.Empty;

    try
    {
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

        if (actor is null)
        {
          if (!await HandlePreAuthAsync(line, writer))
            await writer.WriteLineAsync("Unknown command. Use CONNECT <name> <password>.");
          continue;
        }

        var context = new CommandContext(actor, output);
        await _dispatcher.DispatchAsync(context, line, cancellationToken);
      }
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Telnet session terminated with error.");
    }
    finally
    {
      if (sessionId != Guid.Empty)
        _sessionRegistry.Unregister(sessionId);
    }

    async Task<bool> HandlePreAuthAsync(string commandLine, StreamWriter writerInner)
    {
      var parts = commandLine.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length == 0)
        return false;

      var keyword = parts[0];

      if (keyword.Equals("CONNECT", StringComparison.OrdinalIgnoreCase))
      {
        if (parts.Length < 2)
        {
          await writerInner.WriteLineAsync("Usage: CONNECT <name> [<password>]");
          return true;
        }

        var suppliedPassword = parts.Length >= 3 ? parts[2] : string.Empty;
        if (_accountService.TryConnect(parts[1], suppliedPassword, out var connected))
        {
          actor = connected;
          sessionId = _sessionRegistry.Register(actor);
          await writerInner.WriteLineAsync($"Welcome, {actor.Name}!");
          return true;
        }

        await writerInner.WriteLineAsync("Invalid credentials.");
        return true;
      }

      if (keyword.Equals("CREATE", StringComparison.OrdinalIgnoreCase))
      {
        if (parts.Length < 3)
        {
          await writerInner.WriteLineAsync("Usage: CREATE <name> <password>");
          return true;
        }

        if (_gameState.TryGet(parts[1], out _))
        {
          await writerInner.WriteLineAsync("That name is already taken.");
          return true;
        }

        actor = _accountService.Create(parts[1], parts[2]);
        sessionId = _sessionRegistry.Register(actor);
        await writerInner.WriteLineAsync($"Created {actor.Name}. Welcome!");
        return true;
      }

      return false;
    }
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
