namespace OpenPixel2D.Content.Tests;

public sealed class RuntimeImageAssetTests
{
    [Fact]
    public void Constructor_StoresImmutableImageState()
    {
        byte[] pixels = [255, 0, 0, 255];
        RuntimeImageAsset asset = new(1, 1, RuntimeImagePixelFormat.Rgba32, pixels);
        pixels[0] = 0;

        Assert.Equal(1, asset.Width);
        Assert.Equal(1, asset.Height);
        Assert.Equal(RuntimeImagePixelFormat.Rgba32, asset.PixelFormat);
        Assert.Equal([255, 0, 0, 255], asset.PixelData.ToArray());
    }

    [Fact]
    public void Constructor_WithInvalidPixelBufferLength_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new RuntimeImageAsset(2, 1, RuntimeImagePixelFormat.Rgba32, [255, 0, 0, 255]));
    }
}
