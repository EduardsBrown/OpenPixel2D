using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class RenderPassRegistryTests
{
    [Fact]
    public void Register_AddsPassesInAscendingOrder()
    {
        RenderPassRegistry registry = new();

        registry.Register(new RenderPassDescriptor(RenderPassNames.UI, 100, new RenderState()));
        registry.Register(new RenderPassDescriptor(RenderPassNames.WorldSprites, 0, new RenderState()));
        registry.Register(new RenderPassDescriptor(RenderPassNames.Debug, 200, new RenderState()));

        Assert.Collection(
            registry.Passes,
            pass => Assert.Equal(RenderPassNames.WorldSprites, pass.Name),
            pass => Assert.Equal(RenderPassNames.UI, pass.Name),
            pass => Assert.Equal(RenderPassNames.Debug, pass.Name));
    }

    [Fact]
    public void Register_PreservesRegistrationOrderWhenPassOrdersMatch()
    {
        RenderPassRegistry registry = new();

        registry.Register(new RenderPassDescriptor("First", 100, new RenderState()));
        registry.Register(new RenderPassDescriptor("Second", 100, new RenderState()));
        registry.Register(new RenderPassDescriptor("Third", 100, new RenderState()));

        Assert.Collection(
            registry.Passes,
            pass => Assert.Equal("First", pass.Name),
            pass => Assert.Equal("Second", pass.Name),
            pass => Assert.Equal("Third", pass.Name));
    }

    [Fact]
    public void Register_RejectsDuplicatePassNames()
    {
        RenderPassRegistry registry = new();
        registry.Register(new RenderPassDescriptor(RenderPassNames.UI, 100, new RenderState()));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => registry.Register(new RenderPassDescriptor(RenderPassNames.UI, 200, new RenderState())));

        Assert.Contains(RenderPassNames.UI, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TryGetPass_ReturnsRegisteredDescriptor()
    {
        RenderPassRegistry registry = new();
        RenderPassDescriptor descriptor = new(RenderPassNames.WorldSprites, 0, new RenderState(SamplerMode: SamplerMode.PointClamp));

        registry.Register(descriptor);

        bool found = registry.TryGetPass(RenderPassNames.WorldSprites, out RenderPassDescriptor result);

        Assert.True(found);
        Assert.Equal(descriptor, result);
    }
}
