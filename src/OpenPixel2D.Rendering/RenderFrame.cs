using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

public sealed class RenderFrame : IRenderFrame, IRenderCompletedFrame
{
    private readonly IRenderPassRegistry _registry;
    private readonly Dictionary<string, RenderPassBuffer> _buffers = new(StringComparer.Ordinal);

    public RenderFrame(IRenderPassRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    public IRenderPassWriter GetPass(string passName)
    {
        if (!_registry.TryGetPass(passName, out RenderPassDescriptor descriptor))
        {
            throw new InvalidOperationException($"Render pass '{passName}' is not registered.");
        }

        if (_buffers.TryGetValue(passName, out RenderPassBuffer? buffer))
        {
            return buffer;
        }

        buffer = new RenderPassBuffer(descriptor);
        _buffers.Add(passName, buffer);
        return buffer;
    }

    public IEnumerable<RenderPassBuffer> GetPopulatedPasses()
    {
        return GetPopulatedPassesCore();
    }

    IEnumerable<IRenderCompletedPass> IRenderCompletedFrame.GetPopulatedPasses()
    {
        return GetPopulatedPassesCore();
    }

    private IEnumerable<RenderPassBuffer> GetPopulatedPassesCore()
    {
        IReadOnlyList<RenderPassDescriptor> passes = _registry.Passes;

        for (int i = 0; i < passes.Count; i++)
        {
            RenderPassDescriptor descriptor = passes[i];

            if (_buffers.TryGetValue(descriptor.Name, out RenderPassBuffer? buffer) && buffer.HasCommands)
            {
                yield return buffer;
            }
        }
    }

    public void Clear()
    {
        foreach (RenderPassBuffer buffer in _buffers.Values)
        {
            buffer.Clear();
        }

        _buffers.Clear();
    }
}
