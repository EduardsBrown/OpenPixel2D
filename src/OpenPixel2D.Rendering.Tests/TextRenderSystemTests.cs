using System.Drawing;
using System.Numerics;
using OpenPixel2D.Components;
using OpenPixel2D.Content;
using OpenPixel2D.Engine;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class TextRenderSystemTests
{
    [Fact]
    public void Render_VisibleTextWithDefaults_SubmitsTextRenderItem()
    {
        TextRenderSystem system = new();
        World world = CreateStartedWorld(system);
        Entity entity = new();
        entity.AddComponent(new TransformComponent
        {
            Position = new Vector2(12f, 18f)
        });
        entity.AddComponent(new TextComponent
        {
            Asset = new AssetPath("ui-font"),
            Text = "Score"
        });
        world.AddEntity(entity);
        world.Update();

        RenderFrame frame = CreateFrame();
        RenderQueue queue = new();

        world.Render(new RenderPipelineContext(frame, queue));

        TextRenderItem item = Assert.Single(queue.GetItems<TextRenderItem>().ToArray());

        Assert.Equal(new AssetPath("ui-font"), item.Asset);
        Assert.Equal("Score", item.Text);
        Assert.Equal(new Vector2(12f, 18f), item.Position);
        Assert.Equal(1f, item.Size);
        Assert.Equal(Color.White, item.Colour);
        Assert.Empty(frame.GetPopulatedPasses());
    }

    [Fact]
    public void Render_EmptyText_SubmitsNothing()
    {
        TextRenderSystem system = new();
        World world = CreateStartedWorld(system);
        Entity entity = new();
        entity.AddComponent(new TransformComponent
        {
            Position = new Vector2(12f, 18f)
        });
        entity.AddComponent(new TextComponent
        {
            Asset = new AssetPath("ui-font"),
            Text = string.Empty
        });
        world.AddEntity(entity);
        world.Update();

        RenderQueue queue = new();

        world.Render(new RenderPipelineContext(CreateFrame(), queue));

        Assert.True(queue.GetItems<TextRenderItem>().IsEmpty);
    }

    [Fact]
    public void Render_EntityWithoutTransform_SubmitsNothing()
    {
        TextRenderSystem system = new();
        World world = CreateStartedWorld(system);
        Entity entity = new();
        entity.AddComponent(new TextComponent
        {
            Asset = new AssetPath("ui-font"),
            Text = "Score"
        });
        world.AddEntity(entity);
        world.Update();

        RenderQueue queue = new();

        world.Render(new RenderPipelineContext(CreateFrame(), queue));

        Assert.True(queue.GetItems<TextRenderItem>().IsEmpty);
    }

    [Fact]
    public void Render_InvalidTextSize_SubmitsNothing()
    {
        TextRenderSystem system = new();
        World world = CreateStartedWorld(system);
        Entity entity = new();
        entity.AddComponent(new TransformComponent
        {
            Position = new Vector2(12f, 18f)
        });
        entity.AddComponent(new TextComponent
        {
            Asset = new AssetPath("ui-font"),
            Text = "Score",
            Size = 0f
        });
        world.AddEntity(entity);
        world.Update();

        RenderQueue queue = new();

        world.Render(new RenderPipelineContext(CreateFrame(), queue));

        Assert.True(queue.GetItems<TextRenderItem>().IsEmpty);
    }

    private static World CreateStartedWorld(RenderSystem system)
    {
        World world = new();
        world.AddSystem(system);
        world.Initialize();
        world.Start();
        return world;
    }

    private static RenderFrame CreateFrame()
    {
        RenderPassRegistry registry = new();
        registry.RegisterDefaultPasses();
        return new RenderFrame(registry);
    }
}
