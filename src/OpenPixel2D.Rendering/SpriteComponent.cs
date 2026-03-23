using System.Drawing;
using System.Numerics;
using OpenPixel2D.Engine;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

public class SpriteComponent : Component
{
    public TextureId TextureId { get; set; }

    public RectangleF? SourceRectangle { get; set; }

    public Color Colour { get; set; } = Color.White;

    public Vector2 Origin { get; set; }

    public int Layer { get; set; }

    public long SortKey { get; set; }
}
