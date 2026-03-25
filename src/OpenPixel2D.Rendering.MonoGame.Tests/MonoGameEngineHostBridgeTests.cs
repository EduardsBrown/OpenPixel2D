using Microsoft.Xna.Framework;
using OpenPixel2D.Engine;
using OpenPixel2D.Rendering.Abstractions;
using OpenPixel2D.Runtime;
using SystemDrawingColor = System.Drawing.Color;

namespace OpenPixel2D.Rendering.MonoGame.Tests;

public sealed class MonoGameEngineHostBridgeTests
{
    [Fact]
    public void ToEngineTimeStep_MapsElapsedAndTotalTime()
    {
        GameTime gameTime = new(TimeSpan.FromSeconds(12), TimeSpan.FromMilliseconds(16));

        EngineTimeStep timeStep = MonoGameTimeStepAdapter.ToEngineTimeStep(gameTime);

        Assert.Equal(TimeSpan.FromMilliseconds(16), timeStep.ElapsedTime);
        Assert.Equal(TimeSpan.FromSeconds(12), timeStep.TotalTime);
    }

    [Fact]
    public void Update_ForwardsConvertedTimeStepToHost()
    {
        RecordingEngineHost host = new();
        MonoGameEngineHostBridge bridge = new(host);
        GameTime gameTime = new(TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(33));

        bridge.Update(gameTime);

        Assert.Equal(new EngineTimeStep(TimeSpan.FromMilliseconds(33), TimeSpan.FromSeconds(3)), host.LastUpdateTimeStep);
        Assert.Null(host.LastRenderTimeStep);
    }

    [Fact]
    public void Draw_ForwardsConvertedTimeStepAndViewToHost()
    {
        RecordingEngineHost host = new();
        MonoGameEngineHostBridge bridge = new(host);
        GameTime gameTime = new(TimeSpan.FromSeconds(4), TimeSpan.FromMilliseconds(20));
        TestRenderView view = new(
            "Main",
            320,
            180,
            new ClearOptions(ClearColour: true, Colour: SystemDrawingColor.Black));

        bridge.Draw(gameTime, view);

        Assert.Equal(new EngineTimeStep(TimeSpan.FromMilliseconds(20), TimeSpan.FromSeconds(4)), host.LastRenderTimeStep);
        Assert.Same(view, host.LastView);
    }

    private sealed class RecordingEngineHost : IEngineHost
    {
        public World World { get; } = new();

        public EngineTimeStep? LastUpdateTimeStep { get; private set; }

        public EngineTimeStep? LastRenderTimeStep { get; private set; }

        public IRenderView? LastView { get; private set; }

        public void Initialize()
        {
        }

        public void Start()
        {
        }

        public void Update(EngineTimeStep timeStep)
        {
            LastUpdateTimeStep = timeStep;
        }

        public void Render(EngineTimeStep timeStep, IRenderView? view = null)
        {
            LastRenderTimeStep = timeStep;
            LastView = view;
        }

        public void Dispose()
        {
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
}
