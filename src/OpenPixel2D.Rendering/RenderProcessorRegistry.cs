using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

internal sealed class RenderProcessorRegistry
{
    private readonly List<IRenderProcessorInvoker> _processors = [];

    public void Register<T>(IRenderItemProcessor<T> processor)
        where T : struct, IRenderItem
    {
        ArgumentNullException.ThrowIfNull(processor);
        _processors.Add(new RenderProcessorInvoker<T>(processor));
    }

    public void Process(RenderQueue queue, IRenderPipelineContext context)
    {
        ArgumentNullException.ThrowIfNull(queue);
        ArgumentNullException.ThrowIfNull(context);

        for (int i = 0; i < _processors.Count; i++)
        {
            _processors[i].Process(queue, context);
        }
    }

    internal static RenderProcessorRegistry CreateDefault()
    {
        RenderProcessorRegistry registry = new();
        registry.Register(new SpriteRenderItemProcessor());
        return registry;
    }

    private interface IRenderProcessorInvoker
    {
        void Process(RenderQueue queue, IRenderPipelineContext context);
    }

    private sealed class RenderProcessorInvoker<T> : IRenderProcessorInvoker
        where T : struct, IRenderItem
    {
        private readonly IRenderItemProcessor<T> _processor;

        public RenderProcessorInvoker(IRenderItemProcessor<T> processor)
        {
            _processor = processor;
        }

        public void Process(RenderQueue queue, IRenderPipelineContext context)
        {
            _processor.Process(queue.GetItems<T>(), context);
        }
    }
}
