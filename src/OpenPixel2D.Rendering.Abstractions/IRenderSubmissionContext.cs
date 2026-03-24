namespace OpenPixel2D.Rendering.Abstractions;

public interface IRenderSubmissionContext
{
    void Submit<T>(in T item)
        where T : struct, IRenderItem;
}
