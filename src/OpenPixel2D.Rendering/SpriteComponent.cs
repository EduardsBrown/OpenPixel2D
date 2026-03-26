using System.Drawing;
using OpenPixel2D.Content;
using OpenPixel2D.Engine;

namespace OpenPixel2D.Rendering;

public class SpriteComponent : Component
{
    public AssetPath Asset { get; set; }

    public float Width { get; set; } = 1f;

    public float Height { get; set; } = 1f;

    public Color Colour { get; set; } = Color.White;
}
