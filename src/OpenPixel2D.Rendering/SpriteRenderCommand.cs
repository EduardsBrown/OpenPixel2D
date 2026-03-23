using System.Drawing;
using System.Numerics;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

public readonly record struct SpriteRenderCommand(
    RenderCommandMetadata Metadata,
    TextureId TextureId,
    Vector2 Position,
    Vector2 Scale,
    float Rotation,
    Vector2 Origin,
    RectangleF? SourceRectangle,
    Color Colour) : IRenderCommand;