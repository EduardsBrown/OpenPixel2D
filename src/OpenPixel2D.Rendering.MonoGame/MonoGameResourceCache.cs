using Microsoft.Xna.Framework.Graphics;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.MonoGame;

internal sealed class MonoGameResourceCache
{
    private readonly Dictionary<TextureId, IMonoGameTextureResource> _textures = new();
    private readonly Dictionary<FontId, IMonoGameFontResource> _fonts = new();

    public void RegisterTexture(TextureId textureId, Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);
        RegisterTexture(textureId, new MonoGameTextureResource(texture));
    }

    public void RegisterFont(FontId fontId, SpriteFont font)
    {
        ArgumentNullException.ThrowIfNull(font);
        RegisterFont(fontId, new MonoGameFontResource(font));
    }

    internal void RegisterTexture(TextureId textureId, IMonoGameTextureResource texture)
    {
        ArgumentNullException.ThrowIfNull(texture);
        _textures[textureId] = texture;
    }

    internal void RegisterFont(FontId fontId, IMonoGameFontResource font)
    {
        ArgumentNullException.ThrowIfNull(font);
        _fonts[fontId] = font;
    }

    internal IMonoGameTextureResource GetRequiredTexture(TextureId textureId)
    {
        if (_textures.TryGetValue(textureId, out IMonoGameTextureResource? texture))
        {
            return texture;
        }

        throw new InvalidOperationException($"No MonoGame texture resource is registered for texture id '{textureId.Value}'.");
    }

    internal IMonoGameFontResource GetRequiredFont(FontId fontId)
    {
        if (_fonts.TryGetValue(fontId, out IMonoGameFontResource? font))
        {
            return font;
        }

        throw new InvalidOperationException($"No MonoGame font resource is registered for font id '{fontId.Value}'.");
    }
}

internal interface IMonoGameTextureResource
{
    int Width { get; }

    int Height { get; }
}

internal interface IMonoGameFontResource
{
}

internal sealed class MonoGameTextureResource : IMonoGameTextureResource
{
    public MonoGameTextureResource(Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);
        Texture = texture;
    }

    public Texture2D Texture { get; }

    public int Width => Texture.Width;

    public int Height => Texture.Height;
}

internal sealed class MonoGameFontResource : IMonoGameFontResource
{
    public MonoGameFontResource(SpriteFont font)
    {
        ArgumentNullException.ThrowIfNull(font);
        Font = font;
    }

    public SpriteFont Font { get; }
}
