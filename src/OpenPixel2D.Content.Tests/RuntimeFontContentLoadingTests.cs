namespace OpenPixel2D.Content.Tests;

public sealed class RuntimeFontContentLoadingTests
{
    [Fact]
    public void Load_RuntimeFontAsset_FromTtf_LoadsSourceAndMetadata()
    {
        using TemporaryDirectory temporaryDirectory = new();
        temporaryDirectory.CopyFile(TestAssetPaths.GetPath("Fonts/CascadiaMono.ttf"), "fonts/CascadiaMono.ttf");
        ContentManager manager = new(temporaryDirectory.RootPath);

        RuntimeFontAsset asset = manager.Load<RuntimeFontAsset>(new AssetPath("fonts/CascadiaMono.ttf"));

        Assert.Equal(RuntimeFontFormat.TrueType, asset.Format);
        Assert.NotEmpty(asset.SourceData.ToArray());
        Assert.False(string.IsNullOrWhiteSpace(asset.Metadata.FamilyName));
        Assert.False(string.IsNullOrWhiteSpace(asset.Metadata.FontName));
        Assert.False(string.IsNullOrWhiteSpace(asset.Metadata.SubfamilyName));
        Assert.Equal(2048, asset.Metadata.UnitsPerEm);
        Assert.True(asset.Metadata.LineHeight > 0);
    }

    [Fact]
    public void Load_RuntimeFontAsset_ReusesCachedAsset()
    {
        using TemporaryDirectory temporaryDirectory = new();
        temporaryDirectory.CopyFile(TestAssetPaths.GetPath("Fonts/CascadiaMono.ttf"), "fonts/CascadiaMono.ttf");
        ContentManager manager = new(temporaryDirectory.RootPath);

        RuntimeFontAsset first = manager.Load<RuntimeFontAsset>(new AssetPath("fonts/CascadiaMono.ttf"));
        RuntimeFontAsset second = manager.Load<RuntimeFontAsset>(new AssetPath("fonts/CascadiaMono.ttf"));

        Assert.Same(first, second);
    }

    [Fact]
    public void TryLoad_RuntimeFontAsset_WithUnsupportedExtension_ReturnsFalse()
    {
        using TemporaryDirectory temporaryDirectory = new();
        temporaryDirectory.CopyFile(TestAssetPaths.GetPath("Fonts/CascadiaMono.ttf"), "fonts/CascadiaMono.otf");
        ContentManager manager = new(temporaryDirectory.RootPath);

        bool loaded = manager.TryLoad(new AssetPath("fonts/CascadiaMono.otf"), out RuntimeFontAsset? asset);

        Assert.False(loaded);
        Assert.Null(asset);
    }

    [Fact]
    public void Load_RuntimeFontAsset_WithUnsupportedExtension_ThrowsInvalidOperationException()
    {
        using TemporaryDirectory temporaryDirectory = new();
        temporaryDirectory.CopyFile(TestAssetPaths.GetPath("Fonts/CascadiaMono.ttf"), "fonts/CascadiaMono.otf");
        ContentManager manager = new(temporaryDirectory.RootPath);

        Assert.Throws<InvalidOperationException>(() => manager.Load<RuntimeFontAsset>(new AssetPath("fonts/CascadiaMono.otf")));
    }

    [Fact]
    public void Load_RuntimeFontAsset_WithCorruptTtf_ThrowsInvalidDataException()
    {
        using TemporaryDirectory temporaryDirectory = new();
        temporaryDirectory.CreateFile("fonts/broken.ttf", [1, 2, 3, 4, 5, 6]);
        ContentManager manager = new(temporaryDirectory.RootPath);

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => manager.Load<RuntimeFontAsset>(new AssetPath("fonts/broken.ttf")));

        Assert.Contains("fonts/broken.ttf", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DefaultRegistry_CanLoadRuntimeFontAssets()
    {
        using TemporaryDirectory temporaryDirectory = new();
        temporaryDirectory.CopyFile(TestAssetPaths.GetPath("Fonts/CascadiaMono.ttf"), "fonts/CascadiaMono.ttf");
        ContentManager manager = new(temporaryDirectory.RootPath);

        RuntimeFontAsset asset = manager.Load<RuntimeFontAsset>(new AssetPath("fonts/CascadiaMono.ttf"));

        Assert.Equal(RuntimeFontFormat.TrueType, asset.Format);
    }
}
