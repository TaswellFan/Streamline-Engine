namespace StreamlineEngine.Engine.Manager;

public class Managers
{
  public FolderManager Folder { get; } = new();
  public ItemManager Item { get; } = new();
  public DebugManager Debug { get; } = new();
  public ResourceManager Resource { get; } = new();
  public KeybindManager Keybind { get; } = new();
}