using System.Drawing;
using OpenPixel2D.Content;
using OpenPixel2D.Engine;

namespace OpenPixel2D.Rendering;

public sealed class TextComponent : Component
{
    public AssetPath Asset { get; set; }

    public string Text { get; set; } = string.Empty;

    public Color Colour { get; set; } = Color.White;

    public float Size { get; set; } = 1f;
}
