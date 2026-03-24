using System.Drawing;
using System.Numerics;

namespace OpenPixel2D.Rendering.Abstractions;

public interface ITextRenderCommand : IRenderCommand
{
    FontId FontId { get; }

    string Text { get; }

    Vector2 Position { get; }

    float Size { get; }

    Color Colour { get; }
}
