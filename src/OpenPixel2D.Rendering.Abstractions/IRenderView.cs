namespace OpenPixel2D.Rendering.Abstractions;

public interface IRenderView
{
    string Name { get; }

    int ViewportWidth { get; }

    int ViewportHeight { get; }

    ClearOptions Clear { get; }
}
