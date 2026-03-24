using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

/// <summary>
/// Minimal per-frame view state for the internal pipeline. This is the seam where future camera and
/// render-target behavior can attach without changing processor signatures.
/// </summary>
internal sealed class RenderView : IRenderView
{
    public RenderView(string name, int viewportWidth, int viewportHeight, ClearOptions clear = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Render view name cannot be null or whitespace.", nameof(name));
        }

        if (viewportWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(viewportWidth), "Viewport width must be greater than zero.");
        }

        if (viewportHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(viewportHeight), "Viewport height must be greater than zero.");
        }

        Name = name;
        ViewportWidth = viewportWidth;
        ViewportHeight = viewportHeight;
        Clear = clear;
    }

    public string Name { get; }

    public int ViewportWidth { get; }

    public int ViewportHeight { get; }

    public ClearOptions Clear { get; }
}
