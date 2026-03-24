using System.Drawing;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using OpenPixel2D.Rendering.Abstractions;
using RenderClearOptions = OpenPixel2D.Rendering.Abstractions.ClearOptions;

namespace OpenPixel2D.Rendering.MonoGame.Tests;

public sealed class MonoGameRenderFrameExecutorTests
{
    [Fact]
    public void Execute_SinglePassSprites_UsesOneBeginAndEndWithMappedState()
    {
        List<string> events = [];
        RecordingGraphicsDeviceAdapter graphicsDevice = new(events);
        RecordingSpriteBatchAdapter spriteBatch = new(events);
        MonoGameResourceCache cache = new();
        FakeTextureResource texture = new("player", 16, 16);
        cache.RegisterTexture(new TextureId("player"), texture);
        IRenderFrameExecutor executor = CreateExecutor(graphicsDevice, spriteBatch, cache);
        IRenderCompletedFrame frame = CreateFrame(
            new RenderPassDescriptor(RenderPassNames.WorldSprites, 0, new RenderState(SamplerMode: SamplerMode.PointClamp)),
            new TestSpriteRenderCommand(
                new RenderCommandMetadata(SortKey: 1),
                new TextureId("player"),
                new Vector2(12f, 34f),
                new Vector2(2f, 3f),
                0.25f,
                8f,
                10f,
                Color.Crimson));

        executor.Execute(frame, view: null);

        Assert.Single(spriteBatch.BeginSettings);
        Assert.Equal(SpriteSortMode.Deferred, spriteBatch.BeginSettings[0].SortMode);
        Assert.Same(SamplerState.PointClamp, spriteBatch.BeginSettings[0].SamplerState);
        Assert.Single(spriteBatch.DrawCalls);
        Assert.Same(texture, spriteBatch.DrawCalls[0].Texture);
        Assert.Equal(new TextureId("player"), spriteBatch.DrawCalls[0].Command.TextureId);
        Assert.Equal(Color.Crimson.R, spriteBatch.DrawCalls[0].Colour.R);
        Assert.Equal(Color.Crimson.G, spriteBatch.DrawCalls[0].Colour.G);
        Assert.Equal(Color.Crimson.B, spriteBatch.DrawCalls[0].Colour.B);
        Assert.Equal(Color.Crimson.A, spriteBatch.DrawCalls[0].Colour.A);
        Assert.Equal(1, spriteBatch.EndCount);
        Assert.Empty(graphicsDevice.ClearCalls);
        Assert.Equal(["Begin", "Draw:player", "End"], events);
    }

    [Fact]
    public void Execute_MultiplePasses_PreservesPassOrder()
    {
        List<string> events = [];
        RecordingGraphicsDeviceAdapter graphicsDevice = new(events);
        RecordingSpriteBatchAdapter spriteBatch = new(events);
        MonoGameResourceCache cache = new();
        cache.RegisterTexture(new TextureId("world"), new FakeTextureResource("world", 16, 16));
        cache.RegisterTexture(new TextureId("ui"), new FakeTextureResource("ui", 16, 16));
        IRenderFrameExecutor executor = CreateExecutor(graphicsDevice, spriteBatch, cache);
        RenderFrame frame = CreateFrame(
            [
                new RenderPassDescriptor(RenderPassNames.UI, 100, new RenderState(SamplerMode: SamplerMode.LinearClamp)),
                new RenderPassDescriptor(RenderPassNames.WorldSprites, 0, new RenderState(SamplerMode: SamplerMode.PointClamp))
            ]);

        frame.GetPass(RenderPassNames.UI).Submit(new TestSpriteRenderCommand(
            new RenderCommandMetadata(SortKey: 2),
            new TextureId("ui"),
            new Vector2(5f, 5f),
            Vector2.One,
            0f,
            10f,
            10f,
            Color.White));

        frame.GetPass(RenderPassNames.WorldSprites).Submit(new TestSpriteRenderCommand(
            new RenderCommandMetadata(SortKey: 1),
            new TextureId("world"),
            new Vector2(1f, 1f),
            Vector2.One,
            0f,
            10f,
            10f,
            Color.White));

        executor.Execute(frame, view: null);

        Assert.Equal(2, spriteBatch.BeginSettings.Count);
        Assert.Equal(SamplerState.PointClamp, spriteBatch.BeginSettings[0].SamplerState);
        Assert.Equal(SamplerState.LinearClamp, spriteBatch.BeginSettings[1].SamplerState);
        Assert.Equal(["world", "ui"], spriteBatch.DrawCalls.Select(static call => call.Texture.Name).ToArray());
        Assert.Equal(["Begin", "Draw:world", "End", "Begin", "Draw:ui", "End"], events);
    }

