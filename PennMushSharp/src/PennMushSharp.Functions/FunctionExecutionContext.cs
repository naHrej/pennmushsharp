using PennMushSharp.Core;

namespace PennMushSharp.Functions;

public sealed class FunctionExecutionContext
{
  private FunctionExecutionContext(GameObject actor, RegisterSet registers)
  {
    Actor = actor;
    Registers = registers;
  }

  public GameObject Actor { get; }
  public RegisterSet Registers { get; }

  public string? GetRegister(string name) => Registers.GetNamed(name);

  public void SetRegister(string name, string value) => Registers.SetNamed(name, value);

  public string? GetArgument(int index) => Registers.GetArgument(index);

  public static FunctionExecutionContext FromRegisters(GameObject actor, RegisterSet registers, string? rawArguments)
  {
    registers.LoadArguments(rawArguments);
    return new FunctionExecutionContext(actor, registers);
  }

  public static FunctionExecutionContext FromArguments(GameObject actor, string? rawArguments)
  {
    var registers = new RegisterSet();
    registers.LoadArguments(rawArguments);
    return new FunctionExecutionContext(actor, registers);
  }

  public void ClearRegisters() => Registers.ClearAll();
}
