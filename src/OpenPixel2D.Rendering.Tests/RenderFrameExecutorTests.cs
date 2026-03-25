using System.Drawing;
using System.Numerics;
using OpenPixel2D.Components;
using OpenPixel2D.Engine;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class RenderFrameExecutorTests
{
    [Fact]
    public void Execute_ReceivesCompletedFrameAndViewFromCoordinator()
    {
        World world = CreateStartedWorld(new SpriteRenderSystem(), new TextRenderSystem());
        TestRenderView view = new(
            "Main",
            320,
            180,
            new ClearOptions(ClearColour: true, Colour: Color.Black));

        Entity spriteEntity = new();
        spriteEntity.AddComponent(new TransformComponent
        {
            Position = new Vector2(32f, 48f),
            Scale = Vector2.One
        });
        spriteEntity.AddComponent(new SpriteComponent
        {
            Asset = new AssetId("player"),
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
            Asset = new AssetId("ui-font"),
            Text = "Ready"
        });
        world.AddEntity(textEntity);

        world.Update();

        RenderPipelineCoordinator coordinator = new();
        IRenderCompletedFrame frame = coordinator.BuildFrame(world, view);
        RecordingFrameExecutor executor = new();

        executor.Execute(frame, view);

        Assert.Same(frame, executor.Frame);
        Assert.Same(view, executor.View);
        Assert.Equal(
            [RenderPassNames.WorldSprites, RenderPassNames.UI],
            frame.GetPopulatedPasses().Select(static pass => pass.Descriptor.Name).ToArray());
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

    private sealed class RecordingFrameExecutor : IRenderFrameExecutor
    {
        public IRenderCompletedFrame? Frame { get; private set; }

        public IRenderView? View { get; private set; }

        public void Execute(IRenderCompletedFrame frame, IRenderView? view)
        {
            Frame = frame;
            View = view;
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
