using System.Drawing;
using System.Numerics;
using OpenPixel2D.Content;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

internal readonly record struct TextRenderItem(
    AssetPath Asset,
    string Text,
    Vector2 Position,
    float Size,
    Color Colour) : IRenderItem;
