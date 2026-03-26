using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenPixel2D.Content.Tests;

public sealed class RuntimeImageContentLoadingTests
{
    [Fact]
    public void Load_RuntimeImageAsset_FromPng_DecodesExactRgbaPixels()
    {
        using TemporaryDirectory temporaryDirectory = new();
        CreatePng(temporaryDirectory.GetPath("images/player.png"));
        ContentManager manager = new(temporaryDirectory.RootPath);

        RuntimeImageAsset asset = manager.Load<RuntimeImageAsset>(new AssetPath("images/player.png"));

        Assert.Equal(2, asset.Width);
        Assert.Equal(1, asset.Height);
        Assert.Equal(RuntimeImagePixelFormat.Rgba32, asset.PixelFormat);
        Assert.Equal(8, asset.PixelData.Length);
        Assert.Equal([255, 0, 0, 255, 0, 255, 0, 255], asset.PixelData.ToArray());
    }

    [Theory]
    [InlineData("jpg")]
    [InlineData("jpeg")]
    public void Load_RuntimeImageAsset_FromJpegFamily_Succeeds(string extension)
    {
        using TemporaryDirectory temporaryDirectory = new();
        CreateJpeg(temporaryDirectory.GetPath($"images/photo.{extension}"));
        ContentManager manager = new(temporaryDirectory.RootPath);

        RuntimeImageAsset asset = manager.Load<RuntimeImageAsset>(new AssetPath($"images/photo.{extension}"));

        Assert.Equal(3, asset.Width);
        Assert.Equal(2, asset.Height);
        Assert.Equal(RuntimeImagePixelFormat.Rgba32, asset.PixelFormat);
        Assert.Equal(24, asset.PixelData.Length);
    }

    [Fact]
    public void Load_RuntimeImageAsset_ReusesCachedAsset()
    {
        using TemporaryDirectory temporaryDirectory = new();
        CreatePng(temporaryDirectory.GetPath("images/player.png"));
        ContentManager manager = new(temporaryDirectory.RootPath);

        RuntimeImageAsset first = manager.Load<RuntimeImageAsset>(new AssetPath("images/player.png"));
        RuntimeImageAsset second = manager.Load<RuntimeImageAsset>(new AssetPath("images/player.png"));

        Assert.Same(first, second);
    }

    [Fact]
    public void TryLoad_RuntimeImageAsset_WithUnsupportedExtension_ReturnsFalse()
    {
        using TemporaryDirectory temporaryDirectory = new();
        temporaryDirectory.CreateFile("images/player.gif", "not a supported image");
        ContentManager manager = new(temporaryDirectory.RootPath);

        bool loaded = manager.TryLoad(new AssetPath("images/player.gif"), out RuntimeImageAsset? asset);

        Assert.False(loaded);
        Assert.Null(asset);
    }

    [Fact]
    public void Load_RuntimeImageAsset_WithUnsupportedExtension_ThrowsInvalidOperationException()
    {
        using TemporaryDirectory temporaryDirectory = new();
        temporaryDirectory.CreateFile("images/player.gif", "not a supported image");
        ContentManager manager = new(temporaryDirectory.RootPath);

        Assert.Throws<InvalidOperationException>(() => manager.Load<RuntimeImageAsset>(new AssetPath("images/player.gif")));
    }

    [Fact]
    public void Load_RuntimeImageAsset_WithCorruptPng_ThrowsInvalidDataException()
    {
        using TemporaryDirectory temporaryDirectory = new();
        temporaryDirectory.CreateFile("images/broken.png", [1, 2, 3, 4, 5]);
        ContentManager manager = new(temporaryDirectory.RootPath);

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => manager.Load<RuntimeImageAsset>(new AssetPath("images/broken.png")));

        Assert.Contains("images/broken.png", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DefaultRegistry_CanLoadRuntimeImageAssets()
    {
        using TemporaryDirectory temporaryDirectory = new();
        CreatePng(temporaryDirectory.GetPath("images/player.png"));
        ContentManager manager = new(temporaryDirectory.RootPath);

        RuntimeImageAsset asset = manager.Load<RuntimeImageAsset>(new AssetPath("images/player.png"));

        Assert.Equal(2, asset.Width);
    }

    private static void CreatePng(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        using Image<Rgba32> image = new(2, 1);
        image[0, 0] = new Rgba32(255, 0, 0, 255);
        image[1, 0] = new Rgba32(0, 255, 0, 255);
        image.SaveAsPng(path);
    }

    private static void CreateJpeg(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        using Image<Rgba32> image = new(3, 2);

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                image[x, y] = new Rgba32((byte)(50 + (x * 40)), (byte)(70 + (y * 30)), 180, 255);
            }
        }

        image.SaveAsJpeg(path, new JpegEncoder { Quality = 100 });
    }
}