    [Fact]
    public void Execute_UsesViewClearBeforeDrawing()
    {
        List<string> events = [];
        RecordingGraphicsDeviceAdapter graphicsDevice = new(events);
        RecordingSpriteBatchAdapter spriteBatch = new(events);
        MonoGameResourceCache cache = new();
        cache.RegisterTexture(new TextureId("player"), new FakeTextureResource("player", 16, 16));
        IRenderFrameExecutor executor = CreateExecutor(graphicsDevice, spriteBatch, cache);
        IRenderCompletedFrame frame = CreateFrame(
            new RenderPassDescriptor(RenderPassNames.WorldSprites, 0, new RenderState()),
            new TestSpriteRenderCommand(
                new RenderCommandMetadata(),
                new TextureId("player"),
                new Vector2(0f, 0f),
                Vector2.One,
                0f,
                16f,
                16f,
                Color.White));
        TestRenderView view = new("Main", 320, 180, new RenderClearOptions(ClearColour: true, Colour: Color.Black));

        executor.Execute(frame, view);

        ClearCall clear = Assert.Single(graphicsDevice.ClearCalls);
        Assert.Equal(Microsoft.Xna.Framework.Graphics.ClearOptions.Target, clear.Options);
        Assert.Equal(Color.Black.R, clear.Colour.R);
        Assert.Equal(Color.Black.G, clear.Colour.G);
        Assert.Equal(Color.Black.B, clear.Colour.B);
        Assert.Equal(Color.Black.A, clear.Colour.A);
        Assert.Single(spriteBatch.BeginSettings);
        Assert.Equal(["Clear", "Begin", "Draw:player", "End"], events);
    }

    [Fact]
    public void Execute_SkipsUnsupportedTextCommandsAndContinuesDrawingSprites()
    {
        List<string> events = [];
        RecordingGraphicsDeviceAdapter graphicsDevice = new(events);
        RecordingSpriteBatchAdapter spriteBatch = new(events);
        MonoGameResourceCache cache = new();
        cache.RegisterTexture(new TextureId("player"), new FakeTextureResource("player", 16, 16));
        IRenderFrameExecutor executor = CreateExecutor(graphicsDevice, spriteBatch, cache);
        RenderFrame frame = CreateFrame(new RenderPassDescriptor(RenderPassNames.UI, 0, new RenderState()));
        IRenderPassWriter pass = frame.GetPass(RenderPassNames.UI);

        pass.Submit(new TestTextRenderCommand(
            new RenderCommandMetadata(),
            new FontId("ui-font"),
            "Ready",
            new Vector2(1f, 1f),
            12f,
            Color.Gold));
        pass.Submit(new TestSpriteRenderCommand(
            new RenderCommandMetadata(),
            new TextureId("player"),
            new Vector2(2f, 3f),
            Vector2.One,
            0f,
            16f,
            16f,
            Color.White));

        executor.Execute(frame, view: null);

        Assert.Single(spriteBatch.BeginSettings);
        Assert.Single(spriteBatch.DrawCalls);
        Assert.Equal(new TextureId("player"), spriteBatch.DrawCalls[0].Command.TextureId);
        Assert.Equal(1, spriteBatch.EndCount);
        Assert.Equal(["Begin", "Draw:player", "End"], events);
    }

