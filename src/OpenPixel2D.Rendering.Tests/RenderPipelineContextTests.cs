using System.Drawing;
using OpenPixel2D.Rendering.Abstractions;

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
        TestRenderView view = new(
            "Main",
            320,
            180,
            new ClearOptions(ClearColour: true, Colour: Color.CornflowerBlue));
        RenderPipelineContext context = new(new RenderFrame(registry), new RenderQueue(), view);

        Assert.Same(view, context.View);
        IRenderView resolvedView = Assert.IsAssignableFrom<IRenderView>(context.View);
        Assert.Equal("Main", resolvedView.Name);
        Assert.Equal(320, resolvedView.ViewportWidth);
        Assert.Equal(180, resolvedView.ViewportHeight);
        Assert.Equal(new ClearOptions(ClearColour: true, Colour: Color.CornflowerBlue), resolvedView.Clear);
    }

    private sealed class TestRenderView : IRenderView
    {
        public TestRenderView(string name, int viewportWidth, int viewportHeight, ClearOptions clear)
        {
            Name = name;
            ViewportWidth = viewportWidth;
            ViewportHeight = viewportHeight;
            Clear = clear;
        }

        public string Name { get; }

        public int ViewportWidth { get; }

        public int ViewportHeight { get; }

        public ClearOptions Clear { get; }
    }
}
