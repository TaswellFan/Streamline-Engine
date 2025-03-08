using System.Numerics;
using Raylib_cs;
using StreamlineEngine.Engine.Etc;
using StreamlineEngine.Engine.Etc.Builders;
using StreamlineEngine.Engine.Etc.Classes;
using StreamlineEngine.Engine.Etc.Templates;
using StreamlineEngine.Engine.Material;
using StreamlineEngine.Engine.Object;

namespace StreamlineEngine.Engine.Component;

public class TextComponent : ComponentTemplate
{
  public PositionComponent LocalPosition { get; set; }
  public PositionComponent Position { get; set; }
  public SizeComponent LocalSize { get; set; }
  public SizeComponent Size { get; set; }
  public FontMaterial Resource { get; set; }
  public RefObj<string> Text { get; set; }
  public string[] LinedText { get; private set; }
  public float FontSize { get; set; }
  public Color Color { get; set; }
  public TextSettings Settings { get; set; }
  public Vector2 OneSymbolSize { get; set; }
  
  public TextComponent(string text, FontMaterial font, float size, Color color, TextSettings? extra = null)
  {
    Text = new(text);
    Resource = font;
    FontSize = size;
    Color = color;
    Settings = extra ?? new TextSettings.Builder().Build();
    DebugBorderColor = Color.Green;
  }
  
  public TextComponent(RefObj<string> text, FontMaterial font, float size, Color color, TextSettings? extra = null)
  {
    Text = text;
    Resource = font;
    FontSize = size;
    Color = color;
    Settings = extra ?? new TextSettings.Builder().Build();
    DebugBorderColor = Color.Green;
  }

  public Vector2 MeasureText(Context context, string msg)
  {
    bool notReady = !Resource.Ready();
    if (notReady) Resource.Load(context);
    Vector2 measure = Raylib.MeasureTextEx(Resource.Material, msg, FontSize, Settings.BetweenLetterSpacing);
    if (notReady) Resource.Unload(context);
    return measure;
  }

  public override void Init(Context context)
  {
    InitOnce(() =>
    {
      OneSymbolSize = MeasureText(context, "A");
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
      
      if (Settings is { Wrap: true, AlignX: not TextSettings.TextAlign.Negative })
        Warning(context, "X Alignment has been changed. Since wrapping is on, it may cause some visual bugs or be out of 'debug border' (because wrapping is already aligning text).");

      Parent.AddMaterials(Resource);
      Parent.LocalPosSizeToLateInit(this);
    });
  }

  public override void Update(Context context)
  {
    if (!Settings.Wrap)
    {
      LinedText = Text.Value.Split('\n');
      return;
    }

    if (Text.Value.Count(c => c == '\n') > 0)
      Warning(context, "You are using '\\n' char with wrapping turned on. Be aware or incorrect text rendering!", true);
    
    string[] words = Text.Value.Split(' ');
    
    List<string> lines = [];
    string currentLine = "";
    
    foreach (string word in words)
    {
      string temp = currentLine + word;
      if (MeasureText(context, temp).X >= Size.Width + LocalSize.Width - LocalPosition.X)
      {
        lines.Add(currentLine);
        currentLine = word + " ";
      }
      else currentLine = temp + " ";
    }
    lines.Add(currentLine);
    
    if (lines[0] == "") lines.RemoveAt(0);
    LinedText = lines.ToArray();
  }

  public override void Draw(Context context)
  {
    float offsetY = 0;
    
    float totalTextHeight = LinedText.Length * (OneSymbolSize.Y + Settings.LineSpacing) - Settings.LineSpacing;
    float offsetX = 0;
    float offsetYAdjustment = 0;
    
    if (Settings.AlignX == TextSettings.TextAlign.Center)
    {
      float totalTextWidth = 0;
      foreach (string line in LinedText) 
        totalTextWidth = Math.Max(totalTextWidth, MeasureText(context, line).X);
      offsetX = (Size.Width + LocalSize.Width - totalTextWidth) / 2;
    }
    else if (Settings.AlignX == TextSettings.TextAlign.Positive)
    {
      float totalTextWidth = 0;
      foreach (string line in LinedText) 
        totalTextWidth = Math.Max(totalTextWidth, MeasureText(context, line).X);
      offsetX = Size.Width + LocalSize.Width - totalTextWidth;
    }
    
    if (Settings.AlignY == TextSettings.TextAlign.Center) 
      offsetYAdjustment = (Size.Height + LocalSize.Height - totalTextHeight) / 2;
    else if (Settings.AlignY == TextSettings.TextAlign.Positive) 
      offsetYAdjustment = Size.Height + LocalSize.Height - totalTextHeight;
    
    foreach (string line in LinedText)
    {
      Raylib.DrawTextEx(Resource.Material, line, Position.Vec2 + LocalPosition.Vec2 + new Vector2(offsetX, offsetY + offsetYAdjustment), FontSize, Settings.BetweenLetterSpacing, Color);
      offsetY += OneSymbolSize.Y + Settings.LineSpacing;
    }
  }
}