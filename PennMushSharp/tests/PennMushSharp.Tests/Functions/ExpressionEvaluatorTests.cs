using System.Collections.Generic;
using System.Threading.Tasks;
using PennMushSharp.Core;
using PennMushSharp.Functions;
using PennMushSharp.Functions.Builtins;
using Xunit;

namespace PennMushSharp.Tests.Functions;

public sealed class ExpressionEvaluatorTests
{
  [Fact]
  public async Task EvaluateAsync_ExtendsRegistersAndSetq()
  {
    var context = CreateContext();
    var evaluator = CreateEvaluator();

    var output = await evaluator.EvaluateAsync(context, "[setq(foo,Hello World)]%qfoo %10");

    Assert.Equal("Hello World lambda", output);
  }

  [Fact]
  public async Task EvaluateAsync_AllowsBareLeadingFunctionInvocation()
  {
    var evaluator = CreateEvaluator();
    var context = CreateContext();

    var output = await evaluator.EvaluateAsync(context, "setq(foo,Hello)");

    Assert.Equal(string.Empty, output);
    Assert.Equal("Hello", context.GetRegister("foo"));
  }

  [Fact]
  public async Task EvaluateAsync_DoesNotInvokeFunctionsWhenNotLeading()
  {
    var evaluator = CreateEvaluator();
    var context = CreateContext();

    var input = "Prefix setq(foo,Hello)";
    var output = await evaluator.EvaluateAsync(context, input);

    Assert.Equal(input, output);
    Assert.Null(context.GetRegister("foo"));
  }

  [Fact]
  public async Task EvaluateAsync_AllowsBareLeadingInvocationWithTrailingText()
  {
    var evaluator = CreateEvaluator();
    var context = CreateContext();

    var output = await evaluator.EvaluateAsync(context, "setq(foo,Hello) %qfoo world");

    Assert.Equal(" Hello world", output);
  }

  private static ExpressionEvaluator CreateEvaluator()
  {
    var registryBuilder = new FunctionRegistryBuilder();
    registryBuilder.Add(new SetqFunction());
    var registry = registryBuilder.Build();
    return new ExpressionEvaluator(new FunctionEvaluator(registry));
  }

  private static FunctionExecutionContext CreateContext()
  {
    var actor = new GameObject(
      dbRef: 1,
      name: "One",
      type: GameObjectType.Player,
      owner: 1,
      location: null,
      flags: Array.Empty<string>(),
      attributes: new Dictionary<string, string>(),
      locks: new Dictionary<string, string>());

    return FunctionExecutionContext.FromArguments(actor, "alpha beta gamma delta epsilon zeta eta theta iota kappa lambda mu");
  }
}
