using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PennMushSharp.Runtime;

using var host = RuntimeApplication.BuildHost(args);
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("runtime");

await host.StartAsync();

var bootstrapper = host.Services.GetRequiredService<IServerBootstrapper>();
await bootstrapper.StartAsync();

logger.LogInformation("PennMushSharp runtime initialized. Subsystems will attach as ports land.");

await host.StopAsync();
