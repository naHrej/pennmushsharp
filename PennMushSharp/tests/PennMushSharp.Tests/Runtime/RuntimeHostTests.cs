using Microsoft.Extensions.DependencyInjection;
using PennMushSharp.Core.Locks.Runtime;
using PennMushSharp.Runtime;
using Xunit;

namespace PennMushSharp.Tests.Runtime;

public sealed class RuntimeHostTests
{
  [Fact]
  public async Task RuntimeHost_ComposesServices()
  {
    using var host = RuntimeApplication.BuildHost(new[]
    {
      "PennMushSharp:ListenPort=0",
      "PennMushSharp:InitialDumpPath="
    });
    await host.StartAsync();
    var bootstrapper = host.Services.GetRequiredService<IServerBootstrapper>();
    var lockService = host.Services.GetRequiredService<ILockService>();

    Assert.NotNull(lockService);
    await bootstrapper.StartAsync();
    await host.StopAsync();
  }
}
