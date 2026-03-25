using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

internal sealed class RenderPipelineContext : IRenderContext, IRenderPipelineContext
{
    public RenderPipelineContext(IRenderFrame frame, IRenderSubmissionContext submission, IRenderView? view = null)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(submission);

        Frame = frame;
        Submission = submission;
        View = view;
    }

    public IRenderFrame Frame { get; }

    public IRenderSubmissionContext Submission { get; }

    public IRenderView? View { get; private set; }

    internal void SetView(IRenderView? view)
    {
        View = view;
    }
}
