using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using StreamlineEngine.Engine.Etc;
using StreamlineEngine.Engine.Etc.Classes;
using StreamlineEngine.Engine.Etc.Interfaces;
using StreamlineEngine.Engine.Etc.Templates;
using StreamlineEngine.Engine.Object;

namespace StreamlineEngine.Engine.Component;

public class MouseHitboxComponent : ComponentTemplate, ICloneable<MouseHitboxComponent>
{
  public PositionComponent LocalPosition { get; set; }
  public PositionComponent Position { get; set; }
  public SizeComponent LocalSize { get; set; }
  public SizeComponent Size { get; set; }
  public FigureComponent Figure { get; set; }
  public BorderComponent Border { get; set; }
  public Color Color { get; set; }
  /// <summary>
  /// Returns true when hovering over hitbox with your mouse
  /// </summary>
  public bool Hover { get; private set; }
  /// <summary>
  /// Returns true when clicking with mouse button (ANY click detected)
  /// </summary>
  public bool[] Press { get; private set; } = [false, false, false];
  /// <summary>
  /// Returns true when clicking with mouse button (ONLY on the hitbox)
  /// </summary>
  public bool[] Click { get; private set; } = [false, false, false];
  /// <summary>
  /// Returns true when released mouse button
  /// </summary>
  public bool[] Release { get; private set; } = [false, false, false];
  /// <summary>
  /// Returns true when mouse button is down (ANY hold detected)
  /// </summary>
  public bool[] Down { get; private set; } = [false, false, false];
  /// <summary>
  /// Returns true when mouse button is down (ONLY on the hitbox)
  /// </summary>
  public bool[] Hold { get; private set; } = [false, false, false];
  /// <summary>
  /// Returns true when mouse button is dragged (Hold starting in hitbox only, can be released outside hitbox)
  /// </summary>
  public bool[] Drag { get; private set; } = [false, false, false];
  private bool ColorInit;

  public MouseHitboxComponent() { ColorInit = true; DebugBorderColor = Color.Red; }
  public MouseHitboxComponent(Color debugColor) { Color = debugColor; DebugBorderColor = Color.Red; }

  public override void Init(Context context)
  {
    InitOnce(() =>
    {
      if (ColorInit) Color = Defaults.DebugHitboxColor;
      
      PositionComponent? position = Parent.ComponentTry<PositionComponent>();
      if (position is null)
      {
        Warning(context, "Item has no position component. Initialising default position.");
        Position = new PositionComponent();
        Parent.AddComponents(Position);
        Position.Init(context);
      }
      else
      {
        Information(context, "Found position component!");
        Position = position;
      }
      
      SizeComponent? size = Parent.ComponentTry<SizeComponent>();
      if (size is null)
      {
        Warning(context, "Item has no size component. Initialising default size.");
        Size = new SizeComponent();
        Parent.AddComponents(Size);
        Size.Init(context);
      }
      else
      {
        Information(context, "Found size component!");
        Size = size;
      }
      
      FigureComponent? figure = Parent.ComponentTry<FigureComponent>();
      if (figure is null)
      {
        Warning(context, "Item has no figure component. Initialising default figure.");
        Figure = new FigureComponent();
        Parent.AddComponents(Figure);
        Figure.Init(context);
      }
      else
      {
        Information(context, "Found figure component!");
        Figure = figure;
      }
      
      BorderComponent? border = Parent.ComponentTry<BorderComponent>();
      if (border is not null && border.Figure.Type == FigureType.Rounded)
      {
        Information(context, "Found border component!");
        Border = border;
      }
      else
      {
        Warning(context, "Item has no border component. Initialising junk border.");
        Border = new BorderComponent{ Junk = true };
      }
      
      Parent.LocalPosSizeToLateInit(this);

      switch (Figure.Type)
      {
        case FigureType.Rounded:
          Warning(context, "Hitbox on rounded figure works the same as rectangle ones. Be careful with corners!");
          break;
        case FigureType.Circle:
          if (Math.Abs(Size.Width - Size.Height) > 0)
            Error(context, "Item's size is not square. Hitbox is now stretched to circle. Be careful!");
          break;
      }
    });
  }

  private bool DecideHover(Context context)
  {
    switch (Figure.Type)
    {
      case FigureType.Rectangle:
        return Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new Rectangle(Position.Vec2, Size.Vec2));
      case FigureType.Rounded:
        return Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new Rectangle(Position.Vec2 - new Vector2(Border.Thickness), Size.Vec2 + new Vector2(Border.Thickness * 2)));
      case FigureType.Circle:
        return Raylib.CheckCollisionPointCircle(Raylib.GetMousePosition(), new Vector2(Position.X + LocalPosition.X + Size.Width / 2 + LocalSize.Width / 2, Position.Y + LocalPosition.Y + Size.Height / 2 + LocalSize.Height / 2), Math.Max(Size.Width + LocalSize.Width, Size.Height + LocalSize.Height));
    }
    return false;
  }

  private void Reset()
  {
    Hover = false;
    Press = [false, false, false];
    Click = [false, false, false];
    Release = [false, false, false];
    Down = [false, false, false];
    Hold = [false, false, false];
    Drag = [false, false, false];
  }

  public override void Enter(Context context)
  {
    Reset();
  }

  public override void Update(Context context)
  {
    #if !RESOURCES
    if (context.Managers.Debug.TurnedOn && context.Debugger.Show) { Reset(); return; }
    #endif    

    Hover = DecideHover(context);
    for (int i = 0; i < Press.Length; i++)
    {
      Press[i] = Raylib.IsMouseButtonPressed((MouseButton)i);
      Click[i] = Press[i] && Hover;
      Release[i] = Raylib.IsMouseButtonReleased((MouseButton)i);
      Down[i] = Raylib.IsMouseButtonDown((MouseButton)i);
      Hold[i] = Down[i] && Hover;
      if (!Drag[i] & Click[i]) Drag[i] = true;
      if (Drag[i] & !Down[i]) Drag[i] = false;
    }
  }

  public override void DebugDraw(Context context)
  {
    switch (Figure.Type)
    {
      case FigureType.Rectangle or FigureType.Rounded:
        Raylib.DrawRectangleV(Position.Vec2 + LocalPosition.Vec2 - new Vector2(Border.Thickness), Size.Vec2  + LocalSize.Vec2 + new Vector2(Border.Thickness * 2), Color);
        break;
      case FigureType.Circle:
        Raylib.DrawCircle((int)(Position.X + LocalPosition.X + Size.Width / 2 + LocalSize.Width / 2), (int)(Position.Y + LocalPosition.Y + Size.Height / 2 + LocalSize.Height / 2), Math.Max(Size.Width + LocalSize.Width, Size.Height + LocalSize.Height), Color);
        break;
    }
  }
  
  public MouseHitboxComponent Clone() => (MouseHitboxComponent)MemberwiseClone();

  public override void DebuggerInfo(Context context)
  {
    base.DebuggerInfo(context);
    Extra.TransformImGuiInfo(Position, Size, Color, LocalPosition, LocalSize);
    ImGui.Separator();
    Extra.LinkToAnotherObjectImGui(context, "Figure", Figure);
    Extra.LinkToProbablyJunkObjectImGui(context, "Border", Border);
    
  }
}