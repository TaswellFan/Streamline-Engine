using System.Reflection;
using StreamlineEngine.Engine.Etc;

namespace StreamlineEngine.Engine.Manager;

public class DebugManager
{
  public bool TurnedOn { get; set; } = Config.DebugMode;
  public bool Changed { get; set; } = true;
  
  public void PrintSeparator(ConsoleColor fgColor = ConsoleColor.Magenta, string message = "") {
    string space = " ";
    if (message is "") space = "";
    string prepostfix = new('=', (Console.WindowWidth - message.Length - space.Length * 2) / 2);
    
    Console.ForegroundColor = fgColor;
    Console.Write(prepostfix + space + message + space + prepostfix);
    Console.ResetColor();
    Console.WriteLine();
  }
  
  public void PrintSeparator(string message) => PrintSeparator(ConsoleColor.Magenta, message);

  public void TriggerConsts()
  {
    foreach (var invoker in typeof(Config.Defaults).GetFields(BindingFlags.CreateInstance).Select(f => f.GetValue(null)).ToList())
    {
      object dummy = invoker!;
    }
  }
  
  public void Toggle() {
    TurnedOn = !TurnedOn;
    Changed = true;
  }
}