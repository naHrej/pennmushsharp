namespace PennMushSharp.Space;

public interface ISpaceSimulationLoop
{
  Task TickAsync(TimeSpan delta, CancellationToken cancellationToken = default);
}

public sealed class NullSpaceSimulationLoop : ISpaceSimulationLoop
{
  public Task TickAsync(TimeSpan delta, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
