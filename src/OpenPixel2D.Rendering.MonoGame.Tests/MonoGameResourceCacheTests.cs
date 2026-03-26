using OpenPixel2D.Content;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.MonoGame.Tests;

public sealed class MonoGameResourceCacheTests
{
    [Fact]
    public void RegisterTexture_StoresAndReturnsTextureResource()
    {
        MonoGameResourceCache cache = new();
        FakeTextureResource texture = new(16, 24);

        cache.RegisterTexture(new TextureId("player"), texture);

        Assert.Same(texture, cache.GetRequiredTexture(new TextureId("player")));
    }

    [Fact]
    public void RegisterTexture_OverwritesExistingTextureResource()
    {
        MonoGameResourceCache cache = new();
        FakeTextureResource first = new(16, 16);
        FakeTextureResource second = new(32, 32);

        cache.RegisterTexture(new TextureId("player"), first);
        cache.RegisterTexture(new TextureId("player"), second);

        Assert.Same(second, cache.GetRequiredTexture(new TextureId("player")));
    }

    [Fact]
    public void GetRequiredTexture_ThrowsForMissingTexture()
    {
        MonoGameResourceCache cache = new();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => cache.GetRequiredTexture(new TextureId("missing")));

        Assert.Contains("missing", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RegisterFont_StoresAndReturnsFontResource()
    {
        MonoGameResourceCache cache = new();
        FakeFontResource font = new();

        cache.RegisterFont(new FontId("ui-font"), font);

        Assert.Same(font, cache.GetRequiredFont(new FontId("ui-font")));
    }

    [Fact]
    public void RegisterFont_OverwritesExistingFontResource()
    {
        MonoGameResourceCache cache = new();
        FakeFontResource first = new();
        FakeFontResource second = new();

        cache.RegisterFont(new FontId("ui-font"), first);
        cache.RegisterFont(new FontId("ui-font"), second);

        Assert.Same(second, cache.GetRequiredFont(new FontId("ui-font")));
    }

    [Fact]
    public void GetRequiredFont_ThrowsForMissingFont()
    {
        MonoGameResourceCache cache = new();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => cache.GetRequiredFont(new FontId("missing-font")));

        Assert.Contains("missing-font", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetRequiredTexture_WithContentBackedCache_LoadsRuntimeImageOnceAndCachesCreatedResource()
    {
        RecordingContentManager content = new();
        RecordingTextureFactory textureFactory = new();
        MonoGameResourceCache cache = new(content, textureFactory, new RecordingFontFactory());

        IMonoGameTextureResource first = cache.GetRequiredTexture(new TextureId("textures/player.png"));
        IMonoGameTextureResource second = cache.GetRequiredTexture(new TextureId("textures/player.png"));

        Assert.Same(first, second);
        Assert.Equal(["RuntimeImageAsset:textures/player.png"], content.LoadCalls);
        Assert.Equal(1, textureFactory.CreateCalls);
    }

    [Fact]
    public void GetRequiredTexture_ManualRegistrationOverridesContentBackedLookup()
    {
        RecordingContentManager content = new();
        RecordingTextureFactory textureFactory = new();
        MonoGameResourceCache cache = new(content, textureFactory, new RecordingFontFactory());
        FakeTextureResource manualTexture = new(16, 24);

        cache.RegisterTexture(new TextureId("textures/player.png"), manualTexture);

        IMonoGameTextureResource resolved = cache.GetRequiredTexture(new TextureId("textures/player.png"));

        Assert.Same(manualTexture, resolved);
        Assert.Empty(content.LoadCalls);
        Assert.Equal(0, textureFactory.CreateCalls);
    }

    [Fact]
    public void GetRequiredFont_WithContentBackedCache_LoadsRuntimeFontOnceAndCachesCreatedResource()
    {
        RecordingContentManager content = new();
        RecordingFontFactory fontFactory = new();
        MonoGameResourceCache cache = new(content, new RecordingTextureFactory(), fontFactory);

        IMonoGameFontResource first = cache.GetRequiredFont(new FontId("fonts/ui.ttf"));
        IMonoGameFontResource second = cache.GetRequiredFont(new FontId("fonts/ui.ttf"));

        Assert.Same(first, second);
        Assert.Equal(["RuntimeFontAsset:fonts/ui.ttf"], content.LoadCalls);
        Assert.Equal(1, fontFactory.CreateCalls);
    }

    [Fact]
    public void GetRequiredTexture_WithInvalidContentBackedId_StillThrowsMissingResourceException()
    {
        RecordingContentManager content = new();
        MonoGameResourceCache cache = new(content, new RecordingTextureFactory(), new RecordingFontFactory());

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => cache.GetRequiredTexture(new TextureId("C:/textures/player.png")));

        Assert.Contains("C:/textures/player.png", exception.Message, StringComparison.Ordinal);
        Assert.Empty(content.LoadCalls);
    }

    private sealed class FakeTextureResource : IMonoGameTextureResource
    {
        public FakeTextureResource(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }
    }

    private sealed class FakeFontResource : IMonoGameFontResource
    {
    }

    private sealed class RecordingContentManager : IContentManager
    {
        public List<string> LoadCalls { get; } = [];

        public T Load<T>(AssetPath path)
        {
            LoadCalls.Add($"{typeof(T).Name}:{path}");

            if (typeof(T) == typeof(RuntimeImageAsset))
            {
                return (T)(object)new RuntimeImageAsset(2, 3, RuntimeImagePixelFormat.Rgba32, new byte[24]);
            }

            if (typeof(T) == typeof(RuntimeFontAsset))
            {
                return (T)(object)new RuntimeFontAsset(
                    RuntimeFontFormat.TrueType,
                    new RuntimeFontFaceMetadata(
                        "Test Family",
                        "Test Font",
                        "Regular",
                        RuntimeFontStyle.Regular,
                        2048,
                        1900,
                        -500,
                        0,
                        2400),
                    [1, 2, 3, 4]);
            }

            throw new InvalidOperationException($"Unexpected asset type '{typeof(T).FullName}'.");
        }

        public bool TryLoad<T>(AssetPath path, out T asset)
        {
            asset = Load<T>(path);
            return true;
        }
    }

    private sealed class RecordingTextureFactory : IMonoGameTextureResourceFactory
    {
        public int CreateCalls { get; private set; }

        public IMonoGameTextureResource Create(RuntimeImageAsset asset)
        {
            CreateCalls++;
            return new FakeTextureResource(asset.Width, asset.Height);
        }
    }

    private sealed class RecordingFontFactory : IMonoGameFontResourceFactory
    {
        public int CreateCalls { get; private set; }

        public IMonoGameFontResource Create(RuntimeFontAsset asset)
        {
            CreateCalls++;
            return new FakeFontResource();
        }
    }
}