    [Fact]
    public void Execute_SkipsStateOverrideSpritesAndDrawsRemainingSprites()
    {
        List<string> events = [];
        RecordingGraphicsDeviceAdapter graphicsDevice = new(events);
        RecordingSpriteBatchAdapter spriteBatch = new(events);
        MonoGameResourceCache cache = new();
        cache.RegisterTexture(new TextureId("player"), new FakeTextureResource("player", 16, 16));
        cache.RegisterTexture(new TextureId("enemy"), new FakeTextureResource("enemy", 16, 16));
        IRenderFrameExecutor executor = CreateExecutor(graphicsDevice, spriteBatch, cache);
        RenderFrame frame = CreateFrame(new RenderPassDescriptor(RenderPassNames.WorldSprites, 0, new RenderState()));
        IRenderPassWriter pass = frame.GetPass(RenderPassNames.WorldSprites);

        pass.Submit(new TestSpriteRenderCommand(
            new RenderCommandMetadata(StateOverride: new RenderState(SamplerMode: SamplerMode.PointWrap)),
            new TextureId("player"),
            new Vector2(0f, 0f),
            Vector2.One,
            0f,
            16f,
            16f,
            Color.White));
        pass.Submit(new TestSpriteRenderCommand(
            new RenderCommandMetadata(),
            new TextureId("enemy"),
            new Vector2(1f, 1f),
            Vector2.One,
            0f,
            16f,
            16f,
            Color.White));

        executor.Execute(frame, view: null);

        Assert.Single(spriteBatch.BeginSettings);
        Assert.Single(spriteBatch.DrawCalls);
        Assert.Equal(new TextureId("enemy"), spriteBatch.DrawCalls[0].Command.TextureId);
        Assert.Equal(["Begin", "Draw:enemy", "End"], events);
    }

    [Fact]
    public void Execute_PassWithoutExecutableSprites_DoesNotBeginSpriteBatch()
    {
        List<string> events = [];
        RecordingGraphicsDeviceAdapter graphicsDevice = new(events);
        RecordingSpriteBatchAdapter spriteBatch = new(events);
        MonoGameResourceCache cache = new();
        IRenderFrameExecutor executor = CreateExecutor(graphicsDevice, spriteBatch, cache);
        RenderFrame frame = CreateFrame(new RenderPassDescriptor(RenderPassNames.UI, 0, new RenderState()));
        frame.GetPass(RenderPassNames.UI).Submit(new TestTextRenderCommand(
            new RenderCommandMetadata(),
            new FontId("ui-font"),
            "Ready",
            new Vector2(1f, 2f),
            12f,
            Color.White));

        executor.Execute(frame, view: null);

        Assert.Empty(spriteBatch.BeginSettings);
        Assert.Empty(spriteBatch.DrawCalls);
        Assert.Equal(0, spriteBatch.EndCount);
        Assert.Empty(events);
    }

    [Fact]
    public void Execute_MissingTexture_ThrowsClearMessage()
    {
        List<string> events = [];
        RecordingGraphicsDeviceAdapter graphicsDevice = new(events);
        RecordingSpriteBatchAdapter spriteBatch = new(events);
        IRenderFrameExecutor executor = CreateExecutor(graphicsDevice, spriteBatch, new MonoGameResourceCache());
        IRenderCompletedFrame frame = CreateFrame(
            new RenderPassDescriptor(RenderPassNames.WorldSprites, 0, new RenderState()),
            new TestSpriteRenderCommand(
                new RenderCommandMetadata(),
                new TextureId("missing"),
                new Vector2(0f, 0f),
                Vector2.One,
                0f,
                16f,
                16f,
                Color.White));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => executor.Execute(frame, view: null));

