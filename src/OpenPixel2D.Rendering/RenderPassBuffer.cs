using System.Collections.ObjectModel;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

public sealed class RenderPassBuffer : IRenderPassWriter, IRenderCompletedPass
{
    private readonly List<IRenderCommand> _commands = [];
    private readonly ReadOnlyCollection<IRenderCommand> _readonlyCommands;

    public RenderPassBuffer(RenderPassDescriptor descriptor)
    {
        Descriptor = descriptor;
        _readonlyCommands = _commands.AsReadOnly();
    }

    public RenderPassDescriptor Descriptor { get; }

    public IReadOnlyList<IRenderCommand> Commands => _readonlyCommands;

    public bool HasCommands => _commands.Count > 0;

    void IRenderPassWriter.Submit<TCommand>(in TCommand command)
    {
        _commands.Add(command);
    }

    internal void Clear()
    {
        _commands.Clear();
    }
}
