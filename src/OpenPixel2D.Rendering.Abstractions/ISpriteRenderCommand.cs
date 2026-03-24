using System.Drawing;
using System.Numerics;

namespace OpenPixel2D.Rendering.Abstractions;

public interface ISpriteRenderCommand : IRenderCommand
{
    TextureId TextureId { get; }

    Vector2 Position { get; }

    Vector2 Scale { get; }

    float Rotation { get; }

    float Width { get; }

    float Height { get; }

    Color Colour { get; }
}
