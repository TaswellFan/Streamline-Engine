using StreamlineEngine.Engine.Etc.Interfaces;
using StreamlineEngine.Engine.Etc.Templates;
using StreamlineEngine.Engine.FolderItem;

namespace StreamlineEngine.Engine.Manager;

public class ResourceManager
{
  public List<dynamic> All { get; } = [];
  
  public IMaterial GetByUuid(string uuid) => All.First(i => i.Uuid == uuid);

  public void RegisterFromStruct(MainContext context)
  {
    foreach (var item in typeof(Registration.Materials).GetFields().Select(f => f.GetValue(null)).OfType<IMaterial>().ToArray())
    {
      All.Add(item);
      item.Init(context);
    }
  }
}