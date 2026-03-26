namespace OpenPixel2D.Content;

/// <summary>
/// Backend-neutral decoded image data normalized to tightly packed RGBA8 pixels.
/// Future rendering backends can convert this directly into backend-specific texture resources.
/// </summary>
public sealed class RuntimeImageAsset
{
    private readonly byte[] _pixelData;

    public RuntimeImageAsset(int width, int height, RuntimeImagePixelFormat pixelFormat, ReadOnlySpan<byte> pixelData)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Image width must be greater than zero.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Image height must be greater than zero.");
        }

        if (pixelFormat != RuntimeImagePixelFormat.Rgba32)
        {
            throw new ArgumentOutOfRangeException(nameof(pixelFormat), "Unsupported runtime image pixel format.");
        }

        int expectedLength = checked(width * height * 4);

        if (pixelData.Length != expectedLength)
        {
            throw new ArgumentException(
                $"Pixel data length must be exactly {expectedLength} bytes for a {width}x{height} RGBA32 image.",
                nameof(pixelData));
        }

        Width = width;
        Height = height;
        PixelFormat = pixelFormat;
        _pixelData = pixelData.ToArray();
    }

    public int Width { get; }

    public int Height { get; }

    public RuntimeImagePixelFormat PixelFormat { get; }

    public ReadOnlyMemory<byte> PixelData => _pixelData;
}
