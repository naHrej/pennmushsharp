using System.Threading.Tasks;
using PennMushSharp.Commands;
using PennMushSharp.Core;
using PennMushSharp.Core.Locks.Runtime;
using PennMushSharp.Core.Persistence;
using PennMushSharp.Functions;
using PennMushSharp.Functions.Builtins;
using Xunit;

namespace PennMushSharp.Tests.Functions;

public sealed class GetFunctionTests
{
  [Fact]
  public async Task Get_ReturnsAttributeValue()
  {
    var state = new InMemoryGameState(new InMemoryLockStore());
    var record = new GameObjectRecord { DbRef = 1, Name = "One" };
    record.SetAttribute("DESC", "Hello");
    state.Upsert(record);

    var function = new GetFunction(state);
    var context = FunctionExecutionContext.FromArguments(GameObject.FromRecord(record), null);

    var result = await function.InvokeAsync(context, new[] { "me/DESC" });

    Assert.Equal("Hello", result);
  }

  [Fact]
  public async Task XGet_ReturnsAttributeValueWithoutEvaluation()
  {
    var state = new InMemoryGameState(new InMemoryLockStore());
    var record = new GameObjectRecord { DbRef = 1, Name = "One" };
    record.SetAttribute("DESC", "[add(1,2)]");
    state.Upsert(record);

    var function = new XGetFunction(state);
    var context = FunctionExecutionContext.FromArguments(GameObject.FromRecord(record), null);

    var result = await function.InvokeAsync(context, new[] { "me/DESC" });

    Assert.Equal("[add(1,2)]", result);
  }
}
