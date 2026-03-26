using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;
using OpenPixel2D.Content;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.MonoGame;

/// <summary>
/// Backend-side render resource cache for MonoGame resources.
/// Manual registration remains available for tests and bespoke ids, while normalized asset-path ids
/// can now fall through to runtime content loading and lazy backend resource creation.
/// </summary>
public sealed class MonoGameResourceCache : IMonoGameResourceLookup, IDisposable
{
    private readonly Dictionary<TextureId, IMonoGameTextureResource> _textures = new();
    private readonly Dictionary<FontId, IMonoGameFontResource> _fonts = new();
    private readonly List<IDisposable> _ownedResources = [];
    private readonly IContentManager? _content;
    private readonly IMonoGameTextureResourceFactory? _textureFactory;
    private readonly IMonoGameFontResourceFactory? _fontFactory;

    public MonoGameResourceCache()
    {
    }

    public MonoGameResourceCache(GraphicsDevice graphicsDevice, IContentManager content)
        : this(content, new MonoGameTextureResourceFactory(graphicsDevice), new MonoGameFontResourceFactory())
    {
    }

    internal MonoGameResourceCache(
        IContentManager content,
        IMonoGameTextureResourceFactory textureFactory,
        IMonoGameFontResourceFactory fontFactory)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(textureFactory);
        ArgumentNullException.ThrowIfNull(fontFactory);

        _content = content;
        _textureFactory = textureFactory;
        _fontFactory = fontFactory;
    }

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
        RegisterFontCore(fontId, new MonoGameSpriteFontResource(font));
    }

    public void Dispose()
    {
        for (int i = _ownedResources.Count - 1; i >= 0; i--)
        {
            _ownedResources[i].Dispose();
        }

        _ownedResources.Clear();
    }

    internal void RegisterTexture(TextureId textureId, IMonoGameTextureResource texture)
    {
        ArgumentNullException.ThrowIfNull(texture);
        _textures[textureId] = texture;
    }

    internal void RegisterFont(FontId fontId, IMonoGameFontResource font)
    {
        RegisterFontCore(fontId, font);
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

        if (TryCreateTextureFromContent(textureId, out texture))
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

        if (TryCreateFontFromContent(fontId, out font))
        {
            return font;
        }

        throw new InvalidOperationException($"No MonoGame font resource is registered for font id '{fontId.Value}'.");
    }

    private bool TryCreateTextureFromContent(TextureId textureId, out IMonoGameTextureResource texture)
    {
        if (_content is null || _textureFactory is null || !TryCreateAssetPath(textureId.Value, out AssetPath assetPath))
        {
            texture = default!;
            return false;
        }

        texture = _textureFactory.Create(_content.Load<RuntimeImageAsset>(assetPath));
        _textures[textureId] = texture;
        TrackOwnedResource(texture);
        return true;
    }

    private bool TryCreateFontFromContent(FontId fontId, out IMonoGameFontResource font)
    {
        if (_content is null || _fontFactory is null || !TryCreateAssetPath(fontId.Value, out AssetPath assetPath))
        {
            font = default!;
            return false;
        }

        font = _fontFactory.Create(_content.Load<RuntimeFontAsset>(assetPath));
        _fonts[fontId] = font;
        TrackOwnedResource(font);
        return true;
    }

    private void RegisterFontCore(FontId fontId, IMonoGameFontResource font)
    {
        ArgumentNullException.ThrowIfNull(font);
        _fonts[fontId] = font;
    }

    private void TrackOwnedResource(object resource)
    {
        if (resource is IDisposable disposable)
        {
            _ownedResources.Add(disposable);
        }
    }

    private static bool TryCreateAssetPath(string value, out AssetPath assetPath)
    {
        try
        {
            assetPath = new AssetPath(value);
            return true;
        }
        catch (ArgumentException)
        {
            assetPath = default;
            return false;
        }
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

internal interface IMonoGameTextureResourceFactory
{
    IMonoGameTextureResource Create(RuntimeImageAsset asset);
}

internal interface IMonoGameFontResourceFactory
{
    IMonoGameFontResource Create(RuntimeFontAsset asset);
}

internal sealed class MonoGameTextureResourceFactory : IMonoGameTextureResourceFactory
{
    private readonly GraphicsDevice _graphicsDevice;

    public MonoGameTextureResourceFactory(GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        _graphicsDevice = graphicsDevice;
    }

    public IMonoGameTextureResource Create(RuntimeImageAsset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);

        Texture2D texture = new(_graphicsDevice, asset.Width, asset.Height);
        texture.SetData(asset.PixelData.ToArray());
        return new MonoGameTextureResource(texture);
    }
}

internal sealed class MonoGameFontResourceFactory : IMonoGameFontResourceFactory
{
    public IMonoGameFontResource Create(RuntimeFontAsset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);

        FontSystem fontSystem = new();
        fontSystem.AddFont(asset.SourceData.ToArray());
        return new MonoGameRuntimeFontResource(fontSystem);
    }
}

internal sealed class MonoGameTextureResource : IMonoGameTextureResource, IDisposable
{
    public MonoGameTextureResource(Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);
        Texture = texture;
    }

    public Texture2D Texture { get; }

    public int Width => Texture.Width;

    public int Height => Texture.Height;

    public void Dispose()
    {
        Texture.Dispose();
    }
}

internal sealed class MonoGameSpriteFontResource : IMonoGameFontResource
{
    public MonoGameSpriteFontResource(SpriteFont font)
    {
        ArgumentNullException.ThrowIfNull(font);
        Font = font;
    }

    public SpriteFont Font { get; }
}

internal sealed class MonoGameRuntimeFontResource : IMonoGameFontResource, IDisposable
{
    public MonoGameRuntimeFontResource(FontSystem fontSystem)
    {
        ArgumentNullException.ThrowIfNull(fontSystem);
        FontSystem = fontSystem;
    }

    public FontSystem FontSystem { get; }

    public SpriteFontBase GetFont(float size)
    {
        int normalizedSize = Math.Max(1, (int)MathF.Round(size));
        return FontSystem.GetFont(normalizedSize);
    }

    public void Dispose()
    {
        FontSystem.Dispose();
    }
}
