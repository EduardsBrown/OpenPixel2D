using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenPixel2D.Components;
using OpenPixel2D.Engine;
using OpenPixel2D.Rendering;
using OpenPixel2D.Rendering.Abstractions;
using OpenPixel2D.Rendering.MonoGame;
using OpenPixel2D.Runtime;
using NumericsVector2 = System.Numerics.Vector2;
using RenderClearOptions = OpenPixel2D.Rendering.Abstractions.ClearOptions;
using SystemDrawingColor = System.Drawing.Color;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;

namespace OpenPixel2D.Rendering.MonoGame.Smoke;

internal sealed class SmokeGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly MonoGameResourceCache _resources;
    private readonly IRenderView _view;

    private EngineHost? _host;
    private MonoGameEngineHostBridge? _bridge;

    public SmokeGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        IsMouseVisible = true;
        Window.Title = "OpenPixel2D MonoGame Backend Smoke";
        _resources = new MonoGameResourceCache();
        _view = new SmokeRenderView(
            "Smoke",
            640,
            360,
            new RenderClearOptions(ClearColour: true, Colour: SystemDrawingColor.FromArgb(255, 18, 24, 38)));
    }

    protected override void LoadContent()
    {
        Texture2D spriteTexture = CreateSpriteTexture(GraphicsDevice);
        SpriteFont font = CreateSpriteFont(GraphicsDevice);

        _resources.RegisterTexture(new TextureId("smoke-texture"), spriteTexture);
        _resources.RegisterFont(new FontId("smoke-font"), font);

        World world = CreateWorld();
        _host = new EngineHost(world, new MonoGameRenderFrameExecutor(GraphicsDevice, _resources));
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
        }

        base.Dispose(disposing);
    }

    private static World CreateWorld()
    {
        World world = new();
        world.AddSystem(new SpriteRenderSystem());
        world.AddSystem(new TextRenderSystem());

        Entity spriteEntity = new();
        spriteEntity.AddComponent(new TransformComponent
        {
            Position = new NumericsVector2(56f, 64f),
            Scale = NumericsVector2.One
        });
        spriteEntity.AddComponent(new SpriteComponent
        {
            Asset = new AssetId("smoke-texture"),
            Width = 96f,
            Height = 96f,
            Colour = SystemDrawingColor.White
        });
        world.AddEntity(spriteEntity);

        Entity textEntity = new();
        textEntity.AddComponent(new TransformComponent
        {
            Position = new NumericsVector2(56f, 190f)
        });
        textEntity.AddComponent(new TextComponent
        {
            Asset = new AssetId("smoke-font"),
            Text = "TEXT",
            Size = 4f,
            Colour = SystemDrawingColor.Gold
        });
        world.AddEntity(textEntity);

        return world;
    }

    private static Texture2D CreateSpriteTexture(GraphicsDevice graphicsDevice)
    {
        Texture2D texture = new(graphicsDevice, 16, 16);
        XnaColor[] pixels = new XnaColor[16 * 16];

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                bool border = x == 0 || y == 0 || x == 15 || y == 15;
                bool diagonal = x == y || x + y == 15;
                pixels[(y * 16) + x] = border
                    ? new XnaColor(32, 36, 48)
                    : diagonal
                        ? XnaColor.Gold
                        : new XnaColor(88, 172, 255);
            }
        }

        texture.SetData(pixels);
        return texture;
    }

    private static SpriteFont CreateSpriteFont(GraphicsDevice graphicsDevice)
    {
        GlyphDefinition[] glyphs =
        [
            new('E', ["#####", "#....", "####.", "#....", "#....", "#....", "#####"]),
            new('T', ["#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.."]),
            new('X', ["#...#", ".#.#.", "..#..", "..#..", "..#..", ".#.#.", "#...#"])
        ];

        const int glyphWidth = 5;
        const int glyphHeight = 7;
        const int padding = 1;

        int atlasWidth = padding + (glyphs.Length * (glyphWidth + padding));
        int atlasHeight = glyphHeight + (padding * 2);
        XnaColor[] pixels = new XnaColor[atlasWidth * atlasHeight];

        List<XnaRectangle> glyphBounds = new(glyphs.Length);
        List<XnaRectangle> cropping = new(glyphs.Length);
        List<char> characters = new(glyphs.Length);
        List<XnaVector3> kerning = new(glyphs.Length);

        int cursorX = padding;

        for (int i = 0; i < glyphs.Length; i++)
        {
            GlyphDefinition glyph = glyphs[i];

            for (int y = 0; y < glyphHeight; y++)
            {
                string row = glyph.Rows[y];

                for (int x = 0; x < glyphWidth; x++)
                {
                    pixels[((padding + y) * atlasWidth) + cursorX + x] = row[x] == '#'
                        ? XnaColor.White
                        : XnaColor.Transparent;
                }
            }

            glyphBounds.Add(new XnaRectangle(cursorX, padding, glyphWidth, glyphHeight));
            cropping.Add(new XnaRectangle(0, 0, glyphWidth, glyphHeight));
            characters.Add(glyph.Character);
            kerning.Add(new XnaVector3(0f, glyphWidth, 1f));
            cursorX += glyphWidth + padding;
        }

        Texture2D texture = new(graphicsDevice, atlasWidth, atlasHeight);
        texture.SetData(pixels);

        return new SpriteFont(
            texture,
            glyphBounds,
            cropping,
            characters,
            lineSpacing: glyphHeight + 1,
            spacing: 0f,
            kerning,
            defaultCharacter: null);
    }

    private readonly record struct GlyphDefinition(char Character, string[] Rows);

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
