namespace OpenPixel2D.Rendering;

/// <summary>
/// Backend-facing execution seam for finished frames. Coordinators build frames first, then a backend
/// implementation consumes the completed frame and optional view state.
/// </summary>
internal interface IRenderFrameExecutor
{
    void Execute(RenderFrame frame, RenderView? view);
}
