using System.Drawing;
using System.Numerics;
using OpenPixel2D.Components;
using OpenPixel2D.Content;
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
    public void BuildFrame_WithoutExplicitView_ExposesNullViewToProcessors()
    {
        World world = CreateStartedWorld(new SubmittingRenderSystem([]));
        RenderPassRegistry passRegistry = new();
        passRegistry.RegisterDefaultPasses();
        NullViewRecordingProcessor processor = new();
        RenderProcessorRegistry processors = new();
        processors.Register(processor);
        RenderPipelineCoordinator coordinator = new(passRegistry, processors);

        coordinator.BuildFrame(world);

        Assert.Null(processor.CapturedView);
    }

    [Fact]
    public void BuildFrame_WithExplicitView_ExposesViewToProcessors()
    {
        World world = CreateStartedWorld(new SubmittingRenderSystem([]));
        RenderPassRegistry passRegistry = new();
        passRegistry.RegisterDefaultPasses();
        ViewRecordingProcessor processor = new();
        RenderProcessorRegistry processors = new();
        processors.Register(processor);
        RenderPipelineCoordinator coordinator = new(passRegistry, processors);
        TestRenderView view = new(
            "Gameplay",
            320,
            180,
            new ClearOptions(ClearColour: true, Colour: Color.Black));

        coordinator.BuildFrame(world, view);

        Assert.Same(view, processor.CapturedView);
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
            Scale = Vector2.One
        };
        SpriteComponent sprite = new()
        {
            Asset = new AssetPath("player"),
            Width = 16f,
            Height = 16f
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
    public void BuildFrame_WorldWithSpriteAndTextSystems_ProducesBuiltInPassesInRegistryOrder()
    {
        World world = CreateStartedWorld(new SpriteRenderSystem(), new TextRenderSystem());
        TestRenderView view = new(
            "Main",
            320,
            180,
            new ClearOptions(ClearColour: true, Colour: Color.CornflowerBlue));

        Entity spriteEntity = new();
        spriteEntity.AddComponent(new TransformComponent
        {
            Position = new Vector2(32f, 48f),
            Scale = new Vector2(1f, 2f),
            Rotation = 0.5f
        });
        spriteEntity.AddComponent(new SpriteComponent
        {
            Asset = new AssetPath("player"),
            Width = 16f,
            Height = 24f,
            Colour = Color.Crimson
        });
        world.AddEntity(spriteEntity);

        Entity textEntity = new();
        textEntity.AddComponent(new TransformComponent
        {
            Position = new Vector2(5f, 6f)
        });
        textEntity.AddComponent(new TextComponent
        {
            Asset = new AssetPath("ui-font"),
            Text = "Hello",
            Size = 18f,
            Colour = Color.Gold
        });
        world.AddEntity(textEntity);

        world.Update();

        RenderPipelineCoordinator coordinator = new();

        RenderFrame frame = coordinator.BuildFrame(world, view);
        RenderPassBuffer[] passes = frame.GetPopulatedPasses().ToArray();
        IRenderCompletedFrame completedFrame = frame;
        IRenderCompletedPass[] completedPasses = completedFrame.GetPopulatedPasses().ToArray();

        Assert.Equal(2, passes.Length);
        Assert.Equal(RenderPassNames.WorldSprites, passes[0].Descriptor.Name);
        Assert.Equal(RenderPassNames.UI, passes[1].Descriptor.Name);
        Assert.Equal(RenderPassNames.WorldSprites, completedPasses[0].Descriptor.Name);
        Assert.Equal(RenderPassNames.UI, completedPasses[1].Descriptor.Name);

        SpriteRenderCommand spriteCommand = Assert.IsType<SpriteRenderCommand>(Assert.Single(passes[0].Commands));
        Assert.Equal(new TextureId("player"), spriteCommand.TextureId);
        Assert.Equal(new Vector2(32f, 48f), spriteCommand.Position);
        Assert.Equal(new Vector2(1f, 2f), spriteCommand.Scale);
        Assert.Equal(0.5f, spriteCommand.Rotation);
        Assert.Equal(16f, spriteCommand.Width);
        Assert.Equal(24f, spriteCommand.Height);
        Assert.Equal(Color.Crimson, spriteCommand.Colour);
        Assert.Equal(RenderSpace.World, spriteCommand.Metadata.Space);
        ISpriteRenderCommand spriteReadModel = Assert.IsAssignableFrom<ISpriteRenderCommand>(Assert.Single(completedPasses[0].Commands));
        Assert.Equal(new TextureId("player"), spriteReadModel.TextureId);
        Assert.Equal(new Vector2(32f, 48f), spriteReadModel.Position);
        Assert.Equal(new Vector2(1f, 2f), spriteReadModel.Scale);
        Assert.Equal(0.5f, spriteReadModel.Rotation);
        Assert.Equal(16f, spriteReadModel.Width);
        Assert.Equal(24f, spriteReadModel.Height);
        Assert.Equal(Color.Crimson, spriteReadModel.Colour);

        TextRenderCommand textCommand = Assert.IsType<TextRenderCommand>(Assert.Single(passes[1].Commands));
        Assert.Equal(new FontId("ui-font"), textCommand.FontId);
        Assert.Equal("Hello", textCommand.Text);
        Assert.Equal(new Vector2(5f, 6f), textCommand.Position);
        Assert.Equal(18f, textCommand.Size);
        Assert.Equal(Color.Gold, textCommand.Colour);
        Assert.Equal(RenderSpace.Screen, textCommand.Metadata.Space);
        ITextRenderCommand textReadModel = Assert.IsAssignableFrom<ITextRenderCommand>(Assert.Single(completedPasses[1].Commands));
        Assert.Equal(new FontId("ui-font"), textReadModel.FontId);
        Assert.Equal("Hello", textReadModel.Text);
        Assert.Equal(new Vector2(5f, 6f), textReadModel.Position);
        Assert.Equal(18f, textReadModel.Size);
        Assert.Equal(Color.Gold, textReadModel.Colour);
        Assert.Equal(new ClearOptions(ClearColour: true, Colour: Color.CornflowerBlue), view.Clear);
    }

    [Fact]
    public void BuildFrame_WithContentManager_LoadsRuntimeAssetsAndEmitsNormalizedBackendIds()
    {
        World world = CreateStartedWorld(new SpriteRenderSystem(), new TextRenderSystem());

        Entity spriteEntity = new();
        spriteEntity.AddComponent(new TransformComponent
        {
            Position = new Vector2(32f, 48f),
            Scale = Vector2.One
        });
        spriteEntity.AddComponent(new SpriteComponent
        {
            Asset = new AssetPath("textures\\player.png"),
            Width = 16f,
            Height = 16f
        });
        world.AddEntity(spriteEntity);

        Entity textEntity = new();
        textEntity.AddComponent(new TransformComponent
        {
            Position = new Vector2(5f, 6f)
        });
        textEntity.AddComponent(new TextComponent
        {
            Asset = new AssetPath("fonts\\ui.ttf"),
            Text = "Ready",
            Size = 12f
        });
        world.AddEntity(textEntity);

        world.Update();

        RecordingContentManager content = new();
        RenderPipelineCoordinator coordinator = new(content);

        RenderFrame frame = coordinator.BuildFrame(world);
        RenderPassBuffer[] passes = frame.GetPopulatedPasses().ToArray();

        SpriteRenderCommand spriteCommand = Assert.IsType<SpriteRenderCommand>(Assert.Single(passes[0].Commands));
        TextRenderCommand textCommand = Assert.IsType<TextRenderCommand>(Assert.Single(passes[1].Commands));

        Assert.Equal(["RuntimeImageAsset:textures/player.png", "RuntimeFontAsset:fonts/ui.ttf"], content.LoadCalls);
        Assert.Equal(new TextureId("textures/player.png"), spriteCommand.TextureId);
        Assert.Equal(new FontId("fonts/ui.ttf"), textCommand.FontId);
    }

    private static World CreateStartedWorld(params RenderSystem[] systems)
    {
        World world = new();

        for (int i = 0; i < systems.Length; i++)
        {
            world.AddSystem(systems[i]);
        }

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

    private sealed class NullViewRecordingProcessor : IRenderItemProcessor<TestRenderItem>
    {
        public IRenderView? CapturedView { get; private set; }

        public void Process(ReadOnlySpan<TestRenderItem> items, IRenderPipelineContext context)
        {
            CapturedView = context.View;
        }
    }

    private sealed class ViewRecordingProcessor : IRenderItemProcessor<TestRenderItem>
    {
        public IRenderView? CapturedView { get; private set; }

        public void Process(ReadOnlySpan<TestRenderItem> items, IRenderPipelineContext context)
        {
            CapturedView = context.View;
        }
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

    private sealed class RecordingContentManager : IContentManager
    {
        public List<string> LoadCalls { get; } = [];

        public T Load<T>(AssetPath path)
        {
            LoadCalls.Add($"{typeof(T).Name}:{path}");

            if (typeof(T) == typeof(RuntimeImageAsset))
            {
                return (T)(object)new RuntimeImageAsset(1, 1, RuntimeImagePixelFormat.Rgba32, [255, 255, 255, 255]);
            }

            if (typeof(T) == typeof(RuntimeFontAsset))
            {
                return (T)(object)new RuntimeFontAsset(
                    RuntimeFontFormat.TrueType,
                    new RuntimeFontFaceMetadata(
                        "Test Family",
                        "Test Font",
                        "Regular",
                        RuntimeFontStyle.Regular,
                        2048,
                        1900,
                        -500,
                        0,
                        2400),
                    [1, 2, 3, 4]);
            }

            throw new InvalidOperationException($"Unexpected asset type '{typeof(T).FullName}'.");
        }

        public bool TryLoad<T>(AssetPath path, out T asset)
        {
            asset = Load<T>(path);
            return true;
        }
    }
}
