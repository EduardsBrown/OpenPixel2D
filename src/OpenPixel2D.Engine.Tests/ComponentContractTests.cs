using OpenPixel2D.Abstractions;
using OpenPixel2D.Engine;

namespace OpenPixel2D.Engine.Tests;

public sealed class ComponentContractTests
{
    [Fact]
    public void Component_IsNotStartable()
    {
        Assert.False(typeof(IStartable).IsAssignableFrom(typeof(IComponent)));
        Assert.False(typeof(IStartable).IsAssignableFrom(typeof(TestComponent)));
    }

    [Fact]
    public void BehaviorComponent_IsStartableUpdatableAndDestroyable()
    {
        Assert.True(typeof(IStartable).IsAssignableFrom(typeof(IBehaviorComponent)));
        Assert.True(typeof(IUpdatable).IsAssignableFrom(typeof(IBehaviorComponent)));
        Assert.True(typeof(IDestroyable).IsAssignableFrom(typeof(IBehaviorComponent)));
        Assert.True(typeof(IStartable).IsAssignableFrom(typeof(TestBehaviorComponent)));
        Assert.True(typeof(IUpdatable).IsAssignableFrom(typeof(TestBehaviorComponent)));
        Assert.True(typeof(IDestroyable).IsAssignableFrom(typeof(TestBehaviorComponent)));
    }

    private sealed class TestComponent : Component
    {
    }

    private sealed class TestBehaviorComponent : BehaviorComponent
    {
    }
}
