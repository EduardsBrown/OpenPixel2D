using System.Drawing;
using System.Numerics;
using OpenPixel2D.Components;
using OpenPixel2D.Content;
using OpenPixel2D.Engine;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class SpriteRenderSystemTests
{
    [Fact]
    public void SpriteComponent_Defaults_AreSimpleAndAuthorFriendly()
    {
        SpriteComponent sprite = new();

        Assert.Equal(default, sprite.Asset);
        Assert.Equal(1f, sprite.Width);
        Assert.Equal(1f, sprite.Height);
        Assert.Equal(Color.White, sprite.Colour);
        Assert.True(sprite.Active);
    }

    [Fact]
    public void Render_VisibleSpriteWithSimplifiedData_SubmitsSpriteRenderItem()
    {
        SpriteRenderSystem system = new();
        World world = CreateStartedWorld(system);
        Entity entity = new();
        entity.AddComponent(new TransformComponent
        {
            Position = new Vector2(32f, 48f),
            Scale = new Vector2(1.5f, 2f),
            Rotation = 0.5f
        });
        entity.AddComponent(new SpriteComponent
        {
            Asset = new AssetPath("player"),
            Width = 16f,
            Height = 24f,
            Colour = Color.Crimson
        });
        world.AddEntity(entity);
        world.Update();

        RenderFrame frame = CreateFrame();
        RenderQueue queue = new();

        world.Render(new RenderPipelineContext(frame, queue));

        ReadOnlySpan<SpriteRenderItem> items = queue.GetItems<SpriteRenderItem>();

        Assert.Equal(1, items.Length);
        Assert.Equal(new AssetPath("player"), items[0].Asset);
        Assert.Equal(new Vector2(32f, 48f), items[0].Position);
        Assert.Equal(new Vector2(1.5f, 2f), items[0].Scale);
        Assert.Equal(0.5f, items[0].Rotation);
        Assert.Equal(16f, items[0].Width);
        Assert.Equal(24f, items[0].Height);
        Assert.Equal(Color.Crimson, items[0].Colour);
        Assert.Empty(frame.GetPopulatedPasses());
    }

    [Fact]
    public void Render_SpriteWithDefaults_SubmitsDefaultDimensionsAndColour()
    {
        SpriteRenderSystem system = new();
        World world = CreateStartedWorld(system);
        Entity entity = new();
        entity.AddComponent(new TransformComponent
        {
            Position = new Vector2(10f, 20f),
            Scale = Vector2.One
        });
        entity.AddComponent(new SpriteComponent
        {
            Asset = new AssetPath("player")
        });
        world.AddEntity(entity);
        world.Update();

        RenderQueue queue = new();

        world.Render(new RenderPipelineContext(CreateFrame(), queue));

        SpriteRenderItem item = Assert.Single(queue.GetItems<SpriteRenderItem>().ToArray());

        Assert.Equal(1f, item.Width);
        Assert.Equal(1f, item.Height);
        Assert.Equal(Color.White, item.Colour);
    }

    [Fact]
    public void Render_InactiveSprite_SubmitsNothing()
    {
        SpriteRenderSystem system = new();
        World world = CreateStartedWorld(system);
        Entity entity = new();
        entity.AddComponent(new TransformComponent
        {
            Position = new Vector2(10f, 20f),
            Scale = Vector2.One
        });
        entity.AddComponent(new SpriteComponent
        {
            Asset = new AssetPath("player"),
            Width = 16f,
            Height = 16f,
            Active = false
        });
        world.AddEntity(entity);
        world.Update();

        RenderFrame frame = CreateFrame();
        RenderQueue queue = new();

        world.Render(new RenderPipelineContext(frame, queue));

        Assert.True(queue.GetItems<SpriteRenderItem>().IsEmpty);
        Assert.Empty(frame.GetPopulatedPasses());
    }

    [Fact]
    public void Render_EntityWithoutTransform_SubmitsNothing()
    {
        SpriteRenderSystem system = new();
        World world = CreateStartedWorld(system);
        Entity entity = new();
        entity.AddComponent(new SpriteComponent
        {
            Asset = new AssetPath("player"),
            Width = 16f,
            Height = 16f
        });
        world.AddEntity(entity);
        world.Update();

        RenderFrame frame = CreateFrame();
        RenderQueue queue = new();

        world.Render(new RenderPipelineContext(frame, queue));

        Assert.True(queue.GetItems<SpriteRenderItem>().IsEmpty);
        Assert.Empty(frame.GetPopulatedPasses());
    }

    [Fact]
    public void Render_InvalidSpriteSize_SubmitsNothing()
    {
        SpriteRenderSystem system = new();
        World world = CreateStartedWorld(system);
        Entity entity = new();
        entity.AddComponent(new TransformComponent
        {
            Position = new Vector2(10f, 20f),
            Scale = Vector2.One
        });
        entity.AddComponent(new SpriteComponent
        {
            Asset = new AssetPath("player"),
            Width = -1f,
            Height = 16f
        });
        world.AddEntity(entity);
        world.Update();

        RenderFrame frame = CreateFrame();
        RenderQueue queue = new();

        world.Render(new RenderPipelineContext(frame, queue));

        Assert.True(queue.GetItems<SpriteRenderItem>().IsEmpty);
        Assert.Empty(frame.GetPopulatedPasses());
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