        Assert.Contains("missing", exception.Message, StringComparison.Ordinal);
        Assert.Single(spriteBatch.BeginSettings);
        Assert.Equal(1, spriteBatch.EndCount);
        Assert.Equal(["Begin", "End"], events);
    }

    [Fact]
    public void Execute_RenderTargetPass_ThrowsNotSupportedException()
    {
        List<string> events = [];
        RecordingGraphicsDeviceAdapter graphicsDevice = new(events);
        RecordingSpriteBatchAdapter spriteBatch = new(events);
        MonoGameResourceCache cache = new();
        cache.RegisterTexture(new TextureId("player"), new FakeTextureResource("player", 16, 16));
        IRenderFrameExecutor executor = CreateExecutor(graphicsDevice, spriteBatch, cache);
        IRenderCompletedFrame frame = CreateFrame(
            new RenderPassDescriptor(
                RenderPassNames.WorldSprites,
                0,
                new RenderState(),
                Target: new RenderTargetId("offscreen")),
            new TestSpriteRenderCommand(
                new RenderCommandMetadata(),
                new TextureId("player"),
                new Vector2(0f, 0f),
                Vector2.One,
                0f,
                16f,
                16f,
                Color.White));

        NotSupportedException exception = Assert.Throws<NotSupportedException>(() => executor.Execute(frame, view: null));

        Assert.Contains("offscreen", exception.Message, StringComparison.Ordinal);
        Assert.Empty(spriteBatch.BeginSettings);
        Assert.Empty(events);
    }

    [Fact]
    public void Execute_PassLevelClear_ClearsBackbufferBeforeDrawing()
    {
        List<string> events = [];
        RecordingGraphicsDeviceAdapter graphicsDevice = new(events);
        RecordingSpriteBatchAdapter spriteBatch = new(events);
        MonoGameResourceCache cache = new();
        cache.RegisterTexture(new TextureId("player"), new FakeTextureResource("player", 16, 16));
        IRenderFrameExecutor executor = CreateExecutor(graphicsDevice, spriteBatch, cache);
        IRenderCompletedFrame frame = CreateFrame(
            new RenderPassDescriptor(
                RenderPassNames.WorldSprites,
                0,
                new RenderState(),
                Clear: new RenderClearOptions(ClearColour: true, Colour: Color.CornflowerBlue)),
            new TestSpriteRenderCommand(
                new RenderCommandMetadata(),
                new TextureId("player"),
                new Vector2(0f, 0f),
                Vector2.One,
                0f,
                16f,
                16f,
                Color.White));

        executor.Execute(frame, view: null);

        ClearCall clear = Assert.Single(graphicsDevice.ClearCalls);
        Assert.Equal(Color.CornflowerBlue.R, clear.Colour.R);
        Assert.Single(spriteBatch.BeginSettings);
        Assert.Equal(["Clear", "Begin", "Draw:player", "End"], events);
    }

    private static MonoGameRenderFrameExecutor CreateExecutor(
        RecordingGraphicsDeviceAdapter graphicsDevice,
        RecordingSpriteBatchAdapter spriteBatch,
        MonoGameResourceCache cache)
    {
        return new MonoGameRenderFrameExecutor(graphicsDevice, spriteBatch, new MonoGameRenderStateMapper(), cache);
    }

    private static RenderFrame CreateFrame(RenderPassDescriptor descriptor, params IRenderCommand[] commands)
    {
        return CreateFrame([descriptor], commands);
    }

    private static RenderFrame CreateFrame(RenderPassDescriptor[] descriptors, params IRenderCommand[] commands)
    {
        RenderPassRegistry registry = new();

        for (int i = 0; i < descriptors.Length; i++)
        {
            registry.Register(descriptors[i]);
        }

        RenderFrame frame = new(registry);

        if (descriptors.Length > 0 && commands.Length > 0)
        {
            IRenderPassWriter pass = frame.GetPass(descriptors[0].Name);

            for (int i = 0; i < commands.Length; i++)
            {
                Submit(pass, commands[i]);
            }
        }

        return frame;
    }

    private static void Submit(IRenderPassWriter pass, IRenderCommand command)
    {
        switch (command)
        {
            case TestSpriteRenderCommand sprite:
                pass.Submit(sprite);
                break;
            case TestTextRenderCommand text:
                pass.Submit(text);
                break;
            default:
                throw new InvalidOperationException($"Unsupported test command type '{command.GetType().Name}'.");
        }
    }

    private readonly record struct TestSpriteRenderCommand(
        RenderCommandMetadata Metadata,
        TextureId TextureId,
        Vector2 Position,
        Vector2 Scale,
        float Rotation,
        float Width,
        float Height,
        Color Colour) : ISpriteRenderCommand;

    private readonly record struct TestTextRenderCommand(
        RenderCommandMetadata Metadata,
        FontId FontId,
        string Text,
        Vector2 Position,
        float Size,
        Color Colour) : ITextRenderCommand;

    private sealed class TestRenderView : IRenderView
    {
        public TestRenderView(string name, int viewportWidth, int viewportHeight, RenderClearOptions clear)
        {
            Name = name;
            ViewportWidth = viewportWidth;
            ViewportHeight = viewportHeight;
            Clear = clear;
        }

        public string Name { get; }

        public int ViewportWidth { get; }

        public int ViewportHeight { get; }

        public RenderClearOptions Clear { get; }
    }

    private sealed class RecordingGraphicsDeviceAdapter : IMonoGameGraphicsDeviceAdapter
    {
        private readonly List<string> _events;

        public RecordingGraphicsDeviceAdapter(List<string> events)
        {
            _events = events;
        }

        public List<ClearCall> ClearCalls { get; } = [];

        public void Clear(Microsoft.Xna.Framework.Graphics.ClearOptions clearOptions, Microsoft.Xna.Framework.Color colour, float depth, int stencil)
        {
            _events.Add("Clear");
            ClearCalls.Add(new ClearCall(clearOptions, colour, depth, stencil));
        }
    }

    private sealed class RecordingSpriteBatchAdapter : IMonoGameSpriteBatchAdapter
    {
        private readonly List<string> _events;

        public RecordingSpriteBatchAdapter(List<string> events)
        {
            _events = events;
        }

        public List<MonoGameSpriteBatchSettings> BeginSettings { get; } = [];

        public List<DrawCall> DrawCalls { get; } = [];

        public int EndCount { get; private set; }

        public void Begin(MonoGameSpriteBatchSettings settings)
        {
            _events.Add("Begin");
            BeginSettings.Add(settings);
        }

        public void Draw(IMonoGameTextureResource texture, ISpriteRenderCommand command, Microsoft.Xna.Framework.Color colour)
        {
            FakeTextureResource resolvedTexture = (FakeTextureResource)texture;
            _events.Add($"Draw:{resolvedTexture.Name}");
            DrawCalls.Add(new DrawCall(resolvedTexture, command, colour));
        }

        public void End()
        {
            _events.Add("End");
            EndCount++;
        }

        public void Dispose()
        {
        }
    }

    private sealed class FakeTextureResource : IMonoGameTextureResource
    {
        public FakeTextureResource(string name, int width, int height)
        {
            Name = name;
            Width = width;
            Height = height;
        }

        public string Name { get; }

        public int Width { get; }

        public int Height { get; }
    }

    private readonly record struct ClearCall(
        Microsoft.Xna.Framework.Graphics.ClearOptions Options,
        Microsoft.Xna.Framework.Color Colour,
        float Depth,
        int Stencil);

    private readonly record struct DrawCall(
        FakeTextureResource Texture,
        ISpriteRenderCommand Command,
        Microsoft.Xna.Framework.Color Colour);
}
