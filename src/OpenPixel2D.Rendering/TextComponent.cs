using System.Drawing;
using OpenPixel2D.Engine;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

public sealed class TextComponent : Component
{
    public AssetId Asset { get; set; }

    public string Text { get; set; } = string.Empty;

    public Color Colour { get; set; } = Color.White;

    public float Size { get; set; } = 1f;
}
