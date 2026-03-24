using System.Drawing;
using System.Numerics;
using OpenPixel2D.Engine;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

public class SpriteComponent : Component
{
    public AssetId Asset { get; set; }

    public TextureId TextureId
    {
        get => Asset.Value is null ? default : new TextureId(Asset.Value);
        set => Asset = value.Value is null ? default : new AssetId(value.Value);
    }

    public float Width { get; set; }

    public float Height { get; set; }

    public RectangleF? SourceRectangle { get; set; }

    public Color Colour { get; set; } = Color.White;

    public Vector2 Origin { get; set; }

    public int Layer { get; set; }

    public long SortKey { get; set; }
}
