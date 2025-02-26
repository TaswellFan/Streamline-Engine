using Raylib_cs;
using StreamlineEngine.Engine.Etc;
using StreamlineEngine.Engine.Etc.Interfaces;
using StreamlineEngine.Engine.Etc.Templates;
using StreamlineEngine.Engine.Object;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace StreamlineEngine.Engine.Component;

public class FillComponent : ComponentTemplate, ICloneable<FillComponent>
{
  public PositionComponent LocalPosition { get; set; }
  public PositionComponent Position { get; set; }
  public SizeComponent LocalSize { get; set; }
  public SizeComponent Size { get; set; }
  public FigureComponent Figure { get; set; }
  public Color Color { get; set; }

  public FillComponent(Color color) { Color = color; DebugBorderColor = Color.White; }
  public FillComponent() { Color = Color.White; DebugBorderColor = Color.White; }

  public override void Init(Context context)
  {
    InitOnce(() =>
    {
      Position = Parent.ComponentTry<PositionComponent>() ?? Warning(context, new PositionComponent(), "Item has no position component. Initialising default position.");
      Size = Parent.ComponentTry<SizeComponent>() ?? Warning(context, new SizeComponent(), "Item has no size component. Initialising default size.");
      Figure = Parent.ComponentTry<FigureComponent>() ?? Warning(context, new FigureComponent(), "Item has no figure component. Initialising default figure.");

      Parent.LocalPosSizeToLateInit(this);
    });
  }

  public override void Draw(Context context)
  {
    switch (Figure.Type)
    {
      case FigureType.Rectangle:
        Raylib.DrawRectangleV(Position.Vec2 + LocalPosition.Vec2, Size.Vec2 + LocalSize.Vec2, Color);
        break;
      case FigureType.Rounded:
        Raylib.DrawRectangleRounded(new Rectangle(Position.Vec2 + LocalPosition.Vec2, Size.Vec2 + LocalSize.Vec2), Figure.Roundness, Defaults.RoundedSegments, Color);
        break;
      case FigureType.Circle:
        Raylib.DrawEllipse((int)(Position.X + LocalPosition.X + Size.Width / 2 + LocalSize.Width / 2), (int)(Position.Y + LocalPosition.Y + Size.Height / 2 + LocalSize.Height / 2), Size.Width + LocalSize.Width, Size.Height + LocalSize.Height, Color);
        break;
    }
  }
  
  public FillComponent Clone() => (FillComponent)MemberwiseClone();
}