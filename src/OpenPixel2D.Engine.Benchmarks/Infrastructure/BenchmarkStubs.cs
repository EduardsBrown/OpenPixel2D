using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine.Benchmarks.Infrastructure;

internal sealed class NoOpComponent : Component
{
}

internal sealed class NoOpBehaviorComponent : BehaviorComponent
{
}

internal sealed class NoOpUpdateSystem : UpdateSystem
{
    public NoOpUpdateSystem(SystemGroup group = SystemGroup.Default)
    {
        Group = group;
    }
}

internal sealed class NoOpRenderSystem : RenderSystem
{
}
