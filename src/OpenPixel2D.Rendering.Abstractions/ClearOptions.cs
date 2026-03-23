using System.Drawing;

namespace OpenPixel2D.Rendering.Abstractions;

public readonly record struct ClearOptions(
    bool ClearColour = false,
    Color? Colour = null,
    bool ClearDepth = false,
    float Depth = 1f,
    bool ClearStencil = false,
    int Stencil = 0);