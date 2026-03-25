using System.Drawing;
using System.Numerics;
using OpenPixel2D.Components;
using OpenPixel2D.Content;
using OpenPixel2D.Engine;
using OpenPixel2D.Rendering;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Runtime.Tests;

public sealed class EngineHostTests
{
    [Fact]
    public void InitializeAndStart_DelegateToOwnedWorldLifecycle()
    {
        List<string> log = [];
        World world = new();
        Entity entity = new();
        entity.AddComponent(new SpyBehaviorComponent("behavior", log));
        world.AddEntity(entity);
        world.AddSystem(new SpyUpdateSystem("update", log));
        world.AddSystem(new SpyRenderSystem("render", log));
        EngineHost host = new(world, new RecordingFrameExecutor());

        host.Initialize();
        host.Start();

        Assert.Same(world, host.World);
        Assert.Equal(
            [
                "behavior.Initialize",
                "update.Initialize",
                "render.Initialize",
                "behavior.OnStart",
                "update.OnStart",
                "render.OnStart"
            ],
            log);
    }

    [Fact]
    public void Update_OrchestratesWorldUpdateWithoutRendering()
    {
        List<string> log = [];
        World world = new();
        Entity entity = new();
        entity.AddComponent(new SpyBehaviorComponent("behavior", log));
        world.AddEntity(entity);
        world.AddSystem(new SpyUpdateSystem("update", log));
        world.AddSystem(new SpyRenderSystem("render", log));
        RecordingFrameExecutor executor = new();
        EngineHost host = new(world, executor);
        EngineTimeStep timeStep = new(TimeSpan.FromMilliseconds(16), TimeSpan.FromSeconds(1));

        host.Initialize();
        host.Start();
        log.Clear();

        host.Update(timeStep);

        Assert.Equal(timeStep, host.LastUpdateTimeStep);
        Assert.Equal(["update.Update", "behavior.Update"], log);
        Assert.Null(executor.Frame);
    }

    [Fact]
    public void Render_BuildsFrameThroughPipelineAndPassesCompletedFrameToExecutor()
    {
        World world = CreateRenderableWorld();
        RecordingFrameExecutor executor = new();
        EngineHost host = new(world, executor);
        EngineTimeStep timeStep = new(TimeSpan.FromMilliseconds(16), TimeSpan.FromSeconds(1));
        TestRenderView view = new(
            "Main",
            320,
            180,
            new ClearOptions(ClearColour: true, Colour: Color.Black));

        host.Initialize();
        host.Start();

        host.Render(timeStep, view);

        Assert.Equal(timeStep, host.LastRenderTimeStep);
        Assert.NotNull(executor.Frame);
        Assert.Same(view, executor.View);
        Assert.Equal(
            [RenderPassNames.WorldSprites, RenderPassNames.UI],
            executor.Frame!.GetPopulatedPasses().Select(static pass => pass.Descriptor.Name).ToArray());
    }

    [Fact]
    public void Dispose_TearsDownWorldAndDisposesExecutorOnce()
    {
        List<string> log = [];
        World world = new();
        Entity entity = new();
        entity.AddComponent(new SpyBehaviorComponent("behavior", log));
        world.AddEntity(entity);
        world.AddSystem(new SpyUpdateSystem("update", log));
        world.AddSystem(new SpyRenderSystem("render", log));
        RecordingFrameExecutor executor = new();
        EngineHost host = new(world, executor);

        host.Initialize();
        host.Start();
        log.Clear();

        host.Dispose();

        Assert.Equal(
            [
                "behavior.OnDestroy",
                "update.OnDestroy",
                "render.OnDestroy",
                "behavior.Dispose",
                "update.Dispose",
                "render.Dispose"
            ],
            log);
        Assert.Equal(1, executor.DisposeCount);

        Assert.Throws<InvalidOperationException>(() => host.Dispose());
        Assert.Equal(1, executor.DisposeCount);
    }

    [Fact]
    public void InvalidLifecycleOrdering_IsForwardedThroughHost()
    {
        EngineHost host = new(new World(), new RecordingFrameExecutor());
        EngineTimeStep timeStep = new(TimeSpan.FromMilliseconds(16), TimeSpan.FromSeconds(1));
        TestRenderView view = new(
            "Main",
            320,
            180,
            new ClearOptions(ClearColour: true, Colour: Color.Black));

        Assert.Throws<InvalidOperationException>(() => host.Start());
        Assert.Throws<InvalidOperationException>(() => host.Update(timeStep));
        Assert.Throws<InvalidOperationException>(() => host.Render(timeStep, view));
    }

