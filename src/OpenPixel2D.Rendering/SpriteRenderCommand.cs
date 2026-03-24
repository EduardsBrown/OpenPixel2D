using System.Drawing;
using System.Numerics;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

internal readonly record struct SpriteRenderCommand(
    RenderCommandMetadata Metadata,
    TextureId TextureId,
    Vector2 Position,
    Vector2 Scale,
    float Rotation,
    float Width,
    float Height,
    Color Colour) : ISpriteRenderCommand;
