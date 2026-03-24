using System.Drawing;
using System.Numerics;
using OpenPixel2D.Components;
using OpenPixel2D.Engine;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class SpriteRenderSystemTests
{
    [Fact]
    public void Render_VisibleSpriteWithTransform_SubmitsSpriteRenderItem()
    {
        SpriteRenderSystem system = new();
        World world = CreateStartedWorld(system);
        Entity entity = new();
        entity.AddComponent(new TransformComponent
        {
            Position = new Vector2(32f, 48f),
            Scale = new Vector2(1f, 1f),
            Rotation = 0.5f
        });
        entity.AddComponent(new SpriteComponent
        {
            TextureId = new TextureId("player"),
            SourceRectangle = new RectangleF(1f, 2f, 16f, 16f),
            Colour = Color.Crimson,
            Layer = 3,
            Active = true
        });
        world.AddEntity(entity);
        world.Update();

        RenderFrame frame = CreateFrame();
        RenderQueue queue = new();

        world.Render(new TestRenderContext(frame, queue));

        ReadOnlySpan<SpriteRenderItem> items = queue.GetItems<SpriteRenderItem>();

        Assert.Equal(1, items.Length);
        Assert.Equal(new AssetId("player"), items[0].Asset);
        Assert.Equal(new Vector2(32f, 48f), items[0].Position);
        Assert.Equal(new Vector2(1f, 1f), items[0].Scale);
        Assert.Equal(0.5f, items[0].Rotation);
        Assert.Equal(16f, items[0].Width);
        Assert.Equal(16f, items[0].Height);
        Assert.Equal(Color.Crimson, items[0].Colour);
        Assert.Equal(3, items[0].Layer);
        Assert.Empty(frame.GetPopulatedPasses());
    }

    [Fact]
    public void Render_InvisibleSprite_SubmitsNothing()
    {
        SpriteRenderSystem system = new();
        World world = CreateStartedWorld(system);
        Entity entity = new();
        entity.AddComponent(new TransformComponent
        {
            Position = new Vector2(10f, 20f),
            Scale = new Vector2(1f, 1f)
        });
        entity.AddComponent(new SpriteComponent
        {
            TextureId = new TextureId("player"),
            Width = 16f,
            Height = 16f,
            Active = false
        });
        world.AddEntity(entity);
        world.Update();

        RenderFrame frame = CreateFrame();
        RenderQueue queue = new();

        world.Render(new TestRenderContext(frame, queue));

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
            TextureId = new TextureId("player"),
            Width = 16f,
            Height = 16f,
            Active = true
        });
        world.AddEntity(entity);
        world.Update();

        RenderFrame frame = CreateFrame();
        RenderQueue queue = new();

        world.Render(new TestRenderContext(frame, queue));

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
            Scale = new Vector2(1f, 1f)
        });
        entity.AddComponent(new SpriteComponent
        {
            Asset = new AssetId("player"),
            Width = -1f,
            Height = 16f,
            Active = true
        });
        world.AddEntity(entity);
        world.Update();

        RenderFrame frame = CreateFrame();
        RenderQueue queue = new();

        world.Render(new TestRenderContext(frame, queue));

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

    private sealed class TestRenderContext : IRenderContext
    {
        public TestRenderContext(IRenderFrame frame, IRenderSubmissionContext submission)
        {
            Frame = frame;
            Submission = submission;
        }

        public IRenderFrame Frame { get; }
        public IRenderSubmissionContext Submission { get; }
    }
}
