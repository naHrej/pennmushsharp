namespace PennMushSharp.Runtime;

public interface IServerBootstrapper
{
  Task StartAsync(CancellationToken cancellationToken = default);
}
