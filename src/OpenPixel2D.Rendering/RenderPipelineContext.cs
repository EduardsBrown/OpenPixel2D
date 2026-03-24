using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

internal sealed class RenderPipelineContext : IRenderContext, IRenderPipelineContext
{
    public RenderPipelineContext(IRenderFrame frame, IRenderSubmissionContext submission)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(submission);

        Frame = frame;
        Submission = submission;
    }

    public IRenderFrame Frame { get; }

    public IRenderSubmissionContext Submission { get; }
}
