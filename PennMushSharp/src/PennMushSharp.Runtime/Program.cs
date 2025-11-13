using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PennMushSharp.Runtime;

using var host = RuntimeApplication.BuildHost(args);
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("runtime");

try
{
  await host.StartAsync();

  var bootstrapper = host.Services.GetRequiredService<IServerBootstrapper>();
  await bootstrapper.StartAsync();

  logger.LogInformation("PennMushSharp runtime initialized. Waiting for shutdown.");

  await host.WaitForShutdownAsync();
}
finally
{
  await host.StopAsync();
}
