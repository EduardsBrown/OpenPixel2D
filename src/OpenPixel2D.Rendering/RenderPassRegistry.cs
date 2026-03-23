using System.Collections.ObjectModel;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

public sealed class RenderPassRegistry : IRenderPassRegistry
{
    private readonly Dictionary<string, RenderPassDescriptor> _passesByName = new(StringComparer.Ordinal);
    private readonly List<RenderPassDescriptor> _passes = [];
    private readonly ReadOnlyCollection<RenderPassDescriptor> _readonlyPasses;

    public RenderPassRegistry()
    {
        _readonlyPasses = _passes.AsReadOnly();
    }

    public IReadOnlyList<RenderPassDescriptor> Passes => _readonlyPasses;

    public void Register(RenderPassDescriptor descriptor)
    {
        ValidatePassName(descriptor.Name, nameof(descriptor));

        if (!_passesByName.TryAdd(descriptor.Name, descriptor))
        {
            throw new InvalidOperationException($"Render pass '{descriptor.Name}' is already registered.");
        }

        _passes.Insert(GetInsertIndex(descriptor.Order), descriptor);
    }

    public bool TryGetPass(string passName, out RenderPassDescriptor descriptor)
    {
        ValidatePassName(passName, nameof(passName));
        return _passesByName.TryGetValue(passName, out descriptor);
    }

    private int GetInsertIndex(int order)
    {
        int index = 0;

        while (index < _passes.Count && _passes[index].Order <= order)
        {
            index++;
        }

        return index;
    }

    private static void ValidatePassName(string? passName, string paramName)
    {
        if (string.IsNullOrWhiteSpace(passName))
        {
            throw new ArgumentException("Render pass name cannot be null or whitespace.", paramName);
        }
    }
}
