using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

internal interface IRenderItemProcessor<T>
    where T : struct, IRenderItem
{
    void Process(ReadOnlySpan<T> items, IRenderPipelineContext context);
}
