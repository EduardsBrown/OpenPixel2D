using OpenPixel2D.Engine;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

/// <summary>
/// Builds a finished <see cref="RenderFrame"/> by running render systems into the queue and then
/// applying the built-in processors. Backend execution happens later through a separate executor seam.
/// </summary>
internal sealed class RenderPipelineCoordinator
{
    private readonly RenderQueue _queue;
    private readonly RenderFrame _frame;
    private readonly RenderPipelineContext _context;
    private readonly RenderProcessorRegistry _processors;

    public RenderPipelineCoordinator()
        : this(RenderPipelineDefaults.CreatePassRegistry(), RenderPipelineDefaults.CreateProcessorRegistry())
    {
    }

    internal RenderPipelineCoordinator(IRenderPassRegistry passRegistry, RenderProcessorRegistry processors)
    {
        ArgumentNullException.ThrowIfNull(passRegistry);
        ArgumentNullException.ThrowIfNull(processors);

        _queue = new RenderQueue();
        _frame = new RenderFrame(passRegistry);
        _processors = processors;
        _context = new RenderPipelineContext(_frame, _queue);
    }

    public RenderFrame BuildFrame(World world)
    {
        return BuildFrameCore(world, view: null);
    }

    public RenderFrame BuildFrame(World world, RenderView view)
    {
        ArgumentNullException.ThrowIfNull(view);
        return BuildFrameCore(world, view);
    }

    private RenderFrame BuildFrameCore(World world, RenderView? view)
    {
        ArgumentNullException.ThrowIfNull(world);

        _frame.Clear();
        _queue.Clear();
        _context.SetView(view);

        world.Render(_context);
        _processors.Process(_queue, _context);

        return _frame;
    }
}
