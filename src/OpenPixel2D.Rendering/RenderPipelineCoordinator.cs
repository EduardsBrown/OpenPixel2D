using OpenPixel2D.Engine;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

internal sealed class RenderPipelineCoordinator
{
    private readonly RenderQueue _queue;
    private readonly RenderFrame _frame;
    private readonly RenderPipelineContext _context;
    private readonly RenderProcessorRegistry _processors;

    public RenderPipelineCoordinator()
        : this(CreateDefaultPassRegistry(), RenderProcessorRegistry.CreateDefault())
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
        ArgumentNullException.ThrowIfNull(world);

        _frame.Clear();
        _queue.Clear();
        _context.SetView(null);

        world.Render(_context);
        _processors.Process(_queue, _context);

        return _frame;
    }

    private static IRenderPassRegistry CreateDefaultPassRegistry()
    {
        RenderPassRegistry registry = new();
        registry.RegisterDefaultPasses();
        return registry;
    }
}
