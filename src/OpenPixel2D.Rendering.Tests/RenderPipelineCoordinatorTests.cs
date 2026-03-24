using System.Drawing;
using System.Numerics;
using OpenPixel2D.Components;
using OpenPixel2D.Engine;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class RenderPipelineCoordinatorTests
{
    [Fact]
    public void BuildFrame_RunsWorldRenderBeforeProcessors()
    {
        List<string> calls = [];
        World world = CreateStartedWorld(new SubmittingRenderSystem(calls));
        RenderPassRegistry passRegistry = new();
        passRegistry.RegisterDefaultPasses();
        RenderProcessorRegistry processors = new();
        processors.Register(new RecordingProcessor(calls));
        RenderPipelineCoordinator coordinator = new(passRegistry, processors);

        coordinator.BuildFrame(world);

        Assert.Equal(["system.Render", "processor.Process:1"], calls);
    }

    [Fact]
    public void BuildFrame_ClearsQueueAndFrameBetweenBuilds()
    {
        SpriteRenderSystem system = new();
        World world = CreateStartedWorld(system);
        Entity entity = new();
        TransformComponent transform = new()
        {
            Position = new Vector2(10f, 20f),
            Scale = new Vector2(1f, 1f)
        };
        SpriteComponent sprite = new()
        {
            TextureId = new TextureId("player"),
            Width = 16f,
            Height = 16f,
            Active = true
        };
        entity.AddComponent(transform);
        entity.AddComponent(sprite);
        world.AddEntity(entity);
        world.Update();

        RenderPipelineCoordinator coordinator = new();

        RenderFrame firstFrame = coordinator.BuildFrame(world);

        Assert.Single(firstFrame.GetPopulatedPasses());

        sprite.Active = false;

        RenderFrame secondFrame = coordinator.BuildFrame(world);

        Assert.Empty(secondFrame.GetPopulatedPasses());
    }

    [Fact]
    public void BuildFrame_WorldWithSpriteRenderSystem_ProducesSpriteCommands()
    {
        SpriteRenderSystem system = new();
        World world = CreateStartedWorld(system);
        Entity entity = new();
        entity.AddComponent(new TransformComponent
        {
            Position = new Vector2(32f, 48f),
            Scale = new Vector2(1f, 2f),
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

        RenderPipelineCoordinator coordinator = new();

        RenderFrame frame = coordinator.BuildFrame(world);
        RenderPassBuffer pass = Assert.Single(frame.GetPopulatedPasses());
        SpriteRenderCommand command = Assert.IsType<SpriteRenderCommand>(Assert.Single(pass.Commands));

        Assert.Equal(RenderPassNames.WorldSprites, pass.Descriptor.Name);
        Assert.Equal(new TextureId("player"), command.TextureId);
        Assert.Equal(new Vector2(32f, 48f), command.Position);
        Assert.Equal(new Vector2(1f, 2f), command.Scale);
        Assert.Equal(0.5f, command.Rotation);
        Assert.Equal(new Vector2(8f, 8f), command.Origin);
        Assert.Equal(new RectangleF(1f, 2f, 16f, 16f), command.SourceRectangle);
        Assert.Equal(Color.Crimson, command.Colour);
        Assert.Equal(3, command.Metadata.Layer);
        Assert.Equal(42, command.Metadata.SortKey);
        Assert.Equal(RenderSpace.World, command.Metadata.Space);
    }

    private static World CreateStartedWorld(RenderSystem system)
    {
        World world = new();
        world.AddSystem(system);
        world.Initialize();
        world.Start();
        return world;
    }

    private readonly record struct TestRenderItem(int Id) : IRenderItem;

    private sealed class SubmittingRenderSystem : RenderSystem
    {
        private readonly List<string> _calls;

        public SubmittingRenderSystem(List<string> calls)
        {
            _calls = calls;
        }

        public override void Render(IRenderContext context)
        {
            _calls.Add("system.Render");
            context.Submission.Submit(new TestRenderItem(1));
        }
    }

    private sealed class RecordingProcessor : IRenderItemProcessor<TestRenderItem>
    {
        private readonly List<string> _calls;

        public RecordingProcessor(List<string> calls)
        {
            _calls = calls;
        }

        public void Process(ReadOnlySpan<TestRenderItem> items, IRenderPipelineContext context)
        {
            _calls.Add($"processor.Process:{items.Length}");
        }
    }
}
