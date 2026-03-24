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
}
