using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class RenderFrameTests
{
    [Fact]
    public void GetPass_ThrowsForUnknownPassName()
    {
        RenderFrame frame = new(new RenderPassRegistry());

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => frame.GetPass("Missing"));

        Assert.Contains("Missing", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetPopulatedPasses_ReturnsOnlyUsedPassesInPassOrder()
    {
        RenderPassRegistry registry = new();
        registry.Register(new RenderPassDescriptor(RenderPassNames.UI, 100, new RenderState()));
        registry.Register(new RenderPassDescriptor(RenderPassNames.WorldSprites, 0, new RenderState()));
        registry.Register(new RenderPassDescriptor(RenderPassNames.Debug, 200, new RenderState()));

        RenderFrame frame = new(registry);
        IRenderPassWriter worldPass = frame.GetPass(RenderPassNames.WorldSprites);
        IRenderPassWriter debugPass = frame.GetPass(RenderPassNames.Debug);

        worldPass.Submit(new DummyRenderCommand(new RenderCommandMetadata(Layer: 1)));
        debugPass.Submit(new DummyRenderCommand(new RenderCommandMetadata(Layer: 2)));

        RenderPassBuffer[] passes = [.. frame.GetPopulatedPasses()];

        Assert.Collection(
            passes,
            pass => Assert.Equal(RenderPassNames.WorldSprites, pass.Descriptor.Name),
            pass => Assert.Equal(RenderPassNames.Debug, pass.Descriptor.Name));
    }

    [Fact]
    public void GetPopulatedPasses_ExcludesRequestedPassesWithoutCommands()
    {
        RenderPassRegistry registry = new();
        registry.RegisterDefaultPasses();

        RenderFrame frame = new(registry);
        frame.GetPass(RenderPassNames.UI);

        Assert.Empty(frame.GetPopulatedPasses());
    }

    [Fact]
    public void Clear_RemovesSubmittedCommandsAndLeavesNoPopulatedPasses()
    {
        RenderPassRegistry registry = new();
        registry.RegisterDefaultPasses();

        RenderFrame frame = new(registry);
        RenderPassBuffer worldPass = Assert.IsType<RenderPassBuffer>(frame.GetPass(RenderPassNames.WorldSprites));
        ((IRenderPassWriter)worldPass).Submit(new DummyRenderCommand(new RenderCommandMetadata(SortKey: 10)));

        frame.Clear();

        Assert.Empty(worldPass.Commands);
        Assert.Empty(frame.GetPopulatedPasses());
    }

    private readonly record struct DummyRenderCommand(RenderCommandMetadata Metadata) : IRenderCommand;
}
