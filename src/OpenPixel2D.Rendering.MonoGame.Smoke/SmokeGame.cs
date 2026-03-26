using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenPixel2D.Components;
using OpenPixel2D.Content;
using OpenPixel2D.Engine;
using OpenPixel2D.Rendering;
using OpenPixel2D.Rendering.Abstractions;
using OpenPixel2D.Rendering.MonoGame;
using OpenPixel2D.Runtime;
using NumericsVector2 = System.Numerics.Vector2;
using RenderClearOptions = OpenPixel2D.Rendering.Abstractions.ClearOptions;
using SystemDrawingColor = System.Drawing.Color;

namespace OpenPixel2D.Rendering.MonoGame.Smoke;

internal sealed class SmokeGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly IRenderView _view;

    private EngineHost? _host;
    private MonoGameEngineHostBridge? _bridge;
    private MonoGameResourceCache? _resources;

    public SmokeGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        IsMouseVisible = true;
        Window.Title = "OpenPixel2D MonoGame Backend Smoke";
        _view = new SmokeRenderView(
            "Smoke",
            1200,
            800,
            new RenderClearOptions(ClearColour: true, Colour: SystemDrawingColor.FromArgb(255, 18, 24, 38)));
    }

    protected override void LoadContent()
    {
        ContentManager content = new(AppContext.BaseDirectory);
        _resources = new MonoGameResourceCache(GraphicsDevice, content);
        World world = CreateWorld();
        _host = new EngineHost(world, new MonoGameRenderFrameExecutor(GraphicsDevice, _resources), content);
        _bridge = new MonoGameEngineHostBridge(_host);
        _host.Initialize();
        _host.Start();
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        _bridge?.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _bridge?.Draw(gameTime, _view);
        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _host?.Dispose();
            _resources?.Dispose();
        }

        base.Dispose(disposing);
    }

    private static World CreateWorld()
    {
        World world = new();
        world.AddSystem(new SpriteRenderSystem());
        world.AddSystem(new TextRenderSystem());

        TransformComponent spriteTransform = new()
        {
            Position = new NumericsVector2(56f, 64f),
            Scale = NumericsVector2.One
        };
        Entity spriteEntity = new();
        spriteEntity.AddComponent(spriteTransform);
        spriteEntity.AddComponent(new SpriteComponent
        {
            Asset = new AssetPath("textures/smoke-texture.png"),
            Width = 96f,
            Height = 96f,
            Colour = SystemDrawingColor.White
        });
        spriteEntity.AddComponent(new SmokeMotionBehaviour(spriteTransform));
        world.AddEntity(spriteEntity);

        TransformComponent textTransform = new()
        {
            Position = new NumericsVector2(56f, 190f)
        };
        Entity textEntity = new();
        textEntity.AddComponent(textTransform);
        textEntity.AddComponent(new TextComponent
        {
            Asset = new AssetPath("fonts/CascadiaMono.ttf"),
            Text = "TEXT",
            Size = 24f,
            Colour = SystemDrawingColor.Gold
        });
        textEntity.AddComponent(new SmokePulseBehaviour(textTransform));
        world.AddEntity(textEntity);

        return world;
    }

    private sealed class SmokeMotionBehaviour : BehaviorComponent
    {
        private readonly TransformComponent _transform;
        private readonly NumericsVector2 _origin;

        public SmokeMotionBehaviour(TransformComponent transform)
        {
            ArgumentNullException.ThrowIfNull(transform);
            _transform = transform;
            _origin = transform.Position;
        }

        public override void OnStart()
        {
            // Startup uses the zeroed public Time snapshot published by EngineHost before World.Start().
            _transform.Rotation = Time.DeltaTime;
        }

        public override void Update()
        {
            float totalTime = (float)Time.TotalTime;
            _transform.Position = _origin + new NumericsVector2(
                MathF.Sin(totalTime * 2f) * 18f,
                MathF.Cos(totalTime * 1.25f) * 6f);
            _transform.Rotation = totalTime * 0.35f;
        }
    }

    private sealed class SmokePulseBehaviour : BehaviorComponent
    {
        private readonly TransformComponent _transform;
        private readonly NumericsVector2 _origin;

        public SmokePulseBehaviour(TransformComponent transform)
        {
            ArgumentNullException.ThrowIfNull(transform);
            _transform = transform;
            _origin = transform.Position;
        }

        public override void OnStart()
        {
            _transform.Scale = NumericsVector2.One + new NumericsVector2((float)Time.FrameCount * 0f);
        }

        public override void Update()
        {
            float pulse = 1f + (MathF.Sin((float)Time.TotalTime * 3f) * 0.08f);
            _transform.Position = _origin + new NumericsVector2(0f, MathF.Sin((float)Time.TotalTime * 2f) * 4f);
            _transform.Scale = new NumericsVector2(pulse, pulse);
        }
    }

    private sealed class SmokeRenderView : IRenderView
    {
        public SmokeRenderView(string name, int viewportWidth, int viewportHeight, RenderClearOptions clear)
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
}
