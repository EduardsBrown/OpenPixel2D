using System.Drawing;
using System.Numerics;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class SpriteRenderItemProcessorTests
{
    [Fact]
    public void Process_SpriteItems_PopulatesWorldSpritesPassWithSimplifiedCommands()
    {
        RenderPassRegistry registry = new();
        registry.RegisterDefaultPasses();
        RenderFrame frame = new(registry);
        RenderQueue queue = new();
        queue.Submit(new SpriteRenderItem(
            new AssetId("player"),
            new Vector2(32f, 48f),
            new Vector2(1.5f, 2f),
            0.5f,
            16f,
            24f,
            Color.Crimson));

        SpriteRenderItemProcessor processor = new();

        processor.Process(queue.GetItems<SpriteRenderItem>(), new RenderPipelineContext(frame, queue));

        RenderPassBuffer pass = Assert.Single(frame.GetPopulatedPasses());
        SpriteRenderCommand command = Assert.IsType<SpriteRenderCommand>(Assert.Single(pass.Commands));

        Assert.Equal(RenderPassNames.WorldSprites, pass.Descriptor.Name);
        Assert.Equal(new TextureId("player"), command.TextureId);
        Assert.Equal(new Vector2(32f, 48f), command.Position);
        Assert.Equal(new Vector2(1.5f, 2f), command.Scale);
        Assert.Equal(0.5f, command.Rotation);
        Assert.Equal(16f, command.Width);
        Assert.Equal(24f, command.Height);
        Assert.Equal(Color.Crimson, command.Colour);
        Assert.Equal(0, command.Metadata.Layer);
        Assert.Equal(0L, command.Metadata.SortKey);
        Assert.Equal(RenderSpace.World, command.Metadata.Space);
    }

    [Fact]
    public void Process_SpriteItems_SortsByPositionYAndPreservesSubmissionOrderForTies()
    {
        RenderPassRegistry registry = new();
        registry.RegisterDefaultPasses();
        RenderFrame frame = new(registry);
        RenderQueue queue = new();
        queue.Submit(new SpriteRenderItem(
            new AssetId("first"),
            new Vector2(10f, 20f),
            Vector2.One,
            0f,
            8f,
            8f,
            Color.Red));
        queue.Submit(new SpriteRenderItem(
            new AssetId("second"),
            new Vector2(10f, 10f),
            Vector2.One,
            0f,
            8f,
            8f,
            Color.Green));
        queue.Submit(new SpriteRenderItem(
            new AssetId("third"),
            new Vector2(10f, 20f),
            Vector2.One,
            0f,
            8f,
            8f,
            Color.Blue));

        SpriteRenderItemProcessor processor = new();

        processor.Process(queue.GetItems<SpriteRenderItem>(), new RenderPipelineContext(frame, queue));

        RenderPassBuffer pass = Assert.Single(frame.GetPopulatedPasses());

        Assert.Collection(
            pass.Commands,
            command =>
            {
                SpriteRenderCommand spriteCommand = Assert.IsType<SpriteRenderCommand>(command);
                Assert.Equal(new TextureId("second"), spriteCommand.TextureId);
                Assert.Equal(0L, spriteCommand.Metadata.SortKey);
            },
            command =>
            {
                SpriteRenderCommand spriteCommand = Assert.IsType<SpriteRenderCommand>(command);
                Assert.Equal(new TextureId("first"), spriteCommand.TextureId);
                Assert.Equal(1L, spriteCommand.Metadata.SortKey);
            },
            command =>
            {
                SpriteRenderCommand spriteCommand = Assert.IsType<SpriteRenderCommand>(command);
                Assert.Equal(new TextureId("third"), spriteCommand.TextureId);
                Assert.Equal(2L, spriteCommand.Metadata.SortKey);
            });
    }
}
