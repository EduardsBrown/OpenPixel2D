using Microsoft.Xna.Framework.Graphics;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.MonoGame;

/// <summary>
/// Temporary backend-side manual resource registry for MonoGame render resources.
/// Real asset loading will replace this seam in a later backend story.
/// </summary>
public sealed class MonoGameResourceCache : IMonoGameResourceLookup
{
    private readonly Dictionary<TextureId, IMonoGameTextureResource> _textures = new();
    private readonly Dictionary<FontId, IMonoGameFontResource> _fonts = new();

    /// <summary>
    /// Registers the MonoGame texture used to resolve the supplied backend texture id.
    /// </summary>
    public void RegisterTexture(TextureId textureId, Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);
        RegisterTexture(textureId, new MonoGameTextureResource(texture));
    }

    /// <summary>
    /// Registers the MonoGame sprite font used to resolve the supplied backend font id.
    /// </summary>
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

    IMonoGameTextureResource IMonoGameResourceLookup.GetRequiredTexture(TextureId textureId)
    {
        return GetRequiredTexture(textureId);
    }

    IMonoGameFontResource IMonoGameResourceLookup.GetRequiredFont(FontId fontId)
    {
        return GetRequiredFont(fontId);
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

internal interface IMonoGameResourceLookup
{
    IMonoGameTextureResource GetRequiredTexture(TextureId textureId);

    IMonoGameFontResource GetRequiredFont(FontId fontId);
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
