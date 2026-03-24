namespace OpenPixel2D.Rendering.Tests;

public sealed class RenderPipelineContextTests
{
    [Fact]
    public void View_DefaultsToNull()
    {
        RenderPassRegistry registry = new();
        registry.RegisterDefaultPasses();
        RenderPipelineContext context = new(new RenderFrame(registry), new RenderQueue());

        Assert.Null(context.View);
    }

    [Fact]
    public void View_CanBeProvidedWithoutChangingProcessorContract()
    {
        RenderPassRegistry registry = new();
        registry.RegisterDefaultPasses();
        RenderView view = new();
        RenderPipelineContext context = new(new RenderFrame(registry), new RenderQueue(), view);

        Assert.Same(view, context.View);
    }
}
