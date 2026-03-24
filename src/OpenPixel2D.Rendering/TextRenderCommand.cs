using System.Drawing;
using System.Numerics;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

internal readonly record struct TextRenderCommand(
    RenderCommandMetadata Metadata,
    FontId FontId,
    string Text,
    Vector2 Position,
    float Size,
    Color Colour) : IRenderCommand;