    [Fact]
    public void UpdateAndRender_RecordMostRecentTimeSteps()
    {
        World world = CreateRenderableWorld();
        EngineHost host = new(world, new RecordingFrameExecutor());
        EngineTimeStep firstUpdate = new(TimeSpan.FromMilliseconds(16), TimeSpan.FromSeconds(1));
        EngineTimeStep secondUpdate = new(TimeSpan.FromMilliseconds(17), TimeSpan.FromSeconds(2));
        EngineTimeStep firstRender = new(TimeSpan.FromMilliseconds(18), TimeSpan.FromSeconds(3));
        EngineTimeStep secondRender = new(TimeSpan.FromMilliseconds(19), TimeSpan.FromSeconds(4));

        host.Initialize();
        host.Start();

        host.Update(firstUpdate);
        host.Update(secondUpdate);
        host.Render(firstRender);
        host.Render(secondRender);

        Assert.Equal(secondUpdate, host.LastUpdateTimeStep);
        Assert.Equal(secondRender, host.LastRenderTimeStep);
    }

    private static World CreateRenderableWorld()
    {
        World world = new();
        world.AddSystem(new SpriteRenderSystem());
        world.AddSystem(new TextRenderSystem());

        Entity spriteEntity = new();
        spriteEntity.AddComponent(new TransformComponent
        {
            Position = new Vector2(32f, 48f),
            Scale = Vector2.One
        });
        spriteEntity.AddComponent(new SpriteComponent
        {
            Asset = new AssetPath("player"),
            Width = 16f,
            Height = 16f,
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
            Text = "Ready",
            Size = 12f,
            Colour = Color.Gold
        });
        world.AddEntity(textEntity);

        return world;
    }

    private sealed class RecordingFrameExecutor : IRenderFrameExecutor, IDisposable
    {
        public IRenderCompletedFrame? Frame { get; private set; }

        public IRenderView? View { get; private set; }

        public int DisposeCount { get; private set; }

        public void Execute(IRenderCompletedFrame frame, IRenderView? view)
        {
            Frame = frame;
            View = view;
        }

        public void Dispose()
        {
            DisposeCount++;
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

    private sealed class SpyBehaviorComponent : BehaviorComponent
    {
        private readonly string _name;
        private readonly List<string> _log;

        public SpyBehaviorComponent(string name, List<string> log)
        {
            _name = name;
            _log = log;
        }

        public override void Initialize()
        {
            _log.Add($"{_name}.Initialize");
        }

        public override void OnStart()
        {
            _log.Add($"{_name}.OnStart");
        }

        public override void Update()
        {
            _log.Add($"{_name}.Update");
        }

        public override void OnDestroy()
        {
            _log.Add($"{_name}.OnDestroy");
        }

        public override void Dispose()
        {
            _log.Add($"{_name}.Dispose");
        }
    }

    private sealed class SpyUpdateSystem : UpdateSystem
    {
        private readonly string _name;
        private readonly List<string> _log;

        public SpyUpdateSystem(string name, List<string> log)
        {
            _name = name;
            _log = log;
        }

        public override void Initialize()
        {
            _log.Add($"{_name}.Initialize");
        }

        public override void OnStart()
        {
            _log.Add($"{_name}.OnStart");
        }

        public override void Update()
        {
            _log.Add($"{_name}.Update");
        }

        public override void OnDestroy()
        {
            _log.Add($"{_name}.OnDestroy");
        }

        public override void Dispose()
        {
            _log.Add($"{_name}.Dispose");
        }
    }

    private sealed class SpyRenderSystem : RenderSystem
    {
        private readonly string _name;
        private readonly List<string> _log;

        public SpyRenderSystem(string name, List<string> log)
        {
            _name = name;
            _log = log;
        }

        public override void Initialize()
        {
            _log.Add($"{_name}.Initialize");
        }

        public override void OnStart()
        {
            _log.Add($"{_name}.OnStart");
        }

        public override void Render(IRenderContext context)
        {
            _log.Add($"{_name}.Render");
        }

        public override void OnDestroy()
        {
            _log.Add($"{_name}.OnDestroy");
        }

        public override void Dispose()
        {
            _log.Add($"{_name}.Dispose");
        }
    }
}
