using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using PennMushSharp.Core;
using PennMushSharp.Core.Metadata;
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

  [Fact]
  public async Task EvaluateAsync_SupportsBasicMathFunctions()
  {
    var evaluator = CreateEvaluator();
    var context = CreateContext();

    var add = await evaluator.EvaluateAsync(context, "[add(1,2,3)]");
    var sub = await evaluator.EvaluateAsync(context, "[sub(10,4,1)]");
    var mul = await evaluator.EvaluateAsync(context, "[mul(2,3,4)]");
    var div = await evaluator.EvaluateAsync(context, "[div(20,2)]");
    var mod = await evaluator.EvaluateAsync(context, "[mod(7,3)]");
    var sqrt = await evaluator.EvaluateAsync(context, "[sqrt(9)]");
    var power = await evaluator.EvaluateAsync(context, "[power(2,3)]");
    var ceil = await evaluator.EvaluateAsync(context, "[ceil(1.2)]");
    var floor = await evaluator.EvaluateAsync(context, "[floor(7.8)]");
    var pi = await evaluator.EvaluateAsync(context, "[pi()]");

    Assert.Equal("6", add);
    Assert.Equal("5", sub);
    Assert.Equal("24", mul);
    Assert.Equal("10", div);
    Assert.Equal("1", mod);
    Assert.Equal("3", sqrt);
    Assert.Equal("8", power);
    Assert.Equal("2", ceil);
    Assert.Equal("7", floor);
    Assert.StartsWith("3.1415", pi);
  }

  [Fact]
  public async Task EvaluateAsync_SupportsStringHelpers()
  {
    var evaluator = CreateEvaluator();
    var context = CreateContext();

    var upcase = await evaluator.EvaluateAsync(context, "[upcase(hello)]");
    var downcase = await evaluator.EvaluateAsync(context, "[downcase(HELLO)]");
    var strlen = await evaluator.EvaluateAsync(context, "[strlen(abc)]");
    var trimmed = await evaluator.EvaluateAsync(context, "[trim(__Hello__,_)]");
    var left = await evaluator.EvaluateAsync(context, "[left(foobar,3)]");
    var right = await evaluator.EvaluateAsync(context, "[right(foobar,3)]");
    var mid = await evaluator.EvaluateAsync(context, "[mid(foobar,2,3)]");
    var repeat = await evaluator.EvaluateAsync(context, "[repeat(x,4)]");

    Assert.Equal("HELLO", upcase);
    Assert.Equal("hello", downcase);
    Assert.Equal("3", strlen);
    Assert.Equal("Hello", trimmed);
    Assert.Equal("foo", left);
    Assert.Equal("bar", right);
    Assert.Equal("oba", mid);
    Assert.Equal("xxxx", repeat);
  }

  [Fact]
  public async Task EvaluateAsync_HonorsEscapedDelimiters()
  {
    var evaluator = CreateEvaluator();
    var context = CreateContext();

    var output = await evaluator.EvaluateAsync(context, @"\[\] \(\) %% \(test)");

    Assert.Equal("[] () % (test)", output);
  }

  [Fact]
  public async Task EvaluateAsync_PercentEscapesLiteralCharacters()
  {
    var evaluator = CreateEvaluator();
    var context = CreateContext();

    context.SetRegister("foo", "BAR");
    var output = await evaluator.EvaluateAsync(context, @"%[%] %% %a %qfoo");

    Assert.Equal("[] % a BAR", output);
  }

  [Fact]
  public async Task EvaluateAsync_SupportsTrigFunctions()
  {
    var evaluator = CreateEvaluator();
    var context = CreateContext();

    var sin = await evaluator.EvaluateAsync(context, "[sin(90,d)]");
    var cos = await evaluator.EvaluateAsync(context, "[cos(0,d)]");
    var tan = await evaluator.EvaluateAsync(context, "[tan(45,d)]");
    var asin = await evaluator.EvaluateAsync(context, "[asin(1,d)]");
    var atan2 = await evaluator.EvaluateAsync(context, "[atan2(1,1,d)]");

    Assert.Equal("1", sin);
    Assert.Equal("1", cos);
    Assert.Equal("1", tan);
    Assert.Equal("90", asin);
    Assert.Equal("45", atan2);
  }

  [Fact]
  public async Task EvaluateAsync_SupportsLogAndRootFunctions()
  {
    var evaluator = CreateEvaluator();
    var context = CreateContext();

    var log10 = await evaluator.EvaluateAsync(context, "[log(100)]");
    var logBase = await evaluator.EvaluateAsync(context, "[log(8,2)]");
    var ln = await evaluator.EvaluateAsync(context, "[ln(1)]");
    var root = await evaluator.EvaluateAsync(context, "[root(27,3)]");
    var ctu = await evaluator.EvaluateAsync(context, "[ctu(90,d,r)]");

    Assert.Equal("2", log10);
    Assert.Equal("3", logBase);
    Assert.Equal("0", ln);
    Assert.Equal("3", root);
    Assert.Equal("1.5707963267949", ctu);
  }

  [Fact]
  public async Task EvaluateAsync_PreservesEscapedBracketsBeforeExpressions()
  {
    var evaluator = CreateEvaluator();
    var context = CreateContext();

    var output = await evaluator.EvaluateAsync(context, @"-\[Test\] [repeat(-,3)]");

    Assert.StartsWith("-[Test]", output);
  }

  private static ExpressionEvaluator CreateEvaluator()
  {
    var metadata = PennMushSharp.Core.Metadata.MetadataCatalogs.Default.Functions;
    var registryBuilder = new FunctionRegistryBuilder(metadata);
    registryBuilder.Add(new SetqFunction());
    registryBuilder.Add(new SetrFunction());
    registryBuilder.Add(new AddFunction());
    registryBuilder.Add(new SubFunction());
    registryBuilder.Add(new MulFunction());
    registryBuilder.Add(new DivFunction());
    registryBuilder.Add(new ModFunction());
    registryBuilder.Add(new AbsFunction());
    registryBuilder.Add(new MinFunction());
    registryBuilder.Add(new MaxFunction());
    registryBuilder.Add(new CeilFunction());
    registryBuilder.Add(new FloorFunction());
    registryBuilder.Add(new PiFunction());
    registryBuilder.Add(new PowerFunction());
    registryBuilder.Add(new SqrtFunction());
    registryBuilder.Add(new UpcaseFunction());
    registryBuilder.Add(new DowncaseFunction());
    registryBuilder.Add(new StrlenFunction());
    registryBuilder.Add(new TrimFunction());
    registryBuilder.Add(new LeftTrimFunction());
    registryBuilder.Add(new RightTrimFunction());
    registryBuilder.Add(new LeftFunction());
    registryBuilder.Add(new RightFunction());
    registryBuilder.Add(new MidFunction());
    registryBuilder.Add(new RepeatFunction());
    registryBuilder.Add(new SinFunction());
    registryBuilder.Add(new CosFunction());
    registryBuilder.Add(new TanFunction());
    registryBuilder.Add(new AsinFunction());
    registryBuilder.Add(new AcosFunction());
    registryBuilder.Add(new AtanFunction());
    registryBuilder.Add(new Atan2Function());
    registryBuilder.Add(new LogFunction());
    registryBuilder.Add(new LnFunction());
    registryBuilder.Add(new RootFunction());
    registryBuilder.Add(new CtuFunction());
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
  [Fact]
  public async Task EvaluateAsync_RandProducesValuesInRange()
  {
    var evaluator = CreateEvaluator();
    var context = CreateContext();

    for (var i = 0; i < 20; i++)
    {
      var zeroArg = await evaluator.EvaluateAsync(context, "[rand()]");
      var asDouble = double.Parse(zeroArg, CultureInfo.InvariantCulture);
      Assert.InRange(asDouble, 0, 1);

      var positive = await evaluator.EvaluateAsync(context, "[rand(5)]");
      var posValue = long.Parse(positive, CultureInfo.InvariantCulture);
      Assert.InRange(posValue, 0, 4);

      var negative = await evaluator.EvaluateAsync(context, "[rand(-5)]");
      var negValue = long.Parse(negative, CultureInfo.InvariantCulture);
      Assert.InRange(negValue, -4, 0);

      var range = await evaluator.EvaluateAsync(context, "[rand(2,4)]");
      var ranged = long.Parse(range, CultureInfo.InvariantCulture);
      Assert.InRange(ranged, 2, 4);
    }
  }
