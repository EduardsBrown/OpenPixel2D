using System.Drawing;
using System.Numerics;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class SpriteRenderItemProcessorTests
{
    [Fact]
    public void Process_SpriteItems_PopulatesWorldSpritesPassInQueueOrder()
    {
        RenderPassRegistry registry = new();
        registry.RegisterDefaultPasses();
        RenderFrame frame = new(registry);
        RenderQueue queue = new();
        queue.Submit(new SpriteRenderItem(
            new AssetId("player"),
            new Vector2(32f, 48f),
            new Vector2(1f, 1f),
            0.5f,
            16f,
            16f,
            Color.Crimson,
            3,
            42,
            new Vector2(8f, 8f),
            new RectangleF(1f, 2f, 16f, 16f)));
        queue.Submit(new SpriteRenderItem(
            new AssetId("enemy"),
            new Vector2(12f, 14f),
            new Vector2(2f, 2f),
            1.5f,
            32f,
            32f,
            Color.DarkBlue,
            7,
            100,
            new Vector2(4f, 4f),
            null));

        SpriteRenderItemProcessor processor = new();

        processor.Process(queue.GetItems<SpriteRenderItem>(), new RenderPipelineContext(frame, queue));

        RenderPassBuffer pass = Assert.Single(frame.GetPopulatedPasses());

        Assert.Equal(RenderPassNames.WorldSprites, pass.Descriptor.Name);
        Assert.Collection(
            pass.Commands,
            command =>
            {
                SpriteRenderCommand spriteCommand = Assert.IsType<SpriteRenderCommand>(command);
                Assert.Equal(new TextureId("player"), spriteCommand.TextureId);
                Assert.Equal(new Vector2(32f, 48f), spriteCommand.Position);
                Assert.Equal(new Vector2(1f, 1f), spriteCommand.Scale);
                Assert.Equal(0.5f, spriteCommand.Rotation);
                Assert.Equal(new Vector2(8f, 8f), spriteCommand.Origin);
                Assert.Equal(new RectangleF(1f, 2f, 16f, 16f), spriteCommand.SourceRectangle);
                Assert.Equal(Color.Crimson, spriteCommand.Colour);
                Assert.Equal(3, spriteCommand.Metadata.Layer);
                Assert.Equal(42, spriteCommand.Metadata.SortKey);
                Assert.Equal(RenderSpace.World, spriteCommand.Metadata.Space);
            },
            command =>
            {
                SpriteRenderCommand spriteCommand = Assert.IsType<SpriteRenderCommand>(command);
                Assert.Equal(new TextureId("enemy"), spriteCommand.TextureId);
                Assert.Equal(new Vector2(12f, 14f), spriteCommand.Position);
                Assert.Equal(new Vector2(2f, 2f), spriteCommand.Scale);
                Assert.Equal(1.5f, spriteCommand.Rotation);
                Assert.Equal(new Vector2(4f, 4f), spriteCommand.Origin);
                Assert.Null(spriteCommand.SourceRectangle);
                Assert.Equal(Color.DarkBlue, spriteCommand.Colour);
                Assert.Equal(7, spriteCommand.Metadata.Layer);
                Assert.Equal(100, spriteCommand.Metadata.SortKey);
                Assert.Equal(RenderSpace.World, spriteCommand.Metadata.Space);
            });
    }
}
