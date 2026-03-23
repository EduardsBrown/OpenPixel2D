using System.Drawing;
using System.Numerics;
using OpenPixel2D.Components;
using OpenPixel2D.Engine;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class SpriteRenderSystemTests
{
    [Fact]
    public void Render_VisibleSpriteWithTransform_SubmitsSpriteRenderCommand()
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
            Origin = new Vector2(8f, 8f),
            Layer = 3,
            SortKey = 42,
            Active = true
        });
        world.AddEntity(entity);
        world.Update();

        RenderFrame frame = CreateFrame();

        world.Render(new TestRenderContext(frame));

        RenderPassBuffer pass = Assert.Single(frame.GetPopulatedPasses());
        SpriteRenderCommand command = Assert.IsType<SpriteRenderCommand>(Assert.Single(pass.Commands));

        Assert.Equal(RenderPassNames.WorldSprites, pass.Descriptor.Name);
        Assert.Equal(new TextureId("player"), command.TextureId);
        Assert.Equal(new Vector2(32f, 48f), command.Position);
        Assert.Equal(new Vector2(1f, 1f), command.Scale);
        Assert.Equal(0.5f, command.Rotation);
        Assert.Equal(new Vector2(8f, 8f), command.Origin);
        Assert.Equal(new RectangleF(1f, 2f, 16f, 16f), command.SourceRectangle);
        Assert.Equal(Color.Crimson, command.Colour);
        Assert.Equal(3, command.Metadata.Layer);
        Assert.Equal(42, command.Metadata.SortKey);
        Assert.Equal(RenderSpace.World, command.Metadata.Space);
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
            Active = false
        });
        world.AddEntity(entity);
        world.Update();

        RenderFrame frame = CreateFrame();

        world.Render(new TestRenderContext(frame));

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
            Active = true
        });
        world.AddEntity(entity);
        world.Update();

        RenderFrame frame = CreateFrame();

        world.Render(new TestRenderContext(frame));

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
        public TestRenderContext(IRenderFrame frame)
        {
            Frame = frame;
        }

        public IRenderFrame Frame { get; }
    }
}
