using Raylib_cs;
using StreamlineEngine.Engine;
using StreamlineEngine.Engine.Component;
using StreamlineEngine.Engine.EntityMaterial;

namespace StreamlineEngine;

public static class Registration
{
  public struct Entity
  {
    public static StaticEntity staticEntity;
  }
  
  public static void EntitiesCreation(MainContext context)
  {
    Entity.staticEntity = new(context, "HelloObject", Config.Scenes.TestingOne);
    Entity.staticEntity.AddComponent(new PositionComponent());
    Entity.staticEntity.AddComponent(new SizeComponent());
    Entity.staticEntity.AddComponent(new FigureComponent(FigureType.Rectangle, .2f));
    Entity.staticEntity.AddComponent(new ImageComponent(new ImageMaterial("Image/test.png")));
    Entity.staticEntity.AddComponent(new BorderComponent(4f, Color.Red));
    Entity.staticEntity.AddLateInit(LateInitType.Entity, () => Entity.staticEntity.Component<BorderComponent>()!.LocalPosition.Add(30));
  }

  public static void MaterialsCreation(MainContext context)
  {
    
  }
}