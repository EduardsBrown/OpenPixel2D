using System.Drawing;
using System.Numerics;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class TextRenderItemProcessorTests
{
    [Fact]
    public void Process_TextItems_PopulatesUiPassWithScreenSpaceCommands()
    {
        RenderPassRegistry registry = new();
        registry.RegisterDefaultPasses();
        RenderFrame frame = new(registry);
        RenderQueue queue = new();
        queue.Submit(new TextRenderItem(
            new AssetId("ui-font"),
            "Hello",
            new Vector2(12f, 18f),
            24f,
            Color.Gold));

        TextRenderItemProcessor processor = new();

        processor.Process(queue.GetItems<TextRenderItem>(), new RenderPipelineContext(frame, queue));

        RenderPassBuffer pass = Assert.Single(frame.GetPopulatedPasses());
        TextRenderCommand command = Assert.IsType<TextRenderCommand>(Assert.Single(pass.Commands));

        Assert.Equal(RenderPassNames.UI, pass.Descriptor.Name);
        Assert.Equal(new FontId("ui-font"), command.FontId);
        Assert.Equal("Hello", command.Text);
        Assert.Equal(new Vector2(12f, 18f), command.Position);
        Assert.Equal(24f, command.Size);
        Assert.Equal(Color.Gold, command.Colour);
        Assert.Equal(0, command.Metadata.Layer);
        Assert.Equal(0L, command.Metadata.SortKey);
        Assert.Equal(RenderSpace.Screen, command.Metadata.Space);
    }
}
