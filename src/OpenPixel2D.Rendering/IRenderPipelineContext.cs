using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

internal interface IRenderPipelineContext
{
    IRenderFrame Frame { get; }

    RenderView? View { get; }
}
