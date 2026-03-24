namespace OpenPixel2D.Rendering.Abstractions;

public interface IRenderContext
{
    IRenderFrame Frame { get; }
    IRenderSubmissionContext Submission { get; }
}
