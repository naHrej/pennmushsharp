using Microsoft.Extensions.Options;
using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Runtime;

public interface IAccountRepository
{
  IReadOnlyCollection<GameObjectRecord> LoadAll();
  void Save(GameObjectRecord record);
}

public sealed class TextDumpAccountRepository : IAccountRepository
{
  private readonly string _path;
  private readonly TextDumpParser _parser;
  private readonly TextDumpWriter _writer;
  private readonly object _sync = new();

  public TextDumpAccountRepository(IOptions<RuntimeOptions> options, TextDumpParser parser, TextDumpWriter writer)
  {
    var configured = options.Value.AccountStorePath;
    if (string.IsNullOrWhiteSpace(configured))
      configured = Path.Combine("data", "accounts.dump");

    if (!Path.IsPathRooted(configured))
      configured = Path.Combine(AppContext.BaseDirectory, configured);

    _path = configured;
    _parser = parser;
    _writer = writer;
  }

  public IReadOnlyCollection<GameObjectRecord> LoadAll()
  {
    lock (_sync)
    {
      if (!File.Exists(_path))
        return Array.Empty<GameObjectRecord>();

      using var stream = File.OpenRead(_path);
      return _parser.Parse(stream).ToList();
    }
  }

  public void Save(GameObjectRecord record)
  {
    lock (_sync)
    {
      var accounts = LoadAll().ToList();
      var index = accounts.FindIndex(a => a.DbRef == record.DbRef);
      if (index >= 0)
        accounts[index] = record;
      else
        accounts.Add(record);

      var directory = Path.GetDirectoryName(_path);
      if (!string.IsNullOrEmpty(directory))
        Directory.CreateDirectory(directory);

      using var stream = File.Create(_path);
      _writer.Write(accounts, stream);
    }
  }
}
