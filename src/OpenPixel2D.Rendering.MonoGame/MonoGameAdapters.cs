using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;
using OpenPixel2D.Rendering.Abstractions;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace OpenPixel2D.Rendering.MonoGame;

internal interface IMonoGameGraphicsDeviceAdapter
{
    void Clear(Microsoft.Xna.Framework.Graphics.ClearOptions clearOptions, XnaColor colour, float depth, int stencil);
}

internal interface IMonoGameSpriteBatchAdapter : IDisposable
{
    void Begin(MonoGameSpriteBatchSettings settings);

    void DrawSprite(IMonoGameTextureResource texture, ISpriteRenderCommand command, XnaColor colour);

    void DrawText(IMonoGameFontResource font, ITextRenderCommand command, XnaColor colour);

    void End();
}

internal sealed class MonoGameGraphicsDeviceAdapter : IMonoGameGraphicsDeviceAdapter
{
    private readonly GraphicsDevice _graphicsDevice;

    public MonoGameGraphicsDeviceAdapter(GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        _graphicsDevice = graphicsDevice;
    }

    public void Clear(Microsoft.Xna.Framework.Graphics.ClearOptions clearOptions, XnaColor colour, float depth, int stencil)
    {
        _graphicsDevice.Clear(clearOptions, colour, depth, stencil);
    }
}

internal sealed class MonoGameSpriteBatchAdapter : IMonoGameSpriteBatchAdapter
{
    private readonly SpriteBatch _spriteBatch;

    public MonoGameSpriteBatchAdapter(SpriteBatch spriteBatch)
    {
        ArgumentNullException.ThrowIfNull(spriteBatch);
        _spriteBatch = spriteBatch;
    }

    public void Begin(MonoGameSpriteBatchSettings settings)
    {
        _spriteBatch.Begin(
            settings.SortMode,
            settings.BlendState,
            settings.SamplerState,
            settings.DepthStencilState,
            settings.RasterizerState,
            effect: null,
            transformMatrix: null);
    }

    public void DrawSprite(IMonoGameTextureResource texture, ISpriteRenderCommand command, XnaColor colour)
    {
        ArgumentNullException.ThrowIfNull(texture);

        MonoGameTextureResource monoGameTexture = texture as MonoGameTextureResource
            ?? throw new InvalidOperationException("Texture resource is not backed by a MonoGame Texture2D.");

        float textureWidth = monoGameTexture.Width;
        float textureHeight = monoGameTexture.Height;

        if (textureWidth <= 0f || textureHeight <= 0f)
        {
            throw new InvalidOperationException("Texture resources must report a positive width and height.");
        }

        XnaVector2 scale = new(
            (command.Width * command.Scale.X) / textureWidth,
            (command.Height * command.Scale.Y) / textureHeight);

        _spriteBatch.Draw(
            monoGameTexture.Texture,
            new XnaVector2(command.Position.X, command.Position.Y),
            sourceRectangle: null,
            colour,
            command.Rotation,
            XnaVector2.Zero,
            scale,
            SpriteEffects.None,
            layerDepth: 0f);
    }

    public void DrawText(IMonoGameFontResource font, ITextRenderCommand command, XnaColor colour)
    {
        ArgumentNullException.ThrowIfNull(font);

        if (font is MonoGameRuntimeFontResource runtimeFont)
        {
            _spriteBatch.DrawString(
                runtimeFont.GetFont(command.Size),
                command.Text,
                new XnaVector2(command.Position.X, command.Position.Y),
                colour);
            return;
        }

        MonoGameSpriteFontResource monoGameFont = font as MonoGameSpriteFontResource
            ?? throw new InvalidOperationException("Font resource is not backed by a supported MonoGame font implementation.");

        _spriteBatch.DrawString(
            monoGameFont.Font,
            command.Text,
            new XnaVector2(command.Position.X, command.Position.Y),
            colour,
            rotation: 0f,
            origin: XnaVector2.Zero,
            scale: command.Size,
            effects: SpriteEffects.None,
            layerDepth: 0f);
    }

    public void End()
    {
        _spriteBatch.End();
    }

    public void Dispose()
    {
        _spriteBatch.Dispose();
    }
}
